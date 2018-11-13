using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Numerics;

namespace RealtimeRendering.Models
{
    public class Texture
    {
        private static string rootPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
        private string texturePath;
        private Bitmap img;

        public Texture(string texturePath)
        {
            TexturePath = texturePath;
            Img = new Bitmap(texturePath);
        }

        public string TexturePath { get => texturePath; private set => texturePath = value; }
        public Bitmap Img { get => img; set => img = value; }

        /*
        public Vector3 SphericalProjection(Vector3 direction)
        {
            Vector3 imgColor = Vector3.Zero;

            if (direction.X >= -1f && direction.X <= 1 && direction.Y >= -1f && direction.Y <= 1 && direction.Z >= -1f && direction.Z <= 1)
            {
                double s = Math.Atan2(direction.X, direction.Z);
                double t = Math.Acos(direction.Y);

                double _s = Helpers.MathHelper.RangeConverter(-Math.PI, Math.PI, -1, 1, s);
                double _t = Helpers.MathHelper.RangeConverter(0, Math.PI, -1, 1, t);
                _s *= -1;
                _t *= -1;

                imgColor = GetColorFromImage(_s, _t);
            }
            return imgColor;
        }
        */

        public Vector3 PlanarProjection(Vector3 direction)
        {
            double s = direction.X;
            double t = direction.Y;
            Vector3 imgColor = GetColorFromImage(s, t);

            return imgColor;
        }

        public Vector3 LookUp(Vector2 tc)
        {
            float s = tc.X;
            float t = tc.Y;

            int si = (int)(s * Img.Width) & (Img.Width - 1);
            int ti = (int)(t * Img.Height) & (Img.Height - 1);

            return GetColorFromImage(si, ti);
        }

        private Vector3 GetColorFromImage(double s, double t)
        {
            Vector3 imgColor = Vector3.One;

            int x = (int)((s + 1) / 2f * (Img.Width - 1) + (Img.Width - 1)) % Img.Width;
            int y = (int)((t + 1) / 2f * (Img.Height - 1) + (Img.Height - 1)) % Img.Height;

            var clr = Img.GetPixel(x, y);
            System.Windows.Media.Color c = System.Windows.Media.Color.FromRgb(clr.R, clr.G, clr.B);

            imgColor = new Vector3(c.B / 255f, c.G / 255f, c.R / 255f);

            return imgColor;
        }

        public static string BrickImage()
        {
            return rootPath + @"\Textures\bricks.jpg";
        }
    }
}
