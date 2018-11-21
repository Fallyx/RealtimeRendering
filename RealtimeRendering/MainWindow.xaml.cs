using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Numerics;
using RealtimeRendering.Models;
using RealtimeRendering.Scenes;

namespace RealtimeRendering
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private float alpha = 0;
        private Triangle[] triangles;
        private WriteableBitmap wbmap;

        private static int winWidth = 300;
        private static int winHeight = 300;
        private float zNear = float.PositiveInfinity;
        private float zFar = float.NegativeInfinity;

        private Texture texture;
        private GBuffer gBuff;

        (Vector3 pos, Vector3 color) light = (new Vector3(0, -0.5f, 5), new Vector3(0.5f, 0.5f, 0.5f));
        Vector3 Eye = new Vector3(0, 0, 0);

        public MainWindow()
        {
            InitializeComponent();
            //triangles = CubeScene.ObjectColoredCube(new Vector3(0, 0, 1));
            //triangles = CubeScene.FaceColoredCube(CubeScene.GetPredefinedFaceColors());
            //triangles = CubeScene.VertexColoredCube(CubeScene.GetPredefinedVertexColors());
            triangles = CubeScene.TexturedCube();

            gBuff = new GBuffer(winWidth, winHeight);
            texture = new Texture(Texture.BrickImage());
            
            wbmap = new WriteableBitmap(
                winWidth,
                winHeight,
                96,
                96,
                PixelFormats.Bgr32,
                null);

            CompositionTarget.Rendering += Render;

            rtrImage.Source = wbmap;
        }

        private void Render(object sender, EventArgs e)
        {
            alphaLabel.Content = alpha;

            gBuff.ClearBuffers();

            Matrix4x4 rot = Matrix4x4.CreateFromAxisAngle(new Vector3(0, 1, 0), (float)ToRad(alpha));
            Matrix4x4 rot2 = Matrix4x4.CreateFromAxisAngle(new Vector3(1, 0, 0), (float)ToRad(alpha));
            Matrix4x4 transl = Matrix4x4.CreateTranslation(new Vector3(0, 0, 5));
            Matrix4x4 M = rot * rot2 * transl;

            foreach (Triangle triangle in triangles)
            {
                // Triangle with the transformations
                Triangle tTrans = triangle.Transform(M);
                // Triangle with the transformations and projection
                Triangle tProj = tTrans.Transform(ProjectionMatrix()).Project();

                Vector3 AB = tProj.B.Point - tProj.A.Point;
                Vector3 AC = tProj.C.Point - tProj.A.Point;

                Vector3 backface = Vector3.Cross(new Vector3(AB.X, AB.Y, 0), new Vector3(AC.X, AC.Y, 0));
                if (backface.Z < 0) continue;

                int minX = (int)Math.Max(0, Math.Min(tProj.A.Point.X, Math.Min(tProj.B.Point.X, tProj.C.Point.X)));
                int maxX = (int)Math.Min(winWidth, Math.Max(tProj.A.Point.X, Math.Max(tProj.B.Point.X, tProj.C.Point.X)));
                int minY = (int)Math.Max(0, Math.Min(tProj.A.Point.Y, Math.Min(tProj.B.Point.Y, tProj.C.Point.Y)));
                int maxY = (int)Math.Min(winHeight, Math.Max(tProj.A.Point.Y, Math.Max(tProj.B.Point.Y, tProj.C.Point.Y)));

                for (int py = minY; py < maxY; py++)
                {
                    for (int px = minX; px < maxX; px++)
                    {
                        // Matrix for Barycentric Coordinates
                        Matrix2x2 m = new Matrix2x2(AB.X, AC.X, AB.Y, AC.Y);
                        Matrix2x2 invM = Matrix2x2.Inverse(m);

                        Vector3 AP = new Vector3(px, py, 0) - tProj.A.Point;
                        Vector uv = new Vector(invM.M11 * AP.X + invM.M12 * AP.Y, invM.M21 * AP.X + invM.M22 * AP.Y);

                        if (uv.X >= 0 && uv.Y >= 0 && (uv.X + uv.Y) < 1)
                        {
                            Vector3 P = tTrans.GetPoint((float)uv.X, (float)uv.Y);
                            float depth = P.Z;

                            int buffIdx = py * winWidth + px;

                            if (gBuff.ZBuffer[buffIdx] > depth)
                            {
                                gBuff.ZBuffer[buffIdx] = depth;
                                zNear = Math.Min(zNear, depth);
                                zFar = Math.Max(zFar, depth);

                                if(!tTrans.HasTexture)
                                {
                                    Vector3 c = tTrans.GetColor((float)uv.X, (float)uv.Y);
                                    gBuff.ColorsBuffer[buffIdx] = c;
                                }
                                else
                                {
                                    Vector2 st = tTrans.GetTexture((float)uv.X, (float)uv.Y);
                                    //gBuff.ColorsBuffer[buffIdx] = texture.LookUp(st);
                                    gBuff.ColorsBuffer[buffIdx] = texture.LookUpBilinear(st);
                                }

                                gBuff.NormalBuffer[buffIdx] = tTrans.GetNormal((float)uv.X, (float)uv.Y);
                                gBuff.PosBuffer[buffIdx] = P;
                            }
                        }
                    }
                }
            }
            
            for (int y = 0; y < winHeight; y++)
            {
                for(int x = 0; x < winWidth; x++)
                {
                    int buffIdx = y * winWidth + x;
                    float f = gBuff.ZBuffer[buffIdx];
                    if (!float.IsInfinity(f))
                    {
                        DrawDifSpecColor(buffIdx);
                    }
                }
            }
            
            Int32Rect rect = new Int32Rect(0, 0, winWidth, winHeight);
            int stride = 4 * winWidth;
            wbmap.WritePixels(rect, gBuff.PixelsBuffer, stride, 0);

            alpha = (alpha + 1) % 360;
        }
        

        private void DrawColor(int buffIdx)
        {
            Vector3 pxlClr = gBuff.ColorsBuffer[buffIdx];
            SavePixel(buffIdx * 4, pxlClr);
        }

        private void DrawDifSpecColor(int buffIdx)
        {
            Vector3 clr = Vector3.Zero;

            Vector3 normal = gBuff.NormalBuffer[buffIdx];
            normal = Vector3.Normalize(normal);
            Vector3 pos = gBuff.PosBuffer[buffIdx];
            Vector3 c = gBuff.ColorsBuffer[buffIdx];

            Vector3 PL = Vector3.Normalize(light.pos - pos);
            Vector3 diff = Diffuse(pos, normal, c, PL);
            Vector3 spec = Specular(pos, normal, PL);

            Vector3 pxlClr = diff * c + spec;

            SavePixel(buffIdx * 4, pxlClr);
        }

        private void DrawZBuff(int buffIdx)
        {
            float pxlClrZ = gBuff.ZBuffer[buffIdx];

            SavePixelZ(buffIdx * 4, pxlClrZ);
        }

        private Vector3 Diffuse(Vector3 point, Vector3 normal, Vector3 color, Vector3 PL)
        {
            Vector3 diff = Vector3.Zero;
            float nL = Vector3.Dot(normal, PL);

            if(nL >= 0)
            {
                diff = (light.color * color) * nL;
            }

            return diff;
        }

        private Vector3 Specular(Vector3 point, Vector3 normal, Vector3 PL)
        {
            Vector3 spec = Vector3.Zero;
            float nL = Vector3.Dot(normal, PL);

            if (nL >= 0)
            {
                Vector3 r = 2 * Vector3.Dot(PL, normal) * normal - PL;

                Vector3 EL = Vector3.Normalize(point - Eye);

                float rEL = Vector3.Dot(Vector3.Normalize(r), EL);
                rEL = (float)Math.Pow(rEL, 50);

                spec = light.color * rEL;
            }

            return spec;
        }

        private void SavePixel(int index, Vector3 color)
        {
            Color c = Color.FromScRgb(1, color.Z, color.Y, color.X);

            gBuff.PixelsBuffer[index] = c.B;
            gBuff.PixelsBuffer[index + 1] = c.G;
            gBuff.PixelsBuffer[index + 2] = c.R;
            gBuff.PixelsBuffer[index + 3] = 255;
        }

        private void SavePixelZ(int index, float z)
        {
            int zColor = (int)((z - zFar) / (zNear - zFar) * 255) * 0x010101;
            gBuff.PixelsBuffer[index] = (byte)zColor; // b
            gBuff.PixelsBuffer[index + 1] = (byte)zColor; // g
            gBuff.PixelsBuffer[index + 2] = (byte)zColor; // r
            gBuff.PixelsBuffer[index + 3] = 255;
        }

        public double ToRad(double angle)
        {
            return (Math.PI / 180) * angle;
        }

        private static Matrix4x4 ProjectionMatrix()
        {
            return Matrix4x4.Transpose(new Matrix4x4 (winWidth, 0, winWidth / 2f, 0,
                                0, winWidth, winHeight / 2f, 0,
                                0, 0, 0, 0,
                                0, 0, 1, 0));
        }
    }
}
