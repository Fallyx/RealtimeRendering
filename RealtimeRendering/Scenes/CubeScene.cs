using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using RealtimeRendering.Models;

namespace RealtimeRendering.Scenes
{
    public class CubeScene
    {
        public static Triangle[] ObjectColoredCube(Vector3 objectColor)
        {
            Vector3[] cubePts = GetCubeIdx();
            Vector3[] triangleIdx = GetTrianglesIdx();
            Triangle[] triangles = new Triangle[triangleIdx.Length];
            byte normalIdx = 0;

            for (int i = 0; i < triangleIdx.Length; i += 2)
            {
                triangles[i] = new Triangle(cubePts[(int)triangleIdx[i].X], cubePts[(int)triangleIdx[i].Y], cubePts[(int)triangleIdx[i].Z], objectColor, objectColor, objectColor);
                triangles[i + 1] = new Triangle(cubePts[(int)triangleIdx[i + 1].X], cubePts[(int)triangleIdx[i + 1].Y], cubePts[(int)triangleIdx[i + 1].Z], objectColor, objectColor, objectColor);
                normalIdx++;
            }

            return triangles;
        }

        public static Triangle[] FaceColoredCube(Vector3[] faceColors)
        {
            Vector3[] cubePts = GetCubeIdx();
            Vector3[] triangleIdx = GetTrianglesIdx();
            Triangle[] triangles = new Triangle[triangleIdx.Length];
            byte normalIdx = 0;

            for (int i = 0; i < triangleIdx.Length; i += 2)
            {
                triangles[i] = new Triangle(cubePts[(int)triangleIdx[i].X], cubePts[(int)triangleIdx[i].Y], cubePts[(int)triangleIdx[i].Z], faceColors[normalIdx], faceColors[normalIdx], faceColors[normalIdx]);
                triangles[i + 1] = new Triangle(cubePts[(int)triangleIdx[i + 1].X], cubePts[(int)triangleIdx[i + 1].Y], cubePts[(int)triangleIdx[i + 1].Z], faceColors[normalIdx], faceColors[normalIdx], faceColors[normalIdx]);
                normalIdx++;
            }

            return triangles;
        }

        public static Triangle[] VertexColoredCube((Vector3 A, Vector3 B, Vector3 C)[] vertexColors)
        {
            Vector3[] cubePts = GetCubeIdx();
            Vector3[] triangleIdx = GetTrianglesIdx();
            Triangle[] triangles = new Triangle[triangleIdx.Length];
            byte normalIdx = 0;

            for (int i = 0; i < triangleIdx.Length; i += 2)
            {
                triangles[i] = new Triangle(cubePts[(int)triangleIdx[i].X], cubePts[(int)triangleIdx[i].Y], cubePts[(int)triangleIdx[i].Z], vertexColors[i].A, vertexColors[i].B, vertexColors[i].C);
                triangles[i + 1] = new Triangle(cubePts[(int)triangleIdx[i + 1].X], cubePts[(int)triangleIdx[i + 1].Y], cubePts[(int)triangleIdx[i + 1].Z], vertexColors[i + 1].A, vertexColors[i + 1].B, vertexColors[i + 1].C);
                normalIdx++;
            }

            return triangles;
        }

        public static Triangle[] TexturedCube()
        {
            Vector3[] cubePts = GetCubeIdx();
            Vector3[] triangleIdx = GetTrianglesIdx();
            Triangle[] triangles = new Triangle[triangleIdx.Length];
            Vector2[] textureIdx = GetTextureIdx();
            byte normalIdx = 0;

            for(int i = 0; i < triangleIdx.Length; i += 2)
            {
                triangles[i] = new Triangle(cubePts[(int)triangleIdx[i].X], cubePts[(int)triangleIdx[i].Y], cubePts[(int)triangleIdx[i].Z], textureIdx[0], textureIdx[1], textureIdx[2]);
                triangles[i + 1] = new Triangle(cubePts[(int)triangleIdx[i + 1].X], cubePts[(int)triangleIdx[i + 1].Y], cubePts[(int)triangleIdx[i + 1].Z], textureIdx[3], textureIdx[4], textureIdx[5]);
                normalIdx++;
            }

            return triangles;
        }

        public static Vector3[] GetPredefinedFaceColors()
        {
            return new Vector3[]
            {
                new Vector3(0, 0, 1),
                new Vector3(0, 1, 1),
                new Vector3(0, 1, 0),
                new Vector3(1, 1, 0),
                new Vector3(1, 0, 0),
                new Vector3(1, 0, 1)
            };
        }

        public static (Vector3, Vector3, Vector3)[] GetPredefinedVertexColors()
        {
            return new (Vector3, Vector3, Vector3)[]
            {
                (new Vector3(0, 0, 1), new Vector3(0, 0, 0.8f), new Vector3(0, 0, 0.8f)),
                (new Vector3(0, 0, 0.8f), new Vector3(0, 0, 1), new Vector3(0, 0, 0.8f)),

                (new Vector3(0, 1, 0), new Vector3(0, 0.8f, 0), new Vector3(0, 0.8f, 0)),
                (new Vector3(0, 0.8f, 0), new Vector3(0, 1f, 0), new Vector3(0, 0.8f, 0)),

                (new Vector3(1, 0, 0), new Vector3(0.8f, 0, 0), new Vector3(0.8f, 0, 0)),
                (new Vector3(0.8f, 0, 0), new Vector3(1, 0, 0), new Vector3(0.8f, 0, 0)),

                (new Vector3(0.25f, 0.8f, 0.95f), new Vector3(0.25f, 0.94f, 0.95f), new Vector3(0.25f, 0.94f, 0.95f)),
                (new Vector3(0.25f, 0.94f, 0.95f), new Vector3(0.25f, 0.8f, 0.95f), new Vector3(0.25f, 0.94f, 0.95f)),

                (new Vector3(0.25f, 0.49f, 0.95f), new Vector3(0.25f, 0.69f, 0.95f), new Vector3(0.25f, 0.69f, 0.95f)),
                (new Vector3(0.25f, 0.69f, 0.95f), new Vector3(0.25f, 0.49f, 0.95f), new Vector3(0.25f, 0.69f, 0.95f)),

                (new Vector3(0.95f, 0.25f, 0.76f), new Vector3(0.79f, 0.25f, 0.95f), new Vector3(0.79f, 0.25f, 0.95f)),
                (new Vector3(0.79f, 0.25f, 0.95f), new Vector3(0.95f, 0.25f, 0.76f), new Vector3(0.79f, 0.25f, 0.95f))
            };
        }

        private static Vector3[] GetCubeIdx()
        {
            return new Vector3[]
            {
                // top
                new Vector3(-1, -1, -1),
                new Vector3(1, -1, -1),
                new Vector3(1, 1, -1),
                new Vector3(-1, 1, -1),

                // bottom
                new Vector3(-1, -1, 1),
                new Vector3(1, -1, 1),
                new Vector3(1, 1, 1),
                new Vector3(-1, 1, 1)
            };
        }

        private static Vector3[] GetTrianglesIdx()
        {
            return new Vector3[]
            {
                new Vector3(0, 1, 2), // top
                new Vector3(0, 2, 3),
                new Vector3(7, 6, 5), // bottom
                new Vector3(7, 5, 4),
                new Vector3(0, 3, 7), // left
                new Vector3(0, 7, 4),
                new Vector3(2, 1, 5), // right
                new Vector3(2, 5, 6),
                new Vector3(3, 2, 6), // front
                new Vector3(3, 6, 7),
                new Vector3(1, 0, 4), // back
                new Vector3(1, 4, 5)
            };
        }

        private static Vector2[] GetTextureIdx()
        {
            return new Vector2[]
            {
                // upper right
                new Vector2(0, 0),
                new Vector2(0, 1),
                new Vector2(1, 1),

                // lower left
                new Vector2(0, 0),
                new Vector2(1, 1),
                new Vector2(1, 0)
            };
        }
    }
}
