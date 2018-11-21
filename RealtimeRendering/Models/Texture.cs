using System;
using Media = System.Windows.Media;
using System.Drawing;
using System.Numerics;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace RealtimeRendering.Models
{
    public class Texture
    {
        private string texturePath;
        private Bitmap img;
        private byte[] bmpColors;

        public Texture(string texturePath)
        {
            TexturePath = texturePath;
            Img = new Bitmap(texturePath);
            Rectangle rect = new Rectangle(0, 0, Img.Width, Img.Height);
            BitmapData bmpData = Img.LockBits(rect, ImageLockMode.ReadOnly, Img.PixelFormat);

            int bytes = bmpData.Stride * Img.Height;
            BmpColors = new byte[bytes];

            // Get the address of the first line
            IntPtr ptr = bmpData.Scan0;

            // Copy color values into the array
            Marshal.Copy(ptr, BmpColors, 0, bytes);

            Img.UnlockBits(bmpData);
        }

        public string TexturePath { get => texturePath; private set => texturePath = value; }
        public Bitmap Img { get => img; set => img = value; }
        public byte[] BmpColors { get => bmpColors; set => bmpColors = value; }

        /// <summary>
        /// Get the color from the texture
        /// </summary>
        /// <param name="tc"></param>
        /// <returns>Vector3 texture color</returns>
        public Vector3 LookUp(Vector2 tc)
        {
            Vector3 textureClr = Vector3.One;

            float s = tc.X;
            float t = tc.Y;

            int si = Math.Min((int)(s * Img.Width) & (Img.Width - 1), Img.Width - 2);
            int ti = Math.Min((int)(t * Img.Height) & (Img.Height - 1), Img.Height - 2);

            Vector3 clrST = GetPixel(si, ti);

            textureClr = clrST;

            return textureClr;
        }

        /// <summary>
        /// Get the color from the texture with bilinear filter
        /// </summary>
        /// <param name="tc"></param>
        /// <returns>Vector3 texture color bilinear filtered</returns>
        public Vector3 LookUpBilinear(Vector2 tc)
        {
            Vector3 yellowLine = Vector3.One;

            float s = tc.X;
            float t = tc.Y;

            float s_ = tc.X * Img.Width;
            float t_ = tc.Y * Img.Height;

            int si = Math.Min((int)(s * Img.Width) & (Img.Width - 1), Img.Width - 2);
            int ti = Math.Min((int)(t * Img.Height) & (Img.Height - 1), Img.Height - 2);            
            
            Vector3 imgColorST = GetPixel(si, ti);
            Vector3 imgColorST1 = GetPixel(si, ti + 1);
            Vector3 imgColorS1T = GetPixel(si + 1, ti);
            Vector3 imgColorS1T1 = GetPixel(si + 1, ti + 1);

            Vector3 redLine = Vector3.Lerp(imgColorST, imgColorST1, t_ - ti);
            Vector3 greenLine = Vector3.Lerp(imgColorS1T, imgColorS1T1, t_ - ti);

            yellowLine = Vector3.Lerp(redLine, greenLine, s_ - si);

            return yellowLine;
        }

        /// <summary>
        /// Get the pixel color from the color array
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private Vector3 GetPixel(int x, int y)
        {
            int idx = (y * Img.Height + x) * 3;
            byte b = BmpColors[idx];
            byte g = BmpColors[idx + 1];
            byte r = BmpColors[idx + 2];

            Media.Color clr = Media.Color.FromRgb(r, g, b);

            return new Vector3(clr.B / 255f, clr.G / 255f, clr.R / 255f);
        }

        public static string BrickImage()
        {
            return  @"Textures\bricks.jpg";
        }

        public static string RedImage()
        {
            return @"Textures\red.png";
        }
    }
}
