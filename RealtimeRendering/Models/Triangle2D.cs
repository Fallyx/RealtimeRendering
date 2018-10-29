using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Windows;

namespace RealtimeRendering.Models
{
    public class Triangle2D
    {
        private Vector pointA;
        private Vector pointB;
        private Vector pointC;

        public Triangle2D(Vector pointA, Vector pointB, Vector pointC)
        {
            PointA = pointA;
            PointB = pointB;
            PointC = pointC;
        }

        public Vector PointA { get => pointA; set => pointA = value; }
        public Vector PointB { get => pointB; set => pointB = value; }
        public Vector PointC { get => pointC; set => pointC = value; }
    }
}
