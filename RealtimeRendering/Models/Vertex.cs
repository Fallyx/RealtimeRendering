using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace RealtimeRendering.Models
{
    public class Vertex
    {
        private Vector3 point;
        private Vector3 color;
        private Vector3 normal;
        private Vector2 st;

        public Vector3 Point { get => point; set => point = value; }
        public Vector3 Color { get => color; set => color = value; }
        public Vector3 Normal { get => normal; set => normal = value; }
        public Vector2 St { get => st; set => st = value; }

        public Vertex(Vector3 point, Vector3 color)
        {
            Point = point;
            Color = color;
        }

        public Vertex(Vector3 point, Vector3 color, Vector3 normal)
        {
            Point = point;
            Color = color;
            Normal = normal;
        }
    }
}
