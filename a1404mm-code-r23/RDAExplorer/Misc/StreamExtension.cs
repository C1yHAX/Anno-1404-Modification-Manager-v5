using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace RDAExplorer.Misc
{
    public static class StreamExtension
    {
        public static void Write(this Stream stream, byte[] buffer)
        {
            stream.Write(buffer, 0, buffer.Length);
        }

        public static void Write(this Stream stream, byte[] buffer, int position)
        {
            stream.Position = position;
            stream.Write(buffer, 0, buffer.Length);
        }

        public static byte[] ReadAll(this Stream stream)
        {
            stream.Position = 0;
            byte[] buffer = new byte[stream.Length];

            stream.Read(buffer, 0, (int)stream.Length);

            return buffer;
        }
    }
}
