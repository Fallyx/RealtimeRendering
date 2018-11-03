using System;
using System.Windows;

namespace RealtimeRendering.Models
{
    public class Triangle2D
    {
        private Vector pointA;
        private Vector pointB;
        private Vector pointC;
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
    }
}
