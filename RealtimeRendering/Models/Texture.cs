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

        public Vector3 LookUp(Vector2 tc)
        {
            Vector3 imgColor = Vector3.One;

            float s = tc.X;
            float t = tc.Y;

            int si = (int)(s * Img.Width) & (Img.Width - 1);
            int ti = (int)(t * Img.Height) & (Img.Height - 1);

            var clr = Img.GetPixel(si, ti);
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
