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
        private float[] zBuff1d;

        private static int winWidth = 440;
        private static int winHeight = 440;
        private float zNear = float.PositiveInfinity;
        private float zFar = float.NegativeInfinity;

        Vector3 Eye = new Vector3(0, 0, -4);

        public MainWindow()
        {
            InitializeComponent();

            triangles = CreateTriangles();
            pixels1d = new byte[winWidth * winHeight * 4];
            zBuff1d = new float[winWidth * winHeight];
            
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

            Matrix4x4 rot = Matrix4x4.CreateFromAxisAngle(new Vector3(0, 1, 0), (float)ToRad(alpha));
            Matrix4x4 rot2 = Matrix4x4.CreateFromAxisAngle(new Vector3(1, 0, 0), (float)ToRad(alpha));
            Matrix4x4 transl = Matrix4x4.CreateTranslation(new Vector3(0, 0, 5));
            Matrix4x4 M = rot * rot2 * transl * ProjectionMatrix();

            Array.Clear(pixels1d, 0, pixels1d.Length);
            Array.Clear(zBuff1d, 0, zBuff1d.Length);

            for(int i = 0; i < zBuff1d.Length; i++)
            {
                zBuff1d[i] = float.PositiveInfinity;
            }

            foreach(Triangle triangle in triangles)
            {
                Matrix4x4 mZ = rot * rot2 * transl;

                Vector3 _pA = MathHelper.Transform(triangle.PointA, mZ);
                Vector3 _pB = MathHelper.Transform(triangle.PointB, mZ);
                Vector3 _pC = MathHelper.Transform(triangle.PointC, mZ);


                Vector3 pA = MathHelper.Transform(triangle.PointA, M);
                Vector3 pB = MathHelper.Transform(triangle.PointB, M);
                Vector3 pC = MathHelper.Transform(triangle.PointC, M);

                Vector3 backface = Vector3.Cross(pB - pA, pC - pA);
                if (backface.Z < 0) continue;

                Triangle2D t2d = new Triangle2D(new Vector(pA.X, pA.Y), new Vector(pB.X, pB.Y), new Vector(pC.X, pC.Y), triangle.ColorA, triangle.ColorB, triangle.ColorC, backface.Z);

                //Vector AB = t2d.PointB - t2d.PointA;
                //Vector AC = t2d.PointC - t2d.PointA;

                Vector3 AB = pB - pA;
                Vector3 AC = pC - pA;

                Vector3 _AB = _pB - _pC;
                Vector3 _AC = _pC - _pA;

                Matrix2x2 m = new Matrix2x2(AB.X, AC.X, AB.Y, AC.Y);
                Matrix2x2 invM = Matrix2x2.Inverse(m);

                for (int py = t2d.MinY; py < t2d.MaxY; py++)
                {
                    for (int px = t2d.MinX; px < t2d.MaxX; px++)
                    {
                        //Vector AP = new Vector(px, py) - t2d.PointA;
                        Vector3 AP = new Vector3(px, py, 0) - pA;
                        Vector uv = new Vector(invM.M11 * AP.X + invM.M12 * AP.Y, invM.M21 * AP.X + invM.M22 * AP.Y);

                        if (uv.X >= 0 && uv.Y >= 0 && (uv.X + uv.Y) < 1)
                        {
                            float depth = (float)(_pA.Z + _AB.Z * uv.X + _AC.Z * uv.Y);

                            if(zBuff1d[py * winWidth + px] > depth)
                            {
                                zBuff1d[py * winWidth + px] = depth;
                                zNear = Math.Min(zNear, depth);
                                zFar = Math.Max(zFar, depth);

                                //Vector3 c = t2d.InterpolateColor((float)uv.X, (float)uv.Y);

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
                    float f = zBuff1d[y * winWidth + x];
                    if (!float.IsInfinity(f))
                    SavePixel1dZ((y * winWidth + x) * 4, f);
                }
            }

            Int32Rect rect = new Int32Rect(0, 0, winWidth, winHeight);
            int stride = 4 * winWidth;
            wbmap.WritePixels(rect, pixels1d, stride, 0);

            alpha = (alpha + 1) % 360;
        }

        private void SavePixel1d(int index, Vector3 color)
        {
            pixels1d[index] = (byte)(color.X * 255); // b
            pixels1d[index + 1] = (byte)(color.Y * 255); // g
            pixels1d[index + 2] = (byte)(color.Z * 255); // r
            pixels1d[index + 3] = 255;
        }

        private void SavePixel1dZ(int index, float z)
        {
            // (int)((z - z.Min) / (z.Max - z.Min) * 255) * 0x010101
            int zColor = (int)((z - zNear) / (zFar - zNear) * 255) * 0x010101;
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
            Vector3[] cubePts = new Vector3[]
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

            Vector3[] triangleIdx = new Vector3[]
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

            Triangle[] triangles = new Triangle[triangleIdx.Length];

            /*
            triangles[0] = new Triangle(cubePts[(int)triangleIdx[0].X], cubePts[(int)triangleIdx[0].Y], cubePts[(int)triangleIdx[0].Z], new Vector3(0, 0, 1));
            triangles[1] = new Triangle(cubePts[(int)triangleIdx[1].X], cubePts[(int)triangleIdx[1].Y], cubePts[(int)triangleIdx[1].Z], new Vector3(0, 0, 0.8f));
            triangles[2] = new Triangle(cubePts[(int)triangleIdx[2].X], cubePts[(int)triangleIdx[2].Y], cubePts[(int)triangleIdx[2].Z], new Vector3(0, 1, 0));
            triangles[3] = new Triangle(cubePts[(int)triangleIdx[3].X], cubePts[(int)triangleIdx[3].Y], cubePts[(int)triangleIdx[3].Z], new Vector3(0, 0.8f, 0));
            triangles[4] = new Triangle(cubePts[(int)triangleIdx[4].X], cubePts[(int)triangleIdx[4].Y], cubePts[(int)triangleIdx[4].Z], new Vector3(1, 0, 0));
            triangles[5] = new Triangle(cubePts[(int)triangleIdx[5].X], cubePts[(int)triangleIdx[5].Y], cubePts[(int)triangleIdx[5].Z], new Vector3(0.8f, 0, 0));
            triangles[6] = new Triangle(cubePts[(int)triangleIdx[6].X], cubePts[(int)triangleIdx[6].Y], cubePts[(int)triangleIdx[6].Z], new Vector3(0.25f, 0.8f, 0.95f));
            triangles[7] = new Triangle(cubePts[(int)triangleIdx[7].X], cubePts[(int)triangleIdx[7].Y], cubePts[(int)triangleIdx[7].Z], new Vector3(0.25f, 0.94f, 0.95f));
            triangles[8] = new Triangle(cubePts[(int)triangleIdx[8].X], cubePts[(int)triangleIdx[8].Y], cubePts[(int)triangleIdx[8].Z], new Vector3(0.25f, 0.49f, 0.95f));
            triangles[9] = new Triangle(cubePts[(int)triangleIdx[9].X], cubePts[(int)triangleIdx[9].Y], cubePts[(int)triangleIdx[9].Z], new Vector3(0.25f, 0.69f, 0.95f));
            triangles[10] = new Triangle(cubePts[(int)triangleIdx[10].X], cubePts[(int)triangleIdx[10].Y], cubePts[(int)triangleIdx[10].Z], new Vector3(0.95f, 0.25f, 0.76f));
            triangles[11] = new Triangle(cubePts[(int)triangleIdx[11].X], cubePts[(int)triangleIdx[11].Y], cubePts[(int)triangleIdx[11].Z], new Vector3(0.79f, 0.25f, 0.95f));
            */

            Vector3 normalTop = new Vector3(0, 1, 0);
            Vector3 normalBottom = new Vector3(0, -1, 0);
            Vector3 normalLeft = new Vector3(-1, 0, 0);
            Vector3 normalRight = new Vector3(1, 0, 0);
            Vector3 normalFront = new Vector3(0, 0, -1);
            Vector3 normalBack = new Vector3(0, 0, 1);

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
    }
}
