﻿using System;
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
        private Vector2 textureSt;

        public Vector3 Point { get => point; set => point = value; }
        public Vector3 Color { get => color; set => color = value; }
        public Vector3 Normal { get => normal; set => normal = value; }
        public Vector2 TextureSt { get => textureSt; set => textureSt = value; }

        public Vector4 PointH { get { return new Vector4(Point / Point.Z, 1 / Point.Z); } }
        public Vector4 NormalH { get { return new Vector4(Normal / Point.Z, 1 / Point.Z); } }
        public Vector4 ColorH { get { return new Vector4(Color / Point.Z, 1 / Point.Z); } }
        public Vector3 TextureStH { get { return new Vector3(TextureSt / Point.Z, 1 / Point.Z); } }

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

        public Vertex Transform(Matrix4x4 m)
        {
            return null;
        }
    }
}
