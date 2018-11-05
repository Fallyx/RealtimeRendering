using System.Numerics;
using System.Windows.Media;
using RealtimeRendering.Helpers;

namespace RealtimeRendering
{
    public class Triangle
    {
        private Vector3 pointA;
        private Vector4 pointAH;
        private Vector3 colorA;

        private Vector3 pointB;
        private Vector4 pointBH;
        private Vector3 colorB;

        private Vector3 pointC;
        private Vector4 pointCH;
        private Vector3 colorC;

        private Vector3 normal;
        private Brush bColor;

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

        public Triangle(Vector3 pointA, Vector3 pointB, Vector3 pointC, Vector3 colorA, Vector3 colorB, Vector3 colorC)
        {
            PointA = pointA;
            PointB = pointB;
            PointC = pointC;
            ColorA = colorA;
            ColorB = colorB;
            ColorC = colorC;
        }

        public Triangle(Vector3 pointA, Vector3 pointB, Vector3 pointC, Vector3 normal, Vector3 colorA, Vector3 colorB, Vector3 colorC)
        {
            PointA = pointA;
            ColorA = colorA;
            PointAH = new Vector4(pointA.X / pointA.Z, pointA.Y / pointA.Z, pointA.Z / pointA.Z, 1 / pointA.Z);

            PointB = pointB;
            ColorB = colorB;
            PointBH = new Vector4(pointB.X / pointB.Z, pointB.Y / pointB.Z, pointB.Z / pointB.Z, 1 / pointB.Z);

            ColorC = colorC;
            PointC = pointC;
            PointCH = new Vector4(pointC.X / pointC.Z, pointC.Y / pointC.Z, pointC.Z / pointC.Z, 1 / pointC.Z);

            Normal = Vector3.Normalize(normal);
        }

        public Vector3 PointA { get => pointA; set => pointA = value; }
        public Vector3 PointB { get => pointB; set => pointB = value; }
        public Vector3 PointC { get => pointC; set => pointC = value; }
        public Brush BColor { get => bColor; set => bColor = value; }
        public Vector3 ColorA { get => colorA; set => colorA = value; }
        public Vector3 ColorB { get => colorB; set => colorB = value; }
        public Vector3 ColorC { get => colorC; set => colorC = value; }
        public Vector3 Normal { get => normal; set => normal = value; }
        public Vector4 PointAH { get => pointAH; set => pointAH = value; }
        public Vector4 PointBH { get => pointBH; set => pointBH = value; }
        public Vector4 PointCH { get => pointCH; set => pointCH = value; }

        public void TransformNormal(Matrix4x4 m)
        {
            Vector4 norm = new Vector4(Normal, 0);

            Vector4 transNorm = Vector4.Transform(norm, m);
            transNorm /= transNorm.W;

            Normal = new Vector3(transNorm.X, transNorm.Y, transNorm.Z);
        }
    }
}
