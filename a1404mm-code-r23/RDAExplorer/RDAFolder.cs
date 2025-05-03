using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RDAExplorer.Misc;
using System.IO;
using System.Runtime.InteropServices;

namespace RDAExplorer
{
    public class RDAFolder
    {
        public string FullPath = "";
        public string Name = "";
        public List<RDAFile> Files = new List<RDAFile>();
        public List<RDAFolder> Folders = new List<RDAFolder>();

        public RDAFolder Parent;

        /// <summary>
        /// Used by BlockCreator
        /// </summary>
        public bool? RDABlockCreator_FileType_IsCompressable = null;

        public RDAFolder()
        {
        }

        public List<RDAFile> GetAllFiles()
        {
            List<RDAFile> u = new List<RDAFile>();
            u.AddRange(Files);

            foreach (RDAFolder f in Folders)
                u.AddRange(f.GetAllFiles());

            return u;
        }

        public List<RDAFolder> GetAllFolders()
        {
            List<RDAFolder> u = new List<RDAFolder>();
            u.AddRange(Folders);

            foreach (RDAFolder f in Folders)
                u.AddRange(f.GetAllFolders());

            return u;
        }

        public List<string> GetAllExtensions()
        {
            List<string> u = new List<string>();
            foreach (RDAFile file in Files)
            {
                string ext = Path.GetExtension(file.FileName);

                if (!u.Contains(ext))
                    u.Add(ext);
            }

            foreach (RDAFolder f in Folders)
                u.AddRange(f.GetAllExtensions());

            return u.Distinct().ToList();
        }

        public void AddFiles(List<RDAFile> files)
        {
            foreach (RDAFile f in files)
            {
                if (f.FileName.Contains("/"))
                {
                    RDAFolder fold = NavigateTo(GetRoot(), Path.GetDirectoryName(f.FileName), "");
                    fold.Files.Add(f);
                }
                else
                {
                    Files.Add(f);
                }
            }
        }

        public RDAFolder GetRoot()
        {
            if (FullPath == "")
                return this;
            return Parent.GetRoot();
        }

        public static RDAFolder GenerateFrom(List<RDAFile> file)
        {
            RDAFolder root = new RDAFolder();
            root.Files.AddRange(file.FindAll(f => !f.FileName.Contains("/")));

            foreach (RDAFile f in file.FindAll(f => f.FileName.Contains("/")))
            {
                RDAFolder container = NavigateTo(root, Path.GetDirectoryName(f.FileName), "");
                container.Files.Add(f);
            }

            return root;
        }

        private static RDAFolder NavigateTo(RDAFolder root, string FullPath, string CurrentPos)
        {
            FullPath = FullPath.Replace("\\", "/");
            CurrentPos = CurrentPos.Replace("\\", "/");

            FullPath = FullPath.Trim('/');
            CurrentPos = CurrentPos.Trim('/');

            List<string> segments = FullPath.Split('/').ToList();
            string currentSegment = segments[0];

            RDAFolder foundFolder = null;
            foreach (RDAFolder f in root.Folders)
            {
                if (f.Name == currentSegment)
                {
                    foundFolder = f;
                    break;
                }
            }

            //Gen
            if (foundFolder == null)
            {
                RDAFolder fd = new RDAFolder();
                fd.Parent = root;
                fd.Name = currentSegment;
                fd.FullPath = CurrentPos + "/" + currentSegment;

                root.Folders.Add(fd);

                foundFolder = fd;
            }

            //Nav
            if (segments.Count == 1)
                return foundFolder;
            else
            {
                segments.RemoveAt(0);
                return NavigateTo(foundFolder,
                    StringExtension.PutTogether(segments, '/')
                    , CurrentPos + "/" + currentSegment);
            }
        }

        #region Marshal
        public int GetDataSize()
        {
            int i = 0;
            foreach (RDAFile file in Files)
                i += file.GetData().Length;

            return i;
        }

        public int GetDirEntryBlockSize()
        {
            return Files.Count * Marshal.SizeOf(new DirEntry());
        }
        #endregion
    }
}
