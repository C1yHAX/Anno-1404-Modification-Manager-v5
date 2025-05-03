using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RDAExplorer;
using AnnoModificationManager5.ModificationTypes;
using System.IO;

namespace AnnoModificationManager5.Misc
{
    public static class RDAManagerExtension
    {
        public static Dictionary<string, Tuple<RDAFile, RDAReader>> _RDAFileAssignments = new Dictionary<string, Tuple<RDAFile, RDAReader>>(StringComparer.Ordinal);

        public static void RemoveRDAFileAssignment(string filestr)
        {
            filestr = FileExtension.RDANormalize(filestr);
            _RDAFileAssignments.Remove(filestr);
        }

        public static void RemoveRDAFileAssignment(RDAFile rda)
        {
            string found = null;

            foreach (var kv in _RDAFileAssignments)
            {
                if (kv.Value.Item1 == rda)
                {
                    found = kv.Key;
                    break;
                }
            }

            if (!string.IsNullOrEmpty(found))
            {
                _RDAFileAssignments.Remove(found);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rdapath">rda folder;file path</param>
        /// <returns></returns>
        public static byte[] ReadDataOfRDA(string rdapath)
        {
            return GetRDAFileFromPath(rdapath).GetData();
        }

        public static string ReadTextOfRDA(string rdapath)
        {
            return ReadTextOfRDA(rdapath, Encoding.Default);
        }

        public static string ReadTextOfRDA(string rdapath, Encoding enc)
        {
            var data = ReadDataOfRDA(rdapath);
            using (MemoryStream stream = new MemoryStream(data))
            {
                stream.Position = 0;

                using (StreamReader rd = new StreamReader(stream, enc))
                {
                    return rd.ReadToEnd();
                }
            }
        }

        public static string ReadTextOfRDA(RDAFile rda, Encoding enc)
        {
            var data = rda.GetData();
            using (MemoryStream stream = new MemoryStream(data))
            {
                stream.Position = 0;

                using (StreamReader rd = new StreamReader(stream, enc))
                {
                    return rd.ReadToEnd();
                }
            }
        }

        public static RDAFile GetRDAFileFromPath(string rdapath)
        {

            RDAReader rd;

            return GetRDAFileFromPath(rdapath, out rd);
        }

        public static RDAFile GetRDAFileFromPath(string rdapth, out RDAReader reader)
        {
            return GetRDAFileFromPath(rdapth, Modification.RDAPath, out reader);
        }

        public static RDAReader GetHPReader(string RDASub)
        {
            return Modification.RDAManager.GetReader(RDAManager.OrderRDAs(Directory.GetFiles(Modification.RDAPath + "\\" + RDASub, "*.rda").ToList()).First());
        }

        public static RDAFile GetRDAFileFromPath(string rdapath, string RDAContainingFolder, out RDAReader reader)
        {
            Tuple<RDAFile, RDAReader> existingcouple;
            rdapath = FileExtension.RDANormalize(rdapath);
            _RDAFileAssignments.TryGetValue(rdapath, out existingcouple);

            if (existingcouple == null)
            {
                var cell = rdapath.Split(';');
                string rdaselfolder = RDAContainingFolder + "\\" + cell[0];

                var rd = Modification.RDAManager.LookForFileIn(cell[1], Directory.GetFiles(rdaselfolder, "*.rda").ToList());

                if (rd == null)
                {
                    reader = null;
                    return null;
                }

                RDAFile foundfile = Modification.RDAManager.GetFile(rd, cell[1]);

                _RDAFileAssignments.Add(rdapath,
                    new Tuple<RDAFile, RDAReader>(foundfile, rd));

                reader = rd;
                return foundfile;
            }

            reader = existingcouple.Item2;
            return existingcouple.Item1;
        }

        public static void RemoveRDAFromReader(RDAFile file, RDAReader rd)
        {
            var couple = _RDAFileAssignments.Where((kv) =>
                {
                    return kv.Value.Item1 == file;
                });

            if (couple != null && couple.Count() != 0)
            {
                _RDAFileAssignments.Remove(couple.First().Key);
            }

            rd.rdaFileEntries.Remove(file);
            Modification.AMMRDA.CommitChange(rd, RDA.AMMRDAManager.RDARequestType.SaveChanges);
        }

        public static void Clear()
        {
            _RDAFileAssignments.Clear();
        }
    }
}
