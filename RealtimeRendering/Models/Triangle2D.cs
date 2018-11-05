using System;
using System.Windows;
using System.Numerics;

namespace RealtimeRendering.Models
{
    public class Triangle2D
    {
        private Vector pointA;
        private Vector pointB;
        private Vector pointC;
        private Vector4 colorA;
        private Vector4 colorB;
        private Vector4 colorC;
        private int minX;
        private int maxX;
        private int minY;
        private int maxY;


        public Triangle2D(Vector pointA, Vector pointB, Vector pointC)
        {
            PointA = pointA;
            PointB = pointB;
            PointC = pointC;
        }

        public Triangle2D(Vector pointA, Vector pointB, Vector pointC, Vector3 colorA, Vector3 colorB, Vector3 colorC, float w)
        {
            PointA = pointA;
            PointB = pointB;
            PointC = pointC;
            ColorA = new Vector4(colorA.X / w, colorA.Y / w, colorA.Z / w, 1 / w);
            ColorB = new Vector4(colorB.X / w, colorB.Y / w, colorB.Z / w, 1 / w);
            ColorC = new Vector4(colorC.X / w, colorC.Y / w, colorC.Z / w, 1 / w);

            CalcMinMax();
        }

        public Vector3 InterpolateColor(float u, float v)
        {
            Vector4 _colorPt = new Vector4(ColorA.X + (u * (ColorB.X - ColorA.X)) + (v * (ColorC.X - ColorA.X)), 
                                          ColorA.Y + (u * (ColorB.Y - ColorA.Y)) + (v * (ColorC.Y - ColorA.Y)),
                                          ColorA.Z + (u * (ColorB.Z - ColorA.Z)) + (v * (ColorC.Z - ColorA.Z)),
                                          ColorA.W + (u * (ColorB.W - ColorA.W)) + (v * (ColorC.W - ColorA.W)));

            Vector3 colorPt = new Vector3(_colorPt.X / _colorPt.W, _colorPt.Y / _colorPt.W, _colorPt.Z / _colorPt.W);
            return colorPt;
        }

        private void CalcMinMax()
        {
            MinX = (int)Math.Min(PointA.X, Math.Min(PointB.X, PointC.X));
            MaxX = (int)Math.Max(PointA.X, Math.Max(PointB.X, PointC.X));
            MinY = (int)Math.Min(PointA.Y, Math.Min(PointB.Y, PointC.Y));
            MaxY = (int)Math.Max(PointA.Y, Math.Max(PointB.Y, PointC.Y));
        }

        public Vector PointA { get => pointA; set => pointA = value; }
        public Vector PointB { get => pointB; set => pointB = value; }
        public Vector PointC { get => pointC; set => pointC = value; }
        public int MinX { get => minX; private set => minX = value; }
        public int MaxX { get => maxX; private set => maxX = value; }
        public int MinY { get => minY; private set => minY = value; }
        public int MaxY { get => maxY; private set => maxY = value; }
        public Vector4 ColorA { get => colorA; set => colorA = value; }
        public Vector4 ColorB { get => colorB; set => colorB = value; }
        public Vector4 ColorC { get => colorC; set => colorC = value; }
    }
}
