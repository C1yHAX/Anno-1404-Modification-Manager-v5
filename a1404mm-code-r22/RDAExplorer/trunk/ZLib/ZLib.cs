using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace RDAExplorer.ZLib
{
    public class ZLib
    {
        [DllImport("zlib.DLL", EntryPoint = "uncompress")]
        extern static int uncompress(byte[] des, ref int destLen, byte[] src, int srcLen);

        [DllImport("zlib.DLL", EntryPoint = "compress")]
        extern static int compress(byte[] des, ref int destLen, byte[] src, int srcLen);

        public static byte[] Uncompress(byte[] input, int uncompressedSize)
        {
            byte[] dest = new byte[uncompressedSize];
            int result = uncompress(dest, ref uncompressedSize, input, input.Length);

            Console.WriteLine("\tDecompressing returned " + result);

            return dest;
        }

        public static byte[] Uncompress(byte[] input, int uncompressedSize, out int result)
        {
            byte[] dest = new byte[uncompressedSize];
            result = uncompress(dest, ref uncompressedSize, input, input.Length);

            Console.WriteLine("\tDecompressing returned " + result);

            return dest;
        }

        public static byte[] Compress(byte[] input)
        {
            int destlenght = input.Length;
            byte[] dest = new byte[input.Length];

            int result = compress(dest, ref destlenght, input, input.Length);

            return dest.ToList().GetRange(0, destlenght).ToArray();
        }
    }
}
