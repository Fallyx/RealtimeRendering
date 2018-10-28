using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Windows.Media;

namespace RealtimeRendering
{
    public class Triangle
    {
        private Vector3 pointA;
        private Vector3 pointB;
        private Vector3 pointC;
        private Brush color;       

        public Triangle(Vector3[] points, Brush color)
        {
            PointA = points[0];
            PointB = points[1];
            PointC = points[2];
            Color = color;
        }

        public Triangle(Vector3 pointA, Vector3 pointB, Vector3 pointC, Brush color)
        {
            PointA = pointA;
            PointB = pointB;
            PointC = pointC;
            Color = color;
        }

        public Vector3 PointA { get => pointA; set => pointA = value; }
        public Vector3 PointB { get => pointB; set => pointB = value; }
        public Vector3 PointC { get => pointC; set => pointC = value; }
        public Brush Color { get => color; set => color = value; }
    }
}
