using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace RDAExplorer
{
    public class RDABlockCreator
    {
        public enum BlockTypeEnum
        {
            /// <summary>
            /// Every Directory with files -> Block
            /// </summary>
            Directory,
            /// <summary>
            /// Each file type, one block
            /// </summary>
            FileType,
            /// <summary>
            /// Only create one, big block
            /// </summary>
            One
        }
        public static BlockTypeEnum BlockType = BlockTypeEnum.FileType;

        /// <summary>
        /// Compressed extensions
        /// </summary>
        public static List<string> FileType_CompressedExtensions = new List<string>()
        {
            ".xml",
            ".txt",
            ".cfg",
            ".ini"
        };

        public static List<RDAFolder> GenerateOf(RDAFolder root)
        {
            if(BlockType == BlockTypeEnum.Directory)
            {
                    List<RDAFolder> folders = new List<RDAFolder>();

                    if (root.Files.Count != 0)
                        folders.Add(root);
                    folders.AddRange(root.GetAllFolders().FindAll(fo => fo.Files.Count != 0));
                    return folders;
            }
            else if (BlockType == BlockTypeEnum.FileType)
            {
                Dictionary<string, RDAFolder> folders = new Dictionary<string, RDAFolder>();

                //Get files and filter
                foreach (RDAFile file in root.GetAllFiles())
                {
                    string ext = Path.GetExtension(file.FileName).ToLower();

                    if (!folders.ContainsKey(ext))
                        folders.Add(ext, new RDAFolder());

                    folders[ext].Files.Add(file);
                }

                //Set compression flag
                foreach (KeyValuePair<string, RDAFolder> f in folders)
                {
                    f.Value.RDABlockCreator_FileType_IsCompressable =
                        FileType_CompressedExtensions.Contains(f.Key);
                }

                return folders.Values.ToList();
            }
            else if (BlockType == BlockTypeEnum.One)
            {
                RDAFolder folder = new RDAFolder();
                folder.Files.AddRange(root.GetAllFiles());

                return new List<RDAFolder>() { folder };
            }

            return new List<RDAFolder>();
        }
    }
}
