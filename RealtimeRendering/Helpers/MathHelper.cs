using System.Numerics;

namespace RealtimeRendering.Helpers
{
    public class MathHelper
    {
        public static Vector3 Transform(Vector3 v, Matrix4x4 m)
        {
            Vector4 p = Vector4.Transform(v, m);
            p /= p.W;
            return new Vector3(p.X, p.Y, p.Z);
        }
    }
}
