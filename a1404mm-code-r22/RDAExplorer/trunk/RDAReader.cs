using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using RDAExplorer.Misc;
using System.ComponentModel;

namespace RDAExplorer
{
    public class RDAReader : IDisposable
    {
        public string FileName;
        private BinaryReader read;

        public FileHeader rdaFileHeader;
        public int rdaReadBlocks = 0;

        /// <summary>
        /// Read(!) rda file entries
        /// </summary>
        public List<RDAFile> rdaFileEntries = new List<RDAFile>();
        public List<List<RDAFile>> rdaFileBlocks = new List<List<RDAFile>>();

        /// <summary>
        /// RDA folder; use it to manipulate the file
        /// </summary>
        public RDAFolder rdaFolder = new RDAFolder();

        public BackgroundWorker backgroundWorker;
        public string backgroundWorkerLastMessage;

        public RDAReader()
        {
        }

        /// <summary>
        /// Create RDA read and READ FILE DIRECTLY
        /// </summary>
        /// <param name="file"></param>
        public RDAReader(string file)
        {
            FileName = file;
            ReadRDAFile();
        }

        private void UpdateOutput(string message)
        {
            if (UISettings.EnableConsole)
            {
                Console.WriteLine(message);
            }

            if (backgroundWorker != null)
            {
                backgroundWorkerLastMessage = message;
                backgroundWorker.ReportProgress((int)((float)read.BaseStream.Position / (float)read.BaseStream.Length * 100f));
            }
        }

        public void ReadRDAFile()
        {
            FileStream w = new FileStream(FileName, FileMode.Open);
            read = new BinaryReader(w);

            rdaFileHeader = (FileHeader)MarshalExtension.ReadToStructure(read, typeof(FileHeader));

            if (new String(rdaFileHeader.magic) != "Resource File V2.0")
                throw new Exception("No valid RDA V2.0 file!");

            rdaReadBlocks = 0;
            ReadBlock(rdaFileHeader.firstBlock);
        }

        void ReadBlock(int Offset)
        {
            rdaFileBlocks.Add(new List<RDAFile>());
            rdaReadBlocks++;
            UpdateOutput("----- Reading Block at " + Offset);

            read.BaseStream.Position = Offset;

            //Global BlockInfo
            BlockInfo info = (BlockInfo)MarshalExtension.ReadToStructure(read, typeof(BlockInfo));
            //For MemoryRresident
            RDAMemoryResidentHelper mrrdtHelper = null;

            //Read Flags
            bool IsMemoryResident = false;
            bool IsEncrypted = false;
            bool IsCompressed = false;

            if ((info.flags & 8) == 8) //Deleted
            {
                goto lb_done; //Skip block
            }
            if ((info.flags & 4) == 4) //MemoryResident
            {
                UpdateOutput("MemoryResident");
                IsMemoryResident = true;
            }
            if ((info.flags & 2) == 2) //Encrypted
            {
                UpdateOutput("Encrypted");
                IsEncrypted = true;
            }
            if ((info.flags & 1) == 1) //Compressed
            {
                UpdateOutput("Compressed");
                IsCompressed = true;
            }
            if (info.flags == 0)
            {
                UpdateOutput("No Flags");
            }

            //Read Buffer
            byte[] buffer = new byte[info.directorySize];
            read.BaseStream.Position = Offset - info.directorySize;

            if (IsMemoryResident) //MemoryResident
            {
                read.BaseStream.Position -= 8;
            }

            buffer = read.ReadBytes(buffer.Length);

            //Handle Flags
            if (IsEncrypted) //Encrypted
            {
                buffer = BinaryExtension.Decrypt(buffer);
            }
            if (IsCompressed) //Compressed
            {
                buffer = ZLib.ZLib.Uncompress(buffer, info.decompressedSize);
            }
            if (IsMemoryResident) //MemoryResident
            {
                uint compressed = read.ReadUInt32();
                uint datasize = read.ReadUInt32();
                long offset = read.BaseStream.Position - 8 - info.directorySize - compressed;

                mrrdtHelper = new RDAMemoryResidentHelper((int)offset,
                    (int)datasize,
                    (int)compressed,
                    read.BaseStream,
                    info);
            }

            //Read
            UpdateOutput("-- DirEntries:");
            ReadDirEntries(buffer, info, mrrdtHelper);

            //Goto Next Block
        lb_done:
            if (info.nextBlock < read.BaseStream.Length)
            {
                ReadBlock(info.nextBlock);
            }
            else
            {
                //Finally, generate RDA folder
                rdaFolder = RDAFolder.GenerateFrom(rdaFileEntries);

                UpdateOutput("Done. " + rdaFileEntries.Count + " files. " + rdaReadBlocks + " blocks read.");
            }
        }

        private void ReadDirEntries(byte[] buffer, BlockInfo block, RDAMemoryResidentHelper mrm)
        {
            int sz = Marshal.SizeOf(new DirEntry());

            MemoryStream stream = new MemoryStream(buffer);
            BinaryReader r = new BinaryReader(stream);

            while (stream.Position != stream.Length)
            {
                DirEntry entry = (DirEntry)MarshalExtension.ReadToStructure(r, typeof(DirEntry));

                //UpdateOutput(" - >" + entry.GetFileName());
                RDAFile readFile = RDAFile.FromUnmanaged(entry, block, read, mrm);
                rdaFileEntries.Add(readFile);
                rdaFileBlocks[rdaFileBlocks.Count - 1].Add(readFile);
            }
        }

        public void Dispose()
        {
            if (read != null)
                read.Close();

            rdaFileEntries.Clear();
            rdaFolder = null;

            foreach (Stream str in RDAFileStreamCache.Cache.Values)
                str.Close();
            RDAFileStreamCache.Cache.Clear();

            GC.Collect();
        }
    }
}
