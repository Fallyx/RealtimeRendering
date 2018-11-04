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

        private static int winWidth = 440;
        private static int winHeight = 440;

        public MainWindow()
        {
            InitializeComponent();

            triangles = CreateTriangles();
            pixels1d = new byte[winWidth * winHeight * 4];
            
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

            Matrix4x4 rot = Matrix4x4.CreateFromAxisAngle(new Vector3(0, 1, 0), (float)ToRad(alpha));
            Matrix4x4 rot2 = Matrix4x4.CreateFromAxisAngle(new Vector3(1, 0, 0), (float)ToRad(alpha));
            Matrix4x4 transl = Matrix4x4.CreateTranslation(new Vector3(0, 0, 5));
            Matrix4x4 M = rot * rot2 * transl * ProjectionMatrix();

            Array.Clear(pixels1d, 0, pixels1d.Length);

            foreach(Triangle triangle in triangles)
            {
                Vector3 pA = MathHelper.Transform(triangle.PointA, M);
                Vector3 pB = MathHelper.Transform(triangle.PointB, M);
                Vector3 pC = MathHelper.Transform(triangle.PointC, M);

                Vector3 backface = Vector3.Cross(pB - pA, pC - pA);
                if (backface.Z < 0) continue;

                Triangle2D t2d = new Triangle2D(new Vector(pA.X, pA.Y), new Vector(pB.X, pB.Y), new Vector(pC.X, pC.Y));

                Vector AB = t2d.PointB - t2d.PointA;
                Vector AC = t2d.PointC - t2d.PointA;

                Matrix2x2 m = new Matrix2x2(AB.X, AC.X, AB.Y, AC.Y);
                Matrix2x2 invM = Matrix2x2.Inverse(m);

                for (int py = 0; py < winHeight; py++)
                {
                    for (int px = 0; px < winWidth; px++)
                    {
                        Vector AP = new Vector(px, py) - t2d.PointA;
                        Vector uv = new Vector(invM.M11 * AP.X + invM.M12 * AP.Y, invM.M21 * AP.X + invM.M22 * AP.Y);

                        if (uv.X >= 0 && uv.Y >= 0 && (uv.X + uv.Y) < 1)
                        {
                            SavePixel1d((py * winWidth + px) * 4, triangle.Color);
                        }
                    }
                }
            }

            Int32Rect rect = new Int32Rect(0, 0, winWidth, winHeight);
            int stride = 4 * winWidth;
            wbmap.WritePixels(rect, pixels1d, stride, 0);

            alpha++;
        }

        private void SavePixel1d(int index, Vector3 color)
        {
            pixels1d[index] = (byte)(color.X * 255); // b
            pixels1d[index + 1] = (byte)(color.Y * 255); // g
            pixels1d[index + 2] = (byte)(color.Z * 255); // r
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

            return triangles;
        }
    }
}
