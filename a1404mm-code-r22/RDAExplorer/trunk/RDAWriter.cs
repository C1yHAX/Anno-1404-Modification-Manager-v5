using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using RDAExplorer.Misc;
using RDAExplorer.Misc;
using System.ComponentModel;

namespace RDAExplorer
{
    public class RDAWriter
    {
        public RDAFolder Folder;
        public string UI_LastMessage = "";

        public RDAWriter(RDAFolder folder)
        {
            Folder = folder;
        }

        public RDAWriter(List<RDAFile> files)
        {
            Folder = RDAFolder.GenerateFrom(files);
        }

        public void Write(string Filename, bool compress, BackgroundWorker wrk)
        {
            FileStream writer = new FileStream(Filename, FileMode.Create);

            //First, write File header
            FileHeader fileHeader = FileHeader.Create();
            writer.Write(MarshalExtension.WriteToByte(fileHeader), 0);

            // --- Write Blocks 
            List<RDAFolder> folders = RDABlockCreator.GenerateOf(Folder);

            BlockInfo[] BlockInfoOffsets_BlockInfos = new BlockInfo[folders.Count];
            int[] BlockInfoOffsets_Offsets = new int[folders.Count];

            for (int i = 0; i < folders.Count; i++)
            {
                RDAFolder folder = folders[i];

                bool folder_compressfiles = compress && (folder.RDABlockCreator_FileType_IsCompressable == null |
                    folder.RDABlockCreator_FileType_IsCompressable == true);

                //Update UI
                if (wrk != null)
                {
                    UI_LastMessage = "Writing Block " + (i + 1) + "/" + folders.Count + " => " + folder.Files.Count + " files";
                    wrk.ReportProgress((int)((float)i / (float)folders.Count * 100f));
                }

                //Write FileData and collect
                Dictionary<RDAFile, int> FileOffsets = new Dictionary<RDAFile, int>();
                Dictionary<RDAFile, int> FileCompressedSizes = new Dictionary<RDAFile, int>();
                foreach (RDAFile file in folder.Files)
                {
                    byte[] buffer = file.GetData();

                    //Compress
                    if (folder_compressfiles)
                    {
                        buffer = ZLib.ZLib.Compress(buffer);
                    }

                    FileOffsets.Add(file, (int)writer.Position); //Collect
                    FileCompressedSizes.Add(file, buffer.Length); // -""-

                    writer.Write(buffer); //Write
                }

                //Write DirEntries
                List<byte> WrittenDirectories = new List<byte>(); //Save dir
                foreach (RDAFile file in folder.Files)
                {
                    DirEntry entry = new DirEntry();
                    entry.compressed = FileCompressedSizes[file]; //Only write uncompressed!
                    entry.filesize = file.UncompressedSize;
                    entry.filename = file.FileName.Extend('\0', 260).ToCharArray();
                    entry.timestamp = (int)file.TimeStamp.ToFileTime();
                    entry.uk = 0;
                    entry.offset = FileOffsets[file];

                    WrittenDirectories.AddRange(MarshalExtension.WriteToByte(entry));
                }

                //Compress
                if (folder_compressfiles)
                {
                    WrittenDirectories = ZLib.ZLib.Compress(WrittenDirectories.ToArray()).ToList();
                }
                //Write
                writer.Write(WrittenDirectories.ToArray());

                //Write BlockInfo
                BlockInfo blockinfo = new BlockInfo();
                blockinfo.decompressedSize = folder.GetDirEntryBlockSize();
                blockinfo.directorySize = WrittenDirectories.Count;
                blockinfo.fileCount = folder.Files.Count;
                blockinfo.flags = 0;

                if (folder_compressfiles)
                {
                    blockinfo.flags = 1;
                }

                BlockInfoOffsets_BlockInfos[i] = blockinfo;
                BlockInfoOffsets_Offsets[i] = (int)writer.Position;

                //First Assign
                if (i == 0)
                {
                    fileHeader.firstBlock = (int)writer.Position;
                }
                else
                {
                    BlockInfoOffsets_BlockInfos[i - 1].nextBlock =
                        (int)writer.Position;
                }

                //Then write dummy data (Will be written later)
                writer.Write(MarshalExtension.WriteToByte(blockinfo));
            }

            //Edit last BlockInfo
            BlockInfoOffsets_BlockInfos[BlockInfoOffsets_BlockInfos.Length - 1].nextBlock =
                (int)writer.Length;

            //Finally, refresh blockInfos
            writer.Write(MarshalExtension.WriteToByte(fileHeader), 0);

            for (int i = 0; i < BlockInfoOffsets_BlockInfos.Length; i++)
            {
                writer.Write(MarshalExtension.WriteToByte(
                    BlockInfoOffsets_BlockInfos[i]),
                    BlockInfoOffsets_Offsets[i]);
            }

            writer.Close();
        }
    }
}
