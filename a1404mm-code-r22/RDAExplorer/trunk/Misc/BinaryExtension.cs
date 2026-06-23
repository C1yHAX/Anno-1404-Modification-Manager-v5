using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace RDAExplorer.Misc
{
    public class BinaryExtension
    {
        public static byte[] Decrypt(byte[] buffer)
        {
            if (buffer.Length == 0)
                return buffer;

            //if (buffer.Length % 2 != 0)
            //{
            //    List<byte> _buffer = new List<byte>(buffer);
            //    _buffer.Add(0);
            //    buffer = _buffer.ToArray();
            //}

            MemoryStream stream=new MemoryStream(buffer);
            BinaryReader reader = new BinaryReader(stream);           

            //Collect short
            List<short> collectedshortList = new List<short>();
            //for (int i = 0; i < buffer.Length / 2; i++)
            while(true)
            {
                collectedshortList.Add(reader.ReadInt16());

                if (reader.BaseStream.Position + 2 > reader.BaseStream.Length)
                    break;
            }

            reader.Close();

            //Output
            MemoryStream writerStream=new MemoryStream();
            BinaryWriter writer = new BinaryWriter(writerStream);

            int x = 0xA2C2A;
            short y = 0;

            for (int i = 0; i < collectedshortList.Count; i++)
            {
                x = x * 0x343FD + 0x269EC3;
                y = (short)(x >> 16 & 0x7FFF);

                short w = (short)(collectedshortList[i] ^ y);

                writer.Write(w);
            }

            if (buffer.Length % 2 != 0)
            {
                writer.Write(buffer[buffer.Length - 1]);
            }

            byte[] output = writerStream.ReadAll();
           
            return output;
        }
    }
}
