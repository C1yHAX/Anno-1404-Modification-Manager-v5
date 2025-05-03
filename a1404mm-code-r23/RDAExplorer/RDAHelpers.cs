using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using RDAExplorer.Misc;

namespace RDAExplorer
{
    public class RDAMemoryResidentHelper : IDisposable
    {
        public int Offset;
        public int DataSize;
        public int Compressed;

        public MemoryStream Data;
        public BlockInfo Info;

        public RDAMemoryResidentHelper(int offset, int datasize, int compressed, Stream datasource, BlockInfo info)
        {
            Offset = offset;
            DataSize = datasize;
            Compressed = compressed;
            Info = info;

            Data = new MemoryStream();

            byte[] buffer = new byte[compressed];
            datasource.Position = offset;
            datasource.Read(buffer, 0, compressed);

            if ((info.flags & 2) == 2)
                buffer = BinaryExtension.Decrypt(buffer);
            if ((info.flags & 1) == 1)
                buffer = ZLib.ZLib.Uncompress(buffer, datasize);

            Data.Write(buffer);
        }

        public void Dispose()
        {
            Data.Close();
        }       
    }
}
