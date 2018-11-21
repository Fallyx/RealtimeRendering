using System.Numerics;

namespace RealtimeRendering.Models
{
    class Matrix2x2
    {
        private double m11;
        private double m12;
        private double m21;
        private double m22;

        public Matrix2x2(double m11, double m12, double m21, double m22)
        {
            M11 = m11;
            M12 = m12;
            M21 = m21;
            M22 = m22;
        }

        public double M11 { get => m11; set => m11 = value; }
        public double M12 { get => m12; set => m12 = value; }
        public double M21 { get => m21; set => m21 = value; }
        public double M22 { get => m22; set => m22 = value; }

        public Vector2 GetUV(Vector3 AP)
        {
            Matrix2x2 invM = Inverse();

            return new Vector2((float)(invM.M11 * AP.X + invM.M12 * AP.Y), (float)(invM.M21 * AP.X + invM.M22 * AP.Y));
        }

        private Matrix2x2 Inverse()
        {
            if ((M11 * M22 - M12 * M21) == 0) return new Matrix2x2(M11, M12, M21, M22);

            double det = 1f / (M11 * M22 - M12 * M21);

            double d = det * M22;
            double b = det * (M12 * -1);
            double c = det * (M21 * -1);
            double a = det * M11;

            return new Matrix2x2(d, b, c, a);
        }        
    }
}
