using System;
using System.Numerics;
using System.Windows.Media;

namespace RealtimeRendering.Models
{
    public class Triangle
    {
        #region Vars
        private Vertex vA;
        private Vertex vB;
        private Vertex vC;

        private bool hasTexture;
        #endregion

        /*
        public Triangle(Vector3 pointA, Vector3 pointB, Vector3 pointC, Brush bColor)
        {
            PointA = pointA;
            PointB = pointB;
            PointC = pointC;
            BColor = bColor;
        }
        */

        public Triangle(Vertex vA, Vertex vB, Vertex vC, bool hasTexture)
        {
            A = vA;
            B = vB;
            C = vC;
            HasTexture = hasTexture;
        }

        public Triangle(Vector3 pointA, Vector3 pointB, Vector3 pointC, Vector3 colorA, Vector3 colorB, Vector3 colorC)
        {
            Vector3 normal = Vector3.Normalize(Vector3.Cross(pointB - pointA, pointC- pointA));

            A = new Vertex(pointA, colorA, normal);
            B = new Vertex(pointB, colorB, normal);
            C = new Vertex(pointC, colorC, normal);
            HasTexture = false;
        }

        public Triangle(Vector3 pointA, Vector3 pointB, Vector3 pointC, Vector2 stA, Vector2 stB, Vector2 stC)
        {
            Vector3 normal = Vector3.Normalize(Vector3.Cross(pointB - pointA, pointC - pointA));

            A = new Vertex(pointA, stA, normal);
            B = new Vertex(pointB, stB, normal);
            C = new Vertex(pointC, stC, normal);
            HasTexture = true;
        }

        #region getter and setters
        public Vertex A { get => vA; set => vA = value; }
        public Vertex B { get => vB; set => vB = value; }
        public Vertex C { get => vC; set => vC = value; }
        public bool HasTexture { get => hasTexture; private set => hasTexture = value; }
        #endregion

        public Vector3 GetColor(float u, float v)
        {
            Vector4 clr = A.GetColorH() + (B.GetColorH() - A.GetColorH()) * u + (C.GetColorH() - A.GetColorH()) * v;
            clr /= clr.W;

            return new Vector3(clr.X, clr.Y, clr.Z);
        }

        public Vector3 GetPoint(float u, float v)
        {
            Vector4 pt = A.GetPointH() + (B.GetPointH() - A.GetPointH()) * u + (C.GetPointH() - A.GetPointH()) * v;
            pt /= pt.W;

            return new Vector3(pt.X, pt.Y, pt.Z);
        }

        public Vector3 GetNormal(float u, float v)
        {
            Vector4 norm = A.GetNormalH() + (B.GetNormalH() - A.GetNormalH()) * u + (C.GetNormalH() - A.GetNormalH()) * v;
            norm /= norm.W;

            return new Vector3(norm.X, norm.Y, norm.Z);
        }

        public Vector2 GetTexture(float u, float v)
        {
            Vector3 texture = A.GetTextureStH() + (B.GetTextureStH() - A.GetTextureStH()) * u + (C.GetTextureStH() - A.GetTextureStH()) * v;
            texture /= texture.Z;

            return new Vector2(texture.X, texture.Y);
        }

        public Triangle Transform(Matrix4x4 m)
        {
            Vertex _A;
            Vertex _B;
            Vertex _C;

            if(HasTexture)
            {
                _A = A.TransformTexture(m);
                _B = B.TransformTexture(m);
                _C = C.TransformTexture(m);
            }
            else
            {
                _A = A.Transform(m);
                _B = B.Transform(m);
                _C = C.Transform(m);
            }

            return new Triangle(_A, _B, _C, HasTexture);
        }

        public Triangle Project()
        {
            Vertex _A;
            Vertex _B;
            Vertex _C;

            if(HasTexture)
            {
                _A = A.ProjectTexture();
                _B = B.ProjectTexture();
                _C = C.ProjectTexture();
            }
            else
            {
                _A = A.Project();
                _B = B.Project();
                _C = C.Project();
            }

            return new Triangle(_A, _B, _C, HasTexture);
        }
    }
}
