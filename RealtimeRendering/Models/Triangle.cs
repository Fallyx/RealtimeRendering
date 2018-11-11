using System;
using System.Numerics;
using System.Windows.Media;
using RealtimeRendering.Helpers;

namespace RealtimeRendering
{
    public class Triangle
    {
        #region Vars
        private Vector3 pointA;
        private Vector4 pointAH;
        private Vector3 colorA;
        private Vector4 colorAH;

        private Vector3 pointB;
        private Vector4 pointBH;
        private Vector3 colorB;
        private Vector4 colorBH;

        private Vector3 pointC;
        private Vector4 pointCH;
        private Vector3 colorC;
        private Vector4 colorCH;

        private Vector3 normal;
        private Vector3 normalH;
        private Brush bColor;

        private int minX;
        private int maxX;
        private int minY;
        private int maxY;
        #endregion

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
            PointAH = new Vector4(pointA / pointA.Z, 1 / pointA.Z);

            PointB = pointB;
            ColorB = colorB;
            PointBH = new Vector4(pointB / pointB.Z, 1 / pointB.Z);

            ColorC = colorC;
            PointC = pointC;
            PointCH = new Vector4(pointC / pointC.Z, 1 / pointC.Z);

            Normal = Vector3.Normalize(normal);
        }

        #region getter and setters
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
        public Vector4 ColorAH { get => colorAH; }
        public Vector4 ColorBH { get => colorBH; }
        public Vector4 ColorCH { get => colorCH; }
        public Vector3 NormalH { get => normalH; set => normalH = value; }
        public int MinX { get => minX; set => minX = value; }
        public int MaxX { get => maxX; set => maxX = value; }
        public int MinY { get => minY; set => minY = value; }
        public int MaxY { get => maxY; set => maxY = value; }

        public void SetColorsH (float w)
        {
            colorAH = new Vector4(ColorA.X / w, ColorA.Y / w, ColorA.Z / w, 1 / w);
            colorBH = new Vector4(ColorB.X / w, ColorB.Y / w, ColorB.Z / w, 1 / w);
            colorCH = new Vector4(ColorC.X / w, ColorC.Y / w, ColorC.Z / w, 1 / w);
        }


        #endregion

        public void CalcMinMax()
        {
            MinX = (int)Math.Min(PointA.X, Math.Min(PointB.X, PointC.X));
            MaxX = (int)Math.Max(PointA.X, Math.Max(PointB.X, PointC.X));
            MinY = (int)Math.Min(PointA.Y, Math.Min(PointB.Y, PointC.Y));
            MaxY = (int)Math.Max(PointA.Y, Math.Max(PointB.Y, PointC.Y));
        }

        public Vector3 InterpolateColor(float u, float v)
        {
            Vector4 _colorPt = new Vector4(ColorAH.X + (u * (ColorBH.X - ColorAH.X)) + (v * (ColorCH.X - ColorAH.X)),
                                          ColorAH.Y + (u * (ColorBH.Y - ColorAH.Y)) + (v * (ColorCH.Y - ColorAH.Y)),
                                          ColorAH.Z + (u * (ColorBH.Z - ColorAH.Z)) + (v * (ColorCH.Z - ColorAH.Z)),
                                          ColorAH.W + (u * (ColorBH.W - ColorAH.W)) + (v * (ColorCH.W - ColorAH.W)));

            Vector3 colorPt = new Vector3(_colorPt.X / _colorPt.W, _colorPt.Y / _colorPt.W, _colorPt.Z / _colorPt.W);
            return colorPt;
        }

        public Vector3 GetPoint(float u, float v)
        {
            Vector4 pt = PointAH + (PointBH - PointAH) * u + (PointCH - PointAH) * v;
            pt /= pt.W;

            return new Vector3(pt.X, pt.Y, pt.Z);
        }

        public void TransformNormal(Matrix4x4 m)
        {
            Vector4 norm = new Vector4(Normal, 0);

            Vector4 transNorm = Vector4.Transform(norm, m);
            transNorm /= transNorm.W;

            Normal = new Vector3(transNorm.X, transNorm.Y, transNorm.Z);
        }
    }
}
