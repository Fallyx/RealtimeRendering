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

namespace RealtimeRendering
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int alpha = 0;

        public MainWindow()
        {
            InitializeComponent();

            CompositionTarget.Rendering += Render;
        }

        private void Render(object sender, EventArgs e)
        {
            DrawCanvas.Children.Clear();

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

            foreach (Vector3 v in triangleIdx)
            {
                Polygon poly = new Polygon();
                poly.Stroke = Brushes.Black;

                PointCollection pC = new PointCollection();

                double w2 = DrawCanvas.Width / 2;
                double h2 = DrawCanvas.Width / 2;

                //Matrix mRot = new Matrix(Math.Cos(ToRad(alpha)), Math.Cos(ToRad(alpha + 90)), Math.Sin(ToRad(alpha)), Math.Sin(ToRad(alpha + 90)), 0, 0);
                Matrix4x4 mRot = new Matrix4x4((float)Math.Cos(ToRad(alpha)), 0, (float)Math.Cos(ToRad(alpha + 90)), 0, 0, 1, 0, 0, (float)Math.Sin(ToRad(alpha)),0, (float)Math.Sin(ToRad(alpha + 90)), 0, 0, 0, 0, 1);


                double x = DrawCanvas.Width * (cubePts[(int)v.X].X / (cubePts[(int)v.X].Z + 5)) + w2;
                double y = DrawCanvas.Width * (cubePts[(int)v.X].Y / (cubePts[(int)v.X].Z + 5)) + h2;
                Point p = new Point(x, y);
                p = mRot.Transform(p);
                pC.Add(p);


                x = DrawCanvas.Width * (cubePts[(int)v.Y].X / (cubePts[(int)v.Y].Z + 5)) + w2;
                y = DrawCanvas.Width * (cubePts[(int)v.Y].Y / (cubePts[(int)v.Y].Z + 5)) + h2;
                p = new Point(x, y);
                p = mRot.Transform(p); 
                pC.Add(p);

                x = DrawCanvas.Width * (cubePts[(int)v.Z].X / (cubePts[(int)v.Z].Z + 5)) + w2;
                y = DrawCanvas.Width * (cubePts[(int)v.Z].Y / (cubePts[(int)v.Z].Z + 5)) + h2;
                p = new Point(x, y);
                p = mRot.Transform(p);
                pC.Add(p);

                poly.Points = pC;
                DrawCanvas.Children.Add(poly);
            }

            alpha++;
        }

        public double ToRad(double angle)
        {
            return (Math.PI / 180) * angle;
        }
    }
}
