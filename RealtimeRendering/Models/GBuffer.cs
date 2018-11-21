using System;
using System.Numerics;

namespace RealtimeRendering.Models
{
    public class GBuffer
    {
        private byte[] pixelsBuff;
        private Vector3[] colorsBuff;
        private float[] zBuff;
        private Vector3[] normalBuff;
        private Vector3[] posBuff;

        public GBuffer(int winWidth, int winHeight)
        {
            PixelsBuffer = new byte[winWidth * winHeight * 4];
            ColorsBuffer = new Vector3[winWidth * winHeight];
            ZBuffer = new float[winWidth * winHeight];
            NormalBuffer = new Vector3[winWidth * winHeight];
            PosBuffer = new Vector3[winWidth * winHeight];
        }

        public byte[] PixelsBuffer { get => pixelsBuff; set => pixelsBuff = value; }
        public Vector3[] ColorsBuffer { get => colorsBuff; set => colorsBuff = value; }
        public float[] ZBuffer { get => zBuff; set => zBuff = value; }
        public Vector3[] NormalBuffer { get => normalBuff; set => normalBuff = value; }
        public Vector3[] PosBuffer { get => posBuff; set => posBuff = value; }

        /// <summary>
        /// Clear the G-Buffers and set the Z-Buffer values to Positive Infinity
        /// </summary>
        public void ClearBuffers()
        {
            Array.Clear(PixelsBuffer, 0, PixelsBuffer.Length);
            Array.Clear(ColorsBuffer, 0, ColorsBuffer.Length);
            Array.Clear(ZBuffer, 0, ZBuffer.Length);
            Array.Clear(PosBuffer, 0, PosBuffer.Length);

            for (int i = 0; i < ZBuffer.Length; i++)
            {
                ZBuffer[i] = float.PositiveInfinity;
            }
        }
    }
}
