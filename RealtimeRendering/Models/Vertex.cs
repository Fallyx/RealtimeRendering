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
        public float W { get => w; set => w = value; }

        public Vector4 GetPointH() { return new Vector4(Point / Point.Z, 1 / Point.Z); }
        public Vector4 GetNormalH() { return new Vector4(Normal / Point.Z, 1 / Point.Z); }
        public Vector4 GetColorH()  { return new Vector4(Color / Point.Z, 1 / Point.Z);  }
        public Vector3 GetTextureStH() { return new Vector3(TextureSt / Point.Z, 1 / Point.Z); }        

        public Vertex(Vector3 point, Vector3 color, Vector3 normal)
        {
            Point = point;
            Color = color;
            Normal = normal;
            W = 1; // Doesnt shrink or grow
        }

        public Vertex(Vector3 point, Vector2 st, Vector3 normal)
        {
            Point = point;
            TextureSt = st;
            Normal = normal;
            W = 1;
        }

        /// <summary>
        /// Transform the vertex with the matrix
        /// </summary>
        /// <param name="m"></param>
        /// <returns>New vertex</returns>
        public Vertex Transform(Matrix4x4 m)
        {
            (Vector4 pt, Matrix4x4 invM) = Trans(m);

            Vertex v = new Vertex(new Vector3(pt.X, pt.Y, pt.Z), Color, Vector3.Normalize(Vector3.TransformNormal(Normal, invM)));
            v.W = pt.W;

            return v;
        }

        /// <summary>
        /// Transform the vertex with the matrix
        /// </summary>
        /// <param name="m"></param>
        /// <returns>New vertex</returns>
        public Vertex TransformTexture(Matrix4x4 m)
        {
            (Vector4 pt, Matrix4x4 invM) = Trans(m);

            Vertex v = new Vertex(new Vector3(pt.X, pt.Y, pt.Z), TextureSt, Vector3.Normalize(Vector3.TransformNormal(Normal, invM)));
            v.W = pt.W;

            return v;
        }

        /// <summary>
        /// Transform the point and invert the matrix
        /// </summary>
        /// <param name="m"></param>
        /// <returns>Tuple with new point and inverted matrix</returns>
        private (Vector4, Matrix4x4) Trans(Matrix4x4 m)
        {
            Vector4 pt = Vector4.Transform(Point, m);

            Matrix4x4.Invert(m, out Matrix4x4 invM);
            invM = Matrix4x4.Transpose(invM);

            return (pt, invM);
        }

        /// <summary>
        /// Divide the point by W
        /// </summary>
        /// <returns>New vertex</returns>
        public Vertex Project()
        {
            return new Vertex(Point / W, Color, Normal);
        }

        /// <summary>
        /// Divide the point by W
        /// </summary>
        /// <returns>New vertex</returns>
        public Vertex ProjectTexture()
        {
            return new Vertex(Point / W, TextureSt, Normal);
        }
    }
}
