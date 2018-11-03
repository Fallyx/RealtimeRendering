using System.Numerics;
using System.Windows.Media;
using RealtimeRendering.Helpers;

namespace RealtimeRendering
{
    public class Triangle
    {
        private Vector3 pointA;
        private Vector3 pointB;
        private Vector3 pointC;
        private Brush bColor;
        private Vector3 color;

        public Triangle(Vector3[] points, Brush bColor)
        {
            PointA = points[0];
            PointB = points[1];
            PointC = points[2];
            BColor = bColor;
        }

        public Triangle(Vector3 pointA, Vector3 pointB, Vector3 pointC, Brush bColor)
        {
            PointA = pointA;
            PointB = pointB;
            PointC = pointC;
            BColor = bColor;
        }

        public Triangle(Vector3 pointA, Vector3 pointB, Vector3 pointC, Vector3 color)
        {
            PointA = pointA;
            PointB = pointB;
            PointC = pointC;
            Color = color;
        }

        public Vector3 PointA { get => pointA; set => pointA = value; }
        public Vector3 PointB { get => pointB; set => pointB = value; }
        public Vector3 PointC { get => pointC; set => pointC = value; }
        public Brush BColor { get => bColor; set => bColor = value; }
        public Vector3 Color { get => color; set => color = value; }

        public Triangle Transform(Matrix4x4 m)
        {
            return new Triangle (MathHelper.Transform(PointA, m), MathHelper.Transform(PointB, m), MathHelper.Transform(PointC, m), Color);
        }
    }
}
