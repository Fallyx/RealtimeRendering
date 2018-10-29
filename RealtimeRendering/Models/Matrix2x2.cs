using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public static Matrix2x2 Inverse(Matrix2x2 m)
        {
            //double det = 1f / (AB.X * AC.Y - AC.X * AB.Y);
            double det = 1f / (m.M11 * m.M22 - m.M21 * m.M12);
            /*
            invMat[0] = invCalc * AC.Y;  // d
            invMat[1] = invCalc * -AB.Y; // -c
            invMat[2] = invCalc * -AC.X; // -b
            invMat[3] = invCalc * AB.X; // a
            */

            return new Matrix2x2(det * m.M22, det * -m.M21, det * -m.M12, det * m.M11);
        }
    }
}
