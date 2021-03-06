﻿using System.Numerics;

namespace RealtimeRendering.Models
{
    public class Triangle
    {
        private Vertex vA;
        private Vertex vB;
        private Vertex vC;
        private bool hasTexture;

        public Triangle(Vertex vA, Vertex vB, Vertex vC, bool hasTexture)
        {
            A = vA;
            B = vB;
            C = vC;
            HasTexture = hasTexture;
        }

        public Triangle(Vector3 pointA, Vector3 pointB, Vector3 pointC, Vector3 colorA, Vector3 colorB, Vector3 colorC)
        {
            // Calculate the normal
            Vector3 normal = Vector3.Normalize(Vector3.Cross(pointB - pointA, pointC- pointA));

            A = new Vertex(pointA, colorA, normal);
            B = new Vertex(pointB, colorB, normal);
            C = new Vertex(pointC, colorC, normal);
            HasTexture = false;
        }

        public Triangle(Vector3 pointA, Vector3 pointB, Vector3 pointC, Vector2 stA, Vector2 stB, Vector2 stC)
        {
            // Calculate the normal
            Vector3 normal = Vector3.Normalize(Vector3.Cross(pointB - pointA, pointC - pointA));

            A = new Vertex(pointA, stA, normal);
            B = new Vertex(pointB, stB, normal);
            C = new Vertex(pointC, stC, normal);
            HasTexture = true;
        }

        public Vertex A { get => vA; set => vA = value; }
        public Vertex B { get => vB; set => vB = value; }
        public Vertex C { get => vC; set => vC = value; }
        public bool HasTexture { get => hasTexture; private set => hasTexture = value; }

        /// <summary>
        /// Get the interpolated color of the triangle
        /// </summary>
        /// <param name="u"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        public Vector3 GetColor(float u, float v)
        {
            Vector4 clr = A.GetColorH() + (B.GetColorH() - A.GetColorH()) * u + (C.GetColorH() - A.GetColorH()) * v;
            clr /= clr.W;

            return new Vector3(clr.X, clr.Y, clr.Z);
        }

        /// <summary>
        /// Get the interpolated point of the triangle
        /// </summary>
        /// <param name="u"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        public Vector3 GetPoint(float u, float v)
        {
            Vector4 pt = A.GetPointH() + (B.GetPointH() - A.GetPointH()) * u + (C.GetPointH() - A.GetPointH()) * v;
            pt /= pt.W;

            return new Vector3(pt.X, pt.Y, pt.Z);
        }

        /// <summary>
        /// Get the interpolated normal of the triangle
        /// </summary>
        /// <param name="u"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        public Vector3 GetNormal(float u, float v)
        {
            Vector4 norm = A.GetNormalH() + (B.GetNormalH() - A.GetNormalH()) * u + (C.GetNormalH() - A.GetNormalH()) * v;
            norm /= norm.W;

            return new Vector3(norm.X, norm.Y, norm.Z);
        }

        /// <summary>
        /// Get the interpolated texture point
        /// </summary>
        /// <param name="u"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        public Vector2 GetTexture(float u, float v)
        {
            Vector3 texture = A.GetTextureStH() + (B.GetTextureStH() - A.GetTextureStH()) * u + (C.GetTextureStH() - A.GetTextureStH()) * v;
            texture /= texture.Z;

            return new Vector2(texture.X, texture.Y);
        }

        /// <summary>
        /// Transform the vertices with the matrix
        /// </summary>
        /// <param name="m"></param>
        /// <returns>new triangle with transformed vertices</returns>
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

        /// <summary>
        /// Project the vertices (Perspective Projection)
        /// </summary>
        /// <returns>New triangle with projected vertices</returns>
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
