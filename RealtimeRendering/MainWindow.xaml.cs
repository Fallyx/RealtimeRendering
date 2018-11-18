using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Numerics;
using RealtimeRendering.Models;
using RealtimeRendering.Helpers;

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
        private byte[] pixels1d;
        private Vector3[] colorBuff1d;
        private float[] zBuff1d;
        private Vector3[] normalBuff1d;
        private Vector3[] posBuff1d;

        private static int winWidth = 440;
        private static int winHeight = 440;
        private float zNear = float.PositiveInfinity;
        private float zFar = float.NegativeInfinity;

        private Texture texture;

        Vector3 Light = new Vector3(0, 0, 5);
        Vector3 Eye = new Vector3(0, 0, 0);

        public MainWindow()
        {
            InitializeComponent();

            triangles = CreateTriangles();
            //triangles = CreateTriangles(new Vector3(0, 0, 1));
            //triangles = CreateTriangleTexture();
            pixels1d = new byte[winWidth * winHeight * 4];
            colorBuff1d = new Vector3[winWidth * winHeight];
            zBuff1d = new float[winWidth * winHeight];
            normalBuff1d = new Vector3[winWidth * winHeight];
            posBuff1d = new Vector3[winWidth * winHeight];
            texture = new Texture(Texture.BrickImage());
            
            wbmap = new WriteableBitmap(
                winWidth,
                winHeight,
                96,
                96,
                PixelFormats.Bgr32,
                null);

            CompositionTarget.Rendering += Render;

            Vector3 asdf = Vector3.Lerp(Vector3.Zero, Vector3.One, 0.5f);

            rtrImage.Source = wbmap;
        }

        private void Render(object sender, EventArgs e)
        {
            alphaLabel.Content = alpha;

            Array.Clear(pixels1d, 0, pixels1d.Length);
            Array.Clear(colorBuff1d, 0, colorBuff1d.Length);
            Array.Clear(normalBuff1d, 0, normalBuff1d.Length);
            Array.Clear(posBuff1d, 0, posBuff1d.Length);

            for (int i = 0; i < zBuff1d.Length; i++)
            {
                zBuff1d[i] = float.PositiveInfinity;
            }

            Matrix4x4 rot = Matrix4x4.CreateFromAxisAngle(new Vector3(0, 1, 0), (float)ToRad(alpha));
            Matrix4x4 rot2 = Matrix4x4.CreateFromAxisAngle(new Vector3(1, 0, 0), (float)ToRad(alpha));
            Matrix4x4 transl = Matrix4x4.CreateTranslation(new Vector3(0, 0, 5));
            Matrix4x4 M = rot * rot2 * transl * ProjectionMatrix();

            foreach (Triangle triangle in triangles)
            {
                //Vector3 pA = MathHelper.Transform(triangle.A.Point, M);
                //Vector3 pB = MathHelper.Transform(triangle.B.Point, M);
                //Vector3 pC = MathHelper.Transform(triangle.C.Point, M);

                Triangle t = triangle.Transform(M);

                Vector3 pA = t.A.Point;
                Vector3 pB = t.B.Point;
                Vector3 pC = t.C.Point;

                //Triangle t = new Triangle(pA, pB, pC, triangle.Normal, triangle.ColorA, triangle.ColorB, triangle.ColorC);

                Vector3 AB = pB - pA;
                Vector3 AC = pC - pA;

                Vector3 backface = Vector3.Cross(new Vector3(AB.X, AB.Y, 0), new Vector3(AC.X, AC.Y, 0));
                if (backface.Z < 0) continue;

                //t.TransformNormal(M);
                t.SetColorsH(backface.Z);

                //Vector3 AB = t.PointB - t.PointA;
                //Vector3 AC = t.PointC - t.PointA;

                Matrix2x2 m = new Matrix2x2(AB.X, AC.X, AB.Y, AC.Y);
                Matrix2x2 invM = Matrix2x2.Inverse(m);

                int minX = (int)Math.Max(0, Math.Min(pA.X, Math.Min(pB.X, pC.X)));
                int maxX = (int)Math.Min(winWidth, Math.Max(pA.X, Math.Max(pB.X, pC.X)));
                int minY = (int)Math.Max(0, Math.Min(pA.Y, Math.Min(pB.Y, pC.Y)));
                int maxY = (int)Math.Min(winHeight, Math.Max(pA.Y, Math.Max(pB.Y, pC.Y)));

                for (int py = minY; py < maxY; py++)
                {
                    for (int px = minX; px < maxX; px++)
                    {
                        //Vector2 AP = new Vector2(px - t.PointA.X, py - t.PointA.Y);
                        Vector3 AP = new Vector3(px, py, 0) - pA;
                        Vector uv = new Vector(invM.M11 * AP.X + invM.M12 * AP.Y, invM.M21 * AP.X + invM.M22 * AP.Y);

                        if (uv.X >= 0 && uv.Y >= 0 && (uv.X + uv.Y) < 1)
                        {
                            //float depth = (float)(pA.Z + AB.Z * uv.X + AC.Z * uv.Y);
                            Vector3 P = t.GetPoint((float)uv.X, (float)uv.Y);
                            float depth = P.Z;

                            int buffIdx = py * winWidth + px;

                            if (zBuff1d[buffIdx] > depth)
                            {
                                zBuff1d[buffIdx] = depth;
                                zNear = Math.Min(zNear, depth);
                                zFar = Math.Max(zFar, depth);

                                // Vector3 c = triangle.InterpolateColor((float)uv.X, (float)uv.Y);
                                if(!t.HasTexture)
                                {
                                    Vector3 c = t.GetColor((float)uv.X, (float)uv.Y, backface.Z);

                                    colorBuff1d[buffIdx] = c;
                                }
                                else
                                {
                                    Vector3 stAH = new Vector3(t.A.TextureSt / backface.Z, 1 / backface.Z);
                                    Vector3 stBH = new Vector3(t.B.TextureSt / backface.Z, 1 / backface.Z);
                                    Vector3 stCH = new Vector3(t.B.TextureSt / backface.Z, 1 / backface.Z);

                                    Vector3 stH = new Vector3(stAH.X + (float)uv.X * (stBH.X - stAH.X) + (float)uv.Y * (stCH.X - stAH.X),
                                        stAH.Y + (float)uv.X * (stBH.Y - stAH.Y) + (float)uv.Y * (stCH.Y - stAH.Y),
                                        stAH.Z + (float)uv.X * (stBH.Z - stAH.Z) + (float)uv.Y * (stCH.Z - stAH.Z));

                                    Vector2 lu = new Vector2(stH.X, stH.Y);

                                    colorBuff1d[buffIdx] = texture.LookUp(lu);
                                }
                                
                                //Vector3 norm = Vector3.TransformNormal(triangle.Normal, M); 
                                Vector3 norm = t.GetNormal((float)uv.X, (float)uv.Y);
                                //Vector3 normh = triangle.GetNormal4((float)uv.X, (float)uv.Y, M);

                                //normalBuff1d[buffIdx] = Vector3.TransformNormal(norm, M);
                                normalBuff1d[buffIdx] = norm;
                                //triangle.TransformNormal(M);
                                //normalBuff1d[buffIdx] = triangle.Normal;
                                posBuff1d[buffIdx] = P;
                                    
                                    
                                //SavePixel1d((py * winWidth + px) * 4, c);
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
                    float f = zBuff1d[buffIdx];
                    if (!float.IsInfinity(f))
                    {
                        
                        Vector3 normal = normalBuff1d[buffIdx];
                        Vector3 pos = posBuff1d[buffIdx];
                        Vector3 c = colorBuff1d[buffIdx];

                        // Vector3 diff = Diffuse(pos, normal, c);
                        Vector3 PL = Vector3.Normalize(Light - pos);
                        float diff = Math.Max(Vector3.Dot(normal, PL), 0);
                        float spec = Specular(pos, normal);

                        Vector3 pxlClr = new Vector3(0.1f, 0.1f, 0.1f) * new Vector3(c.X + diff, c.Y + diff, c.Z + diff) * new Vector3(c.X + spec, c.Y + spec, c.Z + spec);
                        

                        //Vector3 pxlClr = colorBuff1d[buffIdx];

                        SavePixel1d(buffIdx * 4, pxlClr);
                    }
                }
            }
            

            Int32Rect rect = new Int32Rect(0, 0, winWidth, winHeight);
            int stride = 4 * winWidth;
            wbmap.WritePixels(rect, pixels1d, stride, 0);

            alpha = (alpha + 1) % 360;
        }

        private Vector3 Diffuse(Vector3 point, Vector3 normal, Vector3 color)
        {
            Vector3 diff = Vector3.Zero;
            Vector3 lightClr = new Vector3(0.3f, 0.3f, 0.3f);

            Vector3 PL = Vector3.Normalize(Light - point);

            float nL = Vector3.Dot(normal, PL);

            if(nL >= 0)
            {
                Vector3 il = Vector3.Multiply(lightClr, color);
                diff = Vector3.Multiply(il, nL);
            }

            return diff;
        }

        private float Specular(Vector3 point, Vector3 normal)
        {
            Vector3 PL = Vector3.Normalize(Light - point);
            Vector3 spec = Vector3.Normalize(2 * Vector3.Dot(PL, normal) * normal - PL);
            Vector3 EL = Vector3.Normalize(Eye - point);

            return (float) Math.Pow(Math.Min(0, Vector3.Dot(spec, EL)), 50);
        }

        private void SavePixel1d(int index, Vector3 color)
        {
            Color c = Color.FromScRgb(1, color.Z, color.Y, color.X);

            pixels1d[index] = c.B; // (byte)(color.X * 255); // b
            pixels1d[index + 1] = c.G; // (byte)(color.Y * 255); // g
            pixels1d[index + 2] = c.R;  //(byte)(color.Z * 255); // r
            pixels1d[index + 3] = 255;
        }

        private void SavePixel1dZ(int index, float z)
        {
            // (int)((z - z.Min) / (z.Max - z.Min) * 255) * 0x010101
            int zColor = (int)((z - zFar) / (zNear - zFar) * 255) * 0x010101;
            pixels1d[index] = (byte)zColor; // b
            pixels1d[index + 1] = (byte)zColor; // g
            pixels1d[index + 2] = (byte)zColor; // r
            pixels1d[index + 3] = 255;
        }

        /*
        private void RenderPoly(object sender, EventArgs e)
        {
            DrawCanvas.Children.Clear();

            Matrix4x4 mRot = Matrix4x4.CreateFromAxisAngle(new Vector3(1, 0, 0), (float)ToRad(alpha)) * Matrix4x4.CreateFromAxisAngle(new Vector3(0, 1, 0), (float)ToRad(alpha));

            foreach(Triangle t in triangles)
            {
                Vector3 pA = Vector3.Transform(t.PointA, mRot);
                Vector3 pB = Vector3.Transform(t.PointB, mRot);
                Vector3 pC = Vector3.Transform(t.PointC, mRot);

                pA += new Vector3(0, 0, 5);
                pB += new Vector3(0, 0, 5);
                pC += new Vector3(0, 0, 5);

                PointCollection ptCol = new PointCollection();

                double w2 = DrawCanvas.Width / 2;
                double h2 = DrawCanvas.Width / 2;

                double x = DrawCanvas.Width * (pA.X / pA.Z) + w2;
                double y = DrawCanvas.Width * (pA.Y / pA.Z) + h2;
                Point p = new Point(x, y);
                ptCol.Add(p);

                x = DrawCanvas.Width * (pB.X / pB.Z) + w2;
                y = DrawCanvas.Width * (pB.Y / pB.Z) + h2;
                p = new Point(x, y);
                ptCol.Add(p);

                x = DrawCanvas.Width * (pC.X / pC.Z) + w2;
                y = DrawCanvas.Width * (pC.Y / pC.Z) + h2;
                p = new Point(x, y);
                ptCol.Add(p);

                Vector3 culA = new Vector3((float)ptCol[0].X, (float)ptCol[0].Y, 0);
                Vector3 culB = new Vector3((float)ptCol[1].X, (float)ptCol[1].Y, 0);
                Vector3 culC = new Vector3((float)ptCol[2].X, (float)ptCol[2].Y, 0);

                Vector3 backface = Vector3.Cross(culB - culA, culC - culA);
                if (backface.Z < 0) continue;

                Polygon poly = new Polygon();
                poly.Stroke = Brushes.Black;
                poly.Fill = t.BColor;
                poly.Points = ptCol;
                DrawCanvas.Children.Add(poly);
            }

            alpha += 0.5f;
        }
        */

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
        
        private Triangle[] CreateTriangles()
        {
            Vector3[] cubePts = GetCubeIdx();

            Vector3[] triangleIdx = GetTrianglesIdx();

            Triangle[] triangles = new Triangle[triangleIdx.Length];

            Vector3 normalTop = Vector3.UnitZ; // new Vector3(0, 1, 0);
            Vector3 normalBottom = -Vector3.UnitZ; // new Vector3(0, -1, 0);
            Vector3 normalLeft = -Vector3.UnitX; // new Vector3(-1, 0, 0);
            Vector3 normalRight = Vector3.UnitX; // new Vector3(1, 0, 0);
            Vector3 normalFront = Vector3.UnitY; // new Vector3(0, 0, -1);
            Vector3 normalBack = -Vector3.UnitY; // new Vector3(0, 0, 1);

            
            triangles[0] = new Triangle(cubePts[(int)triangleIdx[0].X], cubePts[(int)triangleIdx[0].Y], cubePts[(int)triangleIdx[0].Z], normalTop, new Vector3(0, 0, 1), new Vector3(0, 0, 0.8f), new Vector3(0, 0, 0.8f));
            triangles[1] = new Triangle(cubePts[(int)triangleIdx[1].X], cubePts[(int)triangleIdx[1].Y], cubePts[(int)triangleIdx[1].Z], normalTop, new Vector3(0, 0, 0.8f), new Vector3(0, 0, 1), new Vector3(0, 0, 0.8f));

            triangles[2] = new Triangle(cubePts[(int)triangleIdx[2].X], cubePts[(int)triangleIdx[2].Y], cubePts[(int)triangleIdx[2].Z], normalBottom, new Vector3(0, 1, 0), new Vector3(0, 0.8f, 0), new Vector3(0, 0.8f, 0));
            triangles[3] = new Triangle(cubePts[(int)triangleIdx[3].X], cubePts[(int)triangleIdx[3].Y], cubePts[(int)triangleIdx[3].Z], normalBottom, new Vector3(0, 0.8f, 0), new Vector3(0, 1f, 0), new Vector3(0, 0.8f, 0));

            triangles[4] = new Triangle(cubePts[(int)triangleIdx[4].X], cubePts[(int)triangleIdx[4].Y], cubePts[(int)triangleIdx[4].Z], normalLeft, new Vector3(1, 0, 0), new Vector3(0.8f, 0, 0), new Vector3(0.8f, 0, 0));
            triangles[5] = new Triangle(cubePts[(int)triangleIdx[5].X], cubePts[(int)triangleIdx[5].Y], cubePts[(int)triangleIdx[5].Z], normalLeft, new Vector3(0.8f, 0, 0), new Vector3(1, 0, 0), new Vector3(0.8f, 0, 0));

            triangles[6] = new Triangle(cubePts[(int)triangleIdx[6].X], cubePts[(int)triangleIdx[6].Y], cubePts[(int)triangleIdx[6].Z], normalRight, new Vector3(0.25f, 0.8f, 0.95f), new Vector3(0.25f, 0.94f, 0.95f), new Vector3(0.25f, 0.94f, 0.95f));
            triangles[7] = new Triangle(cubePts[(int)triangleIdx[7].X], cubePts[(int)triangleIdx[7].Y], cubePts[(int)triangleIdx[7].Z], normalRight, new Vector3(0.25f, 0.94f, 0.95f), new Vector3(0.25f, 0.8f, 0.95f), new Vector3(0.25f, 0.94f, 0.95f));

            triangles[8] = new Triangle(cubePts[(int)triangleIdx[8].X], cubePts[(int)triangleIdx[8].Y], cubePts[(int)triangleIdx[8].Z], normalFront, new Vector3(0.25f, 0.49f, 0.95f), new Vector3(0.25f, 0.69f, 0.95f), new Vector3(0.25f, 0.69f, 0.95f));
            triangles[9] = new Triangle(cubePts[(int)triangleIdx[9].X], cubePts[(int)triangleIdx[9].Y], cubePts[(int)triangleIdx[9].Z], normalFront, new Vector3(0.25f, 0.69f, 0.95f), new Vector3(0.25f, 0.49f, 0.95f), new Vector3(0.25f, 0.69f, 0.95f));

            triangles[10] = new Triangle(cubePts[(int)triangleIdx[10].X], cubePts[(int)triangleIdx[10].Y], cubePts[(int)triangleIdx[10].Z], normalBack, new Vector3(0.95f, 0.25f, 0.76f), new Vector3(0.79f, 0.25f, 0.95f), new Vector3(0.79f, 0.25f, 0.95f));
            triangles[11] = new Triangle(cubePts[(int)triangleIdx[11].X], cubePts[(int)triangleIdx[11].Y], cubePts[(int)triangleIdx[11].Z], normalBack, new Vector3(0.79f, 0.25f, 0.95f), new Vector3(0.95f, 0.25f, 0.76f), new Vector3(0.79f, 0.25f, 0.95f));
            

            return triangles;
        }

        private Triangle[] CreateTriangles(Vector3 color)
        {
            Vector3[] cubePts = GetCubeIdx();

            Vector3[] triangleIdx = GetTrianglesIdx();

            Triangle[] triangles = new Triangle[triangleIdx.Length];

            Vector3 normalTop = new Vector3(0, 1, 0);
            Vector3 normalBottom = new Vector3(0, -1, 0);
            Vector3 normalLeft = new Vector3(-1, 0, 0);
            Vector3 normalRight = new Vector3(1, 0, 0);
            Vector3 normalFront = new Vector3(0, 0, -1);
            Vector3 normalBack = new Vector3(0, 0, 1);

            
            triangles[0] = new Triangle(cubePts[(int)triangleIdx[0].X], cubePts[(int)triangleIdx[0].Y], cubePts[(int)triangleIdx[0].Z], normalTop, color, color, color);
            triangles[1] = new Triangle(cubePts[(int)triangleIdx[1].X], cubePts[(int)triangleIdx[1].Y], cubePts[(int)triangleIdx[1].Z], normalTop, color, color, color);

            triangles[2] = new Triangle(cubePts[(int)triangleIdx[2].X], cubePts[(int)triangleIdx[2].Y], cubePts[(int)triangleIdx[2].Z], normalBottom, color, color, color);
            triangles[3] = new Triangle(cubePts[(int)triangleIdx[3].X], cubePts[(int)triangleIdx[3].Y], cubePts[(int)triangleIdx[3].Z], normalBottom, color, color, color);

            triangles[4] = new Triangle(cubePts[(int)triangleIdx[4].X], cubePts[(int)triangleIdx[4].Y], cubePts[(int)triangleIdx[4].Z], normalLeft, color, color, color);
            triangles[5] = new Triangle(cubePts[(int)triangleIdx[5].X], cubePts[(int)triangleIdx[5].Y], cubePts[(int)triangleIdx[5].Z], normalLeft, color, color, color);

            triangles[6] = new Triangle(cubePts[(int)triangleIdx[6].X], cubePts[(int)triangleIdx[6].Y], cubePts[(int)triangleIdx[6].Z], normalRight, color, color, color);
            triangles[7] = new Triangle(cubePts[(int)triangleIdx[7].X], cubePts[(int)triangleIdx[7].Y], cubePts[(int)triangleIdx[7].Z], normalRight, color, color, color);

            triangles[8] = new Triangle(cubePts[(int)triangleIdx[8].X], cubePts[(int)triangleIdx[8].Y], cubePts[(int)triangleIdx[8].Z], normalFront, color, color, color);
            triangles[9] = new Triangle(cubePts[(int)triangleIdx[9].X], cubePts[(int)triangleIdx[9].Y], cubePts[(int)triangleIdx[9].Z], normalFront, color, color, color);

            triangles[10] = new Triangle(cubePts[(int)triangleIdx[10].X], cubePts[(int)triangleIdx[10].Y], cubePts[(int)triangleIdx[10].Z], normalBack, color, color, color);
            triangles[11] = new Triangle(cubePts[(int)triangleIdx[11].X], cubePts[(int)triangleIdx[11].Y], cubePts[(int)triangleIdx[11].Z], normalBack, color, color, color);
            
            return triangles;
        }

        private Triangle[] CreateTriangleTexture()
        {
            Vector3[] cubePts = GetCubeIdx();

            Vector3[] triangleIdx = GetTrianglesIdx();

            Triangle[] triangles = new Triangle[triangleIdx.Length];

            Vector2[] texturePts = GetTextureIdx();

            Vector3 normalTop = new Vector3(0, 1, 0);
            Vector3 normalBottom = new Vector3(0, -1, 0);
            Vector3 normalLeft = new Vector3(-1, 0, 0);
            Vector3 normalRight = new Vector3(1, 0, 0);
            Vector3 normalFront = new Vector3(0, 0, -1);
            Vector3 normalBack = new Vector3(0, 0, 1);

            triangles[0] = new Triangle(cubePts[(int)triangleIdx[0].X], cubePts[(int)triangleIdx[0].Y], cubePts[(int)triangleIdx[0].Z], normalTop, texturePts[0], texturePts[1], texturePts[2]);
            triangles[1] = new Triangle(cubePts[(int)triangleIdx[1].X], cubePts[(int)triangleIdx[1].Y], cubePts[(int)triangleIdx[1].Z], normalTop, texturePts[3], texturePts[4], texturePts[5]);

            triangles[2] = new Triangle(cubePts[(int)triangleIdx[2].X], cubePts[(int)triangleIdx[2].Y], cubePts[(int)triangleIdx[2].Z], normalBottom, texturePts[0], texturePts[1], texturePts[2]);
            triangles[3] = new Triangle(cubePts[(int)triangleIdx[3].X], cubePts[(int)triangleIdx[3].Y], cubePts[(int)triangleIdx[3].Z], normalBottom, texturePts[3], texturePts[4], texturePts[5]);

            triangles[4] = new Triangle(cubePts[(int)triangleIdx[4].X], cubePts[(int)triangleIdx[4].Y], cubePts[(int)triangleIdx[4].Z], normalLeft, texturePts[0], texturePts[1], texturePts[2]);
            triangles[5] = new Triangle(cubePts[(int)triangleIdx[5].X], cubePts[(int)triangleIdx[5].Y], cubePts[(int)triangleIdx[5].Z], normalLeft, texturePts[3], texturePts[4], texturePts[5]);

            triangles[6] = new Triangle(cubePts[(int)triangleIdx[6].X], cubePts[(int)triangleIdx[6].Y], cubePts[(int)triangleIdx[6].Z], normalRight, texturePts[0], texturePts[1], texturePts[2]);
            triangles[7] = new Triangle(cubePts[(int)triangleIdx[7].X], cubePts[(int)triangleIdx[7].Y], cubePts[(int)triangleIdx[7].Z], normalRight, texturePts[3], texturePts[4], texturePts[5]);

            triangles[8] = new Triangle(cubePts[(int)triangleIdx[8].X], cubePts[(int)triangleIdx[8].Y], cubePts[(int)triangleIdx[8].Z], normalFront, texturePts[0], texturePts[1], texturePts[2]);
            triangles[9] = new Triangle(cubePts[(int)triangleIdx[9].X], cubePts[(int)triangleIdx[9].Y], cubePts[(int)triangleIdx[9].Z], normalFront, texturePts[3], texturePts[4], texturePts[5]);

            triangles[10] = new Triangle(cubePts[(int)triangleIdx[10].X], cubePts[(int)triangleIdx[10].Y], cubePts[(int)triangleIdx[10].Z], normalBack, texturePts[0], texturePts[1], texturePts[2]);
            triangles[11] = new Triangle(cubePts[(int)triangleIdx[11].X], cubePts[(int)triangleIdx[11].Y], cubePts[(int)triangleIdx[11].Z], normalBack, texturePts[3], texturePts[4], texturePts[5]);

            return triangles;
        }

        private static Vector3[] GetCubeIdx()
        {
            return new Vector3[]
            {
                // top
                new Vector3(-1, -1, -1),
                new Vector3(1, -1, -1),
                new Vector3(1, 1, -1),
                new Vector3(-1, 1, -1),

                // bottom
                new Vector3(-1, -1, 1),
                new Vector3(1, -1, 1),
                new Vector3(1, 1, 1),
                new Vector3(-1, 1, 1)
            };
        }

        private static Vector3[] GetTrianglesIdx()
        {
            return new Vector3[]
            {
                new Vector3(0, 1, 2), // top
                new Vector3(0, 2, 3),
                new Vector3(7, 6, 5), // bottom
                new Vector3(7, 5, 4),
                new Vector3(0, 3, 7), // left
                new Vector3(0, 7, 4),
                new Vector3(2, 1, 5), // right
                new Vector3(2, 5, 6),
                new Vector3(3, 2, 6), // front
                new Vector3(3, 6, 7),
                new Vector3(1, 0, 4), // back
                new Vector3(1, 4, 5)
            };
        }

        private static Vector2[] GetTextureIdx()
        {
            return new Vector2[]
            {
                // upper right
                new Vector2(0, 0),
                new Vector2(0, 1),
                new Vector2(1, 1),

                // lower left
                new Vector2(0, 0),
                new Vector2(1, 1),
                new Vector2(1, 0)
            };
        }
    }
}
