using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Numerics;
using RealtimeRendering.Models;

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
        private byte[,,] pixels;

        public MainWindow()
        {
            InitializeComponent();

            CompositionTarget.Rendering += Render2;

            wbmap = new WriteableBitmap(
                (int)rtrImage.Width,
                (int)rtrImage.Height,
                96,
                96,
                PixelFormats.Bgr32,
                null);

            triangles = CreateTriangles();

            rtrImage.Source = wbmap;
        }

        //private void Render2()
        private void Render2(object sender, EventArgs e)
        {
            pixels = new byte[(int)rtrImage.Width, (int)rtrImage.Height, 4];
            pixels1d = new byte[(int)rtrImage.Width * (int)rtrImage.Height * 4];

            Matrix4x4 mRot = Matrix4x4.CreateFromAxisAngle(new Vector3(1, 0, 0), (float)ToRad(alpha)) * Matrix4x4.CreateFromAxisAngle(new Vector3(0, 1, 0), (float)ToRad(alpha));

            foreach (Triangle triangle in triangles)
            {
                Vector3 pA = Vector3.Transform(triangle.PointA, mRot);
                Vector3 pB = Vector3.Transform(triangle.PointB, mRot);
                Vector3 pC = Vector3.Transform(triangle.PointC, mRot);

                pA = triangle.PointA + new Vector3(0, 0, 5);
                pB = triangle.PointB + new Vector3(0, 0, 5);
                pC = triangle.PointC + new Vector3(0, 0, 5);

                PointCollection ptCol = new PointCollection();

                double w2 = rtrImage.Width / 2;
                double h2 = rtrImage.Height / 2;

                double x = rtrImage.Width * (pA.X / pA.Z) + w2;
                double y = rtrImage.Width * (pA.Y / pA.Z) + h2;
                Point p = new Point(x, y);
                ptCol.Add(p);

                x = rtrImage.Width * (pB.X / pB.Z) + w2;
                y = rtrImage.Width * (pB.Y / pB.Z) + h2;
                p = new Point(x, y);
                ptCol.Add(p);

                x = rtrImage.Width * (pC.X / pC.Z) + w2;
                y = rtrImage.Width * (pC.Y / pC.Z) + h2;
                p = new Point(x, y);
                ptCol.Add(p);

                Vector3 culA = new Vector3((float)ptCol[0].X, (float)ptCol[0].Y, 0);
                Vector3 culB = new Vector3((float)ptCol[1].X, (float)ptCol[1].Y, 0);
                Vector3 culC = new Vector3((float)ptCol[2].X, (float)ptCol[2].Y, 0);

                Vector3 backface = Vector3.Cross(culB - culA, culC - culA);
                if (backface.Z < 0) continue;

                Triangle2D t2d = new Triangle2D(new Vector(ptCol[0].X, ptCol[0].Y), new Vector(ptCol[1].X, ptCol[1].Y), new Vector(ptCol[2].X, ptCol[2].Y));

                Vector AB = t2d.PointB - t2d.PointA;
                Vector AC = t2d.PointC - t2d.PointA;

                Matrix2x2 m = new Matrix2x2(AB.X, AC.X, AB.Y, AC.Y);
                Matrix2x2 invM = Matrix2x2.Inverse(m);

                for (int py = 0; py < (int)rtrImage.Height; py++)
                {
                    for (int px = 0; px < (int)rtrImage.Width; px++)
                    {
                        Vector AP = new Vector(px, py) - t2d.PointA;

                        Vector uv = new Vector(invM.M11 * AP.X + invM.M12 * AP.Y, invM.M21 * AP.X + invM.M22 * AP.Y);
                        if (uv.X >= 0 && uv.Y >= 0 && (uv.X + uv.Y) < 1)
                        {
                            SavePixel(px, py, triangle.Color);
                        }
                    }
                }
            }

            ConvertTo1d();

            Int32Rect rect = new Int32Rect(0, 0, (int)rtrImage.Width, (int)rtrImage.Height);
            int stride = 4 * (int)rtrImage.Width;
            wbmap.WritePixels(rect, pixels1d, stride, 0);

            alpha++;
        }

        private Vector CalcUV(int pX, int pY, Triangle triangle)
        {
            Vector3 AP = new Vector3(pX, pY, 1) - triangle.PointA;
            Vector3 AB = triangle.PointB - triangle.PointA;
            Vector3 AC = triangle.PointC - triangle.PointA;

            double[,] invMat = new double[2,2];
            double invCalc = 1f / (AB.X * AC.Y - AC.X * AB.Y);
            invMat[0, 0] = invCalc * AC.Y;
            invMat[0, 1] = invCalc * -AB.Y;
            invMat[1, 0] = invCalc * -AC.X;
            invMat[1, 1] = invCalc * AB.X;

            return new Vector(invMat[0, 0] * AP.X + invMat[1, 0] * AP.Y, invMat[0, 1] * AP.X + invMat[1, 1] * AP.Y);
        }

        private void SavePixel(int x, int y, Vector3 color)
        {
            pixels[x, y, 0] = (byte)(color.X * 255); // b
            pixels[x, y, 1] = (byte)(color.Y * 255); // g
            pixels[x, y, 2] = (byte)(color.Z * 255); // r
            pixels[x, y, 3] = 255;
        }

        private void ConvertTo1d()
        {
            int index = 0;

            for (int row = 0; row < (int)rtrImage.Height; row++)
            {
                for (int col = 0; col < (int)rtrImage.Width; col++)
                {
                    pixels1d[index++] = pixels[row, col, 0];
                    pixels1d[index++] = pixels[row, col, 1];
                    pixels1d[index++] = pixels[row, col, 2];
                    pixels1d[index++] = pixels[row, col, 3];
                }
            }
        }

        /*
        private void Render(object sender, EventArgs e)
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

        
        private Matrix4x4 ProjectionMatrix()
        {
            return new Matrix4x4((float)rtrImage.Width, 0, (float)rtrImage.Width / 2f, 0,
                                0, (float)rtrImage.Width, (float)rtrImage.Height / 2f, 0,
                                0, 0, 0, 0,
                                0, 0, 1, 0);
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

            for(int i = 0; i < triangleIdx.Length; i++)
            {
                triangles[i] = new Triangle(cubePts[(int)triangleIdx[i].X], cubePts[(int)triangleIdx[i].Y], cubePts[(int)triangleIdx[i].Z], new Vector3(0,0,1));
            }

            return triangles;
        }
    }
}
