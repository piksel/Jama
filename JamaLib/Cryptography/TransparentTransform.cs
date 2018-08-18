using System;
using System.Security.Cryptography;

namespace Piksel.GrowlLib.Cryptography
{
    internal class TransparentTransform: ICryptoTransform
    {
        public bool CanReuseTransform => true;

        public bool CanTransformMultipleBlocks => true;

        public int InputBlockSize => 128;

        public int OutputBlockSize => 128;

        public void Dispose() { }

        public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
        {
            Array.Copy(inputBuffer, inputOffset, outputBuffer, outputOffset, inputCount);
            return inputCount;
        }

        public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
        {
            var output = new byte[OutputBlockSize];
            Array.Copy(inputBuffer, inputOffset, output, 0, inputCount);
            return output;
        }
    }
}