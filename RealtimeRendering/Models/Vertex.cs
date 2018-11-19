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
        private Vector2 textureSt;
        private float w;

        public Vector3 Point { get => point; set => point = value; }
        public Vector3 Color { get => color; set => color = value; }
        public Vector3 Normal { get => normal; set => normal = value; }
        public Vector2 TextureSt { get => textureSt; set => textureSt = value; }

        public Vector4 GetPointH() { return new Vector4(Point / Point.Z, 1 / Point.Z); }
        public Vector4 GetNormalH() { return new Vector4(Normal / Point.Z, 1 / Point.Z); }
        public Vector4 GetColorH()  { return new Vector4(Color / Point.Z, 1 / Point.Z);  }
        public Vector3 GetTextureStH() { return new Vector3(TextureSt / Point.Z, 1 / Point.Z); }

        public float W { get => w; set => w = value; }

        public Vertex(Vector3 point, Vector3 color)
        {
            Point = point;
            Color = color;
            W = 1;
        }

        public Vertex(Vector3 point, Vector3 color, Vector3 normal)
        {
            Point = point;
            Color = color;
            Normal = normal;
            W = 1;
        }

        public Vertex(Vector3 point, Vector2 st, Vector3 normal)
        {
            Point = point;
            TextureSt = st;
            Normal = normal;
            W = 1;
        }

        public Vertex Transform(Matrix4x4 m)
        {
            (Vector4 pt, Matrix4x4 invM) = Trans(m);

            Vertex v = new Vertex(new Vector3(pt.X, pt.Y, pt.Z), Color, Vector3.Normalize(Vector3.TransformNormal(Normal, invM)));
            v.W = pt.W;

            return v;
        }

        public Vertex TransformTexture(Matrix4x4 m)
        {
            (Vector4 pt, Matrix4x4 invM) = Trans(m);

            Vertex v = new Vertex(new Vector3(pt.X, pt.Y, pt.Z), TextureSt, Vector3.Normalize(Vector3.TransformNormal(Normal, invM)));
            v.W = pt.W;

            return v;
        }

        private (Vector4, Matrix4x4) Trans(Matrix4x4 m)
        {
            Vector4 pt = Vector4.Transform(Point, m);

            Matrix4x4.Invert(m, out Matrix4x4 invM);
            invM = Matrix4x4.Transpose(invM);

            return (pt, invM);
        }

        public Vertex Project()
        {
            return new Vertex(Point / W, Color, Normal);
        }

        public Vertex ProjectTexture()
        {
            return new Vertex(Point / W, TextureSt, Normal);
        }
    }
}
