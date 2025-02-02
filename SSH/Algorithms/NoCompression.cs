using System;

namespace HoneyPot.SSH.Algorithms
{
    public class NoCompression : CompressionAlgorithm
    {
        public override byte[] Compress(byte[] input)
        {
            return input;
        }

        public override ReadOnlyMemory<byte> Decompress(ReadOnlyMemory<byte> input)
        {
            return input;
        }
    }
}
