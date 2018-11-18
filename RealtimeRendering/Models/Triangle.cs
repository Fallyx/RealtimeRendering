using System;
using System.Numerics;
using System.Windows.Media;
using RealtimeRendering.Helpers;

namespace RealtimeRendering.Models
{
    public class Triangle
    {
        #region Vars
        private Vector3 pointA;
        private Vector4 pointAH;
        private Vector3 colorA;
        private Vector4 colorAH;

        private Vector3 pointB;
        private Vector4 pointBH;
        private Vector3 colorB;
        private Vector4 colorBH;

        private Vector3 pointC;
        private Vector4 pointCH;
        private Vector3 colorC;
        private Vector4 colorCH;

        private Vector3 normal;
        private Vector3 normalH;
        private Brush bColor;

        private Vertex vA;
        private Vertex vB;
        private Vertex vC;

        private int minX;
        private int maxX;
        private int minY;
        private int maxY;

        private bool hasTexture;
        #endregion

        public Triangle(Vector3 pointA, Vector3 pointB, Vector3 pointC, Brush bColor)
        {
            PointA = pointA;
            PointB = pointB;
            PointC = pointC;
            BColor = bColor;
        }

        public Triangle(Vector3 pointA, Vector3 pointB, Vector3 pointC, Vector3 normal, Vector3 colorA, Vector3 colorB, Vector3 colorC)
        {
            /*
            PointA = pointA;
            ColorA = colorA;
            PointAH = new Vector4(pointA / pointA.Z, 1 / pointA.Z);

            PointB = pointB;
            ColorB = colorB;
            PointBH = new Vector4(pointB / pointB.Z, 1 / pointB.Z);

            ColorC = colorC;
            PointC = pointC;
            PointCH = new Vector4(pointC / pointC.Z, 1 / pointC.Z);

            Normal = Vector3.Normalize(normal);
            */
            //Vector3 normal = Vector3.Normalize(Vector3.Cross(C - A, B - A));


            A = new Vertex(pointA, colorA, Vector3.Normalize(normal));
            B = new Vertex(pointB, colorB, Vector3.Normalize(normal));
            C = new Vertex(pointC, colorC, Vector3.Normalize(normal));
            HasTexture = false;
        }

        public Triangle(Vertex vA, Vertex vB, Vertex vC)
        {
            A = vA;
            B = vB;
            C = vC;
        }

        public Triangle(Vector3 pointA, Vector3 pointB, Vector3 pointC, Vector3 normal, Vector2 stA, Vector2 stB, Vector2 stC)
        {
            A = new Vertex(pointA, stA, Vector3.Normalize(normal));
            B = new Vertex(pointB, stB, Vector3.Normalize(normal));
            C = new Vertex(pointC, stC, Vector3.Normalize(normal));
            HasTexture = true;
        }

        #region getter and setters
        public Vector3 PointA { get => pointA; set => pointA = value; }
        public Vector3 PointB { get => pointB; set => pointB = value; }
        public Vector3 PointC { get => pointC; set => pointC = value; }
        public Brush BColor { get => bColor; set => bColor = value; }
        public Vector3 ColorA { get => colorA; set => colorA = value; }
        public Vector3 ColorB { get => colorB; set => colorB = value; }
        public Vector3 ColorC { get => colorC; set => colorC = value; }
        public Vector3 Normal { get => normal; set => normal = value; }
        public Vector4 PointAH { get => pointAH; set => pointAH = value; }
        public Vector4 PointBH { get => pointBH; set => pointBH = value; }
        public Vector4 PointCH { get => pointCH; set => pointCH = value; }
        public Vector4 ColorAH { get => colorAH; }
        public Vector4 ColorBH { get => colorBH; }
        public Vector4 ColorCH { get => colorCH; }
        public Vector3 NormalH { get => normalH; set => normalH = value; }


        public int MinX { get => minX; set => minX = value; }
        public int MaxX { get => maxX; set => maxX = value; }
        public int MinY { get => minY; set => minY = value; }
        public int MaxY { get => maxY; set => maxY = value; }
        public Vertex A { get => vA; set => vA = value; }
        public Vertex B { get => vB; set => vB = value; }
        public Vertex C { get => vC; set => vC = value; }
        public bool HasTexture { get => hasTexture; private set => hasTexture = value; }

        public void SetColorsH (float w)
        {
            colorAH = new Vector4(ColorA / w, 1 / w);
            colorBH = new Vector4(ColorB / w, 1 / w);
            colorCH = new Vector4(ColorC / w, 1 / w);
        }
        #endregion

        public Vector3 InterpolateColor(float u, float v)
        {
            Vector4 _colorPt = new Vector4(ColorAH.X + (u * (ColorBH.X - ColorAH.X)) + (v * (ColorCH.X - ColorAH.X)),
                                          ColorAH.Y + (u * (ColorBH.Y - ColorAH.Y)) + (v * (ColorCH.Y - ColorAH.Y)),
                                          ColorAH.Z + (u * (ColorBH.Z - ColorAH.Z)) + (v * (ColorCH.Z - ColorAH.Z)),
                                          ColorAH.W + (u * (ColorBH.W - ColorAH.W)) + (v * (ColorCH.W - ColorAH.W)));

            Vector3 colorPt = new Vector3(_colorPt.X / _colorPt.W, _colorPt.Y / _colorPt.W, _colorPt.Z / _colorPt.W);
            return colorPt;
        }

        /*
        public Vector3 GetPoint(float u, float v)
        {
            Vector4 pt = PointAH + (PointBH - PointAH) * u + (PointCH - PointAH) * v;
            pt /= pt.W;

            return new Vector3(pt.X, pt.Y, pt.Z);
        }
        */

        public Vector3 GetColor(float u, float v)
        {
            Vector4 clr = A.ColorH + (B.ColorH - A.ColorH) * u + (C.ColorH - A.ColorH) * v;
            clr /= clr.W;

            return new Vector3(clr.X, clr.Y, clr.Z);
        }

        public Vector3 GetColor(float u, float v, float w)
        {
            Vector4 clrAH = new Vector4(A.Color / w, 1 / w);
            Vector4 clrBH = new Vector4(B.Color / w, 1 / w);
            Vector4 clrCH = new Vector4(C.Color / w, 1 / w);

            Vector4 clrH = new Vector4(clrAH.X + u * (clrBH.X - clrAH.X) + v * (clrCH.X - clrAH.X),
                clrAH.Y + u * (clrBH.Y - clrAH.Y) + v * (clrCH.Y - clrAH.Y),
                clrAH.Z + u * (clrBH.Z - clrAH.Z) + v * (clrCH.Z - clrAH.Z),
                clrAH.W + u * (clrBH.W - clrAH.W) + v * (clrCH.W - clrAH.W));

           return new Vector3(clrH.X / clrH.W, clrH.Y / clrH.W, clrH.Z / clrH.W);
        }

        public Vector3 GetPoint(float u, float v)
        {
            Vector4 pt = A.PointH + (B.PointH - A.PointH) * u + (C.PointH - A.PointH) * v;
            pt /= pt.W;

            return new Vector3(pt.X, pt.Y, pt.Z);
        }

        public Vector3 GetNormal(float u, float v)
        {
            Vector4 norm = A.NormalH + (B.NormalH - A.NormalH) * u + (C.NormalH - A.NormalH) * v;
            norm /= norm.W;

            return new Vector3(norm.X, norm.Y, norm.Z);
        }

        public Vector3 GetNormal4(float u, float v, Matrix4x4 M)
        {
            Matrix4x4.Invert(M, out Matrix4x4 invM);

            Vector4 normhA = Vector4.Transform(A.Normal, invM);
            normhA.W = 0;
            normhA = Vector4.Normalize(normhA);

            Vector4 normhB = Vector4.Transform(B.Normal, invM);
            normhB.W = 0;
            normhB = Vector4.Normalize(normhB);

            Vector4 normhC = Vector4.Transform(C.Normal, invM);
            normhC.W = 0;
            normhC = Vector4.Normalize(normhC);

            Vector4 normh = normhA + (normhB - normhA) * u + (normhC - normhA) * v;
            normh.W = 0;
            normh = Vector4.Normalize(normh);

            return new Vector3(normh.X, normh.Y, normh.Z);
        }

        public Triangle Transform(Matrix4x4 m)
        {
            Vertex _A = A.Transform(m);
            Vertex _B = B.Transform(m);
            Vertex _C = C.Transform(m);

            return new Triangle(_A, _B, _C);
        }

        public Vector3 TransformNormal(Matrix4x4 m)
        {
            Vector4 norm = new Vector4(Normal, 0);

            bool t = Matrix4x4.Invert(m, out Matrix4x4 invM);

            Vector4 transNorm = Vector4.Transform(norm, invM);
            transNorm /= transNorm.W;

            return new Vector3(transNorm.X, transNorm.Y, transNorm.Z);
        }
    }
}
