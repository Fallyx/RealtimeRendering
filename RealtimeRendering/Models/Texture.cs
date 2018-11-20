using System;
using Media = System.Windows.Media;
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
            Vector3 yellowLine = Vector3.One;

            float s = tc.X;
            float t = tc.Y;

            float s_ = tc.X * Img.Width;
            float t_ = tc.Y * Img.Height;

            int si = Math.Min((int)(s * Img.Width) & (Img.Width - 1), Img.Width - 2);
            int ti = Math.Min((int)(t * Img.Height) & (Img.Height - 1), Img.Height - 2);

            Color clrST = Img.GetPixel(si, ti);
            Color clrST1 = Img.GetPixel(si, ti + 1);
            Color clrS1T = Img.GetPixel(si + 1, ti);
            Color clrS1T1 = Img.GetPixel(si + 1, ti + 1);

            Media.Color cST = Media.Color.FromRgb(clrST.R, clrST.G, clrST.B);
            Vector3 imgColorST = new Vector3(cST.B / 255f, cST.G / 255f, cST.R / 255f);

            Media.Color cST1 = Media.Color.FromRgb(clrST1.R, clrST1.G, clrST1.B);
            Vector3 imgColorST1 = new Vector3(cST1.B / 255f, cST1.G / 255f, cST1.R / 255f);

            Media.Color cS1T = Media.Color.FromRgb(clrS1T.R, clrS1T.G, clrS1T.B);
            Vector3 imgColorS1T = new Vector3(cS1T.B / 255f, cS1T.G / 255f, cS1T.R / 255f);

            Media.Color cS1T1 = Media.Color.FromRgb(clrS1T1.R, clrS1T1.G, clrS1T1.B);
            Vector3 imgColorS1T1 = new Vector3(cS1T1.B / 255f, cS1T1.G / 255f, cS1T1.R / 255f);

            Vector3 redLine = Vector3.Lerp(imgColorST, imgColorST1, t_ - ti);
            Vector3 greenLine = Vector3.Lerp(imgColorS1T, imgColorS1T1, t_ - ti);

            yellowLine = Vector3.Lerp(redLine, greenLine, s_ - si);

            return yellowLine;
        }

        public static string BrickImage()
        {
            return rootPath + @"\Textures\bricks.jpg";
        }
    }
}
