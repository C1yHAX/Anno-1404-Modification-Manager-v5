using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using RDAExplorer.Misc;
using System.ComponentModel;

namespace RDAExplorer
{
    public class RDAFile
    {
        public enum Flag
        {
            None = 0,
            Compressed = 1,
            Encrypted = 2,
            MemoryResident = 4,
            Deleted = 8
        }

        public enum RDAFileMode
        {
            RDA,
            CustomFile,
            CustomBuffer
        }


        ///////////
        //Property FileName
        ///////////
        private string _FileName;
        public string FileName
        {
            get
            {
                return _FileName;
            }
            set
            {
                _FileName = value.Replace("\\", "/").Trim('/');
            }
        }

        public Flag Flags = Flag.None;

        public int Offset;
        public int UncompressedSize;
        public int CompressedSize;
        public DateTime TimeStamp;

        public BinaryReader BinaryFile;

        //Added files
        public string OverwrittenFilePath = "";
        public byte[] OnverwrittenData;
        public RDAFileMode CurrentFileMode = RDAFileMode.RDA;

        public RDAFile()
        {
        }

        /// <summary>
        /// Uncompressed data
        /// </summary>
        /// <param name="data"></param>
        public void SetData(byte[] data)
        {
            Offset = 0;
            CompressedSize = data.Length;
            UncompressedSize = data.Length;
            TimeStamp = DateTime.Now;

            BinaryFile = new BinaryReader(new MemoryStream(data));
            BinaryFile.BaseStream.Position = 0;

            CurrentFileMode = RDAFileMode.CustomBuffer;
        }

        public void SetFile(string file)
        {
            //Set Flag!!
            //Flags = Flag.None;

            FileInfo file_info = new FileInfo(file);

            OverwrittenFilePath = file;
            TimeStamp = file_info.LastWriteTime;
            Offset = 0;
            CompressedSize = (int)file_info.Length;
            UncompressedSize = (int)file_info.Length;

            FileStream stream = RDAFileStreamCache.Open(file);

            if (stream != null)
            {
                BinaryFile = new BinaryReader(stream);
                CurrentFileMode = RDAFileMode.CustomFile;
            }
        }

        public byte[] GetData()
        {
            switch (CurrentFileMode)
            {
                case RDAFileMode.RDA:
                    BinaryFile.BaseStream.Position = Offset;
                    byte[] buffer = BinaryFile.ReadBytes(CompressedSize);

                    if (string.IsNullOrEmpty(OverwrittenFilePath))
                    //Files are always uncompressed and not encrypted                
                    {
                        if ((Flags & Flag.Encrypted) == Flag.Encrypted)
                        {
                            buffer = BinaryExtension.Decrypt(buffer);
                        }
                        if ((Flags & Flag.Compressed) == Flag.Compressed)
                        {
                            buffer = ZLib.ZLib.Uncompress(buffer, UncompressedSize);
                        }
                    }

                    return buffer;
                case RDAFileMode.CustomFile:
                    BinaryFile.BaseStream.Position = 0;
                    return BinaryFile.BaseStream.ReadAll();
                case RDAFileMode.CustomBuffer:
                    goto case RDAFileMode.CustomFile;
                default:
                    throw new NotSupportedException();
            }
        }

        public void ExtractToRoot(string folder)
        {
            string destinationfile = folder + "\\" + FileName.Replace("/", "\\");
            string destinationdirectory = Path.GetDirectoryName(destinationfile);

            if (!Directory.Exists(destinationdirectory))
                Directory.CreateDirectory(destinationdirectory);

            Extract(destinationfile);
        }

        public void Extract(string destinationfile)
        {
            byte[] buffer = GetData();

            using (FileStream stream = new FileStream(destinationfile, FileMode.Create))
            {
                stream.Write(buffer, 0, buffer.Length);
            }

            (new FileInfo(destinationfile)).LastWriteTime = TimeStamp;
        }

        public static RDAFile FromUnmanaged(DirEntry dir, BlockInfo block, BinaryReader reader, RDAMemoryResidentHelper mrm)
        {
            RDAFile file = new RDAFile();
            file.FileName = dir.GetFileName();

            //flags
            if ((block.flags & 4) != 4) //Flags are for MemoryResidentHelper
            {
                if ((block.flags & 1) == 1)
                    file.Flags |= Flag.Compressed;
                if ((block.flags & 2) == 2)
                    file.Flags |= Flag.Encrypted;
            }
            if ((block.flags & 4) == 4)
                file.Flags |= Flag.MemoryResident;
            if ((block.flags & 8) == 8)
                file.Flags |= Flag.Deleted;

            file.Offset = dir.offset;

            file.UncompressedSize = dir.filesize;
            file.CompressedSize = dir.compressed;
            file.TimeStamp = DateTimeExtension.FromTimeStamp(dir.timestamp);

            //Set binary file to original source or memoryResidentHelper
            file.BinaryFile = mrm == null ? reader : new BinaryReader(mrm.Data);

            return file;
        }

        public static RDAFile Create(string file, string folderpath)
        {
            FileInfo file_info = new FileInfo(file);

            RDAFile rdafile = new RDAFile();
            rdafile.FileName = FileNameToRDAFileName(file, folderpath);
            rdafile.OverwrittenFilePath = file;
            rdafile.TimeStamp = file_info.LastWriteTime;
            rdafile.Offset = 0;
            rdafile.CompressedSize = (int)file_info.Length;
            rdafile.UncompressedSize = (int)file_info.Length;

            FileStream stream = RDAFileStreamCache.Open(file);

            if (stream != null)
            {
                rdafile.BinaryFile = new BinaryReader(stream);
                return rdafile;
            }

            return null;
        }

        public static string FileNameToRDAFileName(string file, string folderpath)
        {
            file = file.Replace("\\", "/").Trim('/');
            folderpath = folderpath.Replace("\\", "/").Trim('/');

            return (folderpath + "/" + Path.GetFileName(file)).Trim('/');
        }
    }

    public static class RDAFileExtension
    {
        public static string ExtractAll_LastMessage = "";

        public static void ExtractAll(this List<RDAFile> files, string folder)
        {
            if (UISettings.EnableConsole)
                Console.Clear();

            for (int i = 0; i < files.Count; i++)
            {
                if (UISettings.EnableConsole)
                {
                    Console.WriteLine(((float)i / (float)files.Count * 100f) + "%");
                }

                files[i].ExtractToRoot(folder);
            }
        }

        public static void ExtractAll(this List<RDAFile> files, string folder, BackgroundWorker wrk)
        {
            if (UISettings.EnableConsole)
                Console.Clear();

            for (int i = 0; i < files.Count; i++)
            {
                if (i % UISettings.Progress_UpdateFileCount == 0)
                {
                    if (UISettings.EnableConsole)
                    {
                        Console.WriteLine(((float)i / (float)files.Count * 100f) + "%");
                    }
                    else
                    {
                        wrk.ReportProgress((int)((float)i / (float)files.Count * 100f));
                    }

                    ExtractAll_LastMessage = (i + 1) + " files written";
                }

                files[i].ExtractToRoot(folder);
            }
        }
    }
}
