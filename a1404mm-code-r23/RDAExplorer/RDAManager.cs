using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using RDAExplorer.Misc;

namespace RDAExplorer
{
    /// <summary>
    /// Stores and manages multiple RDA files
    /// </summary>
    public class RDAManager
    {
        private Dictionary<string, RDAReader> _OpenedRDAs = new Dictionary<string, RDAReader>();

        public RDAManager()
        {

        }

        public void DisposeAll()
        {
            foreach (var rd in _OpenedRDAs.Values)
            {
                rd.Dispose();
            }

            _OpenedRDAs.Clear();
        }

        public string GetMostImportantFileInFolder(string folder)
        {
            return OrderRDAs(ListRDAsInFolder(folder)).First();
        }

        public List<string> ListRDAsInFolder(string folder)
        {
            folder = folder.Replace('/', '\\').Trim('\\');
            var output = new List<string>();

            foreach (var file in Directory.GetFiles(folder, "*.rda", SearchOption.AllDirectories))
            {
                var rootedfile = file.Remove(0, folder.Length).Trim('\\');
                output.Add(rootedfile);
            }

            return output;
        }

        public RDAFile GetFile(RDAReader reader, string path)
        {
            path = StringExtension.RDANormalize(path);

            return reader.rdaFileEntries.Find(r => path == StringExtension.RDANormalize(r.FileName));
        }

        public RDAReader GetReader(string filename)
        {
            filename = filename.ToLower();
            RDAReader output;
            _OpenedRDAs.TryGetValue(filename, out output);

            if (output == null)
            {
                if (File.Exists(filename))
                {
                    output = new RDAReader(filename);
                    _OpenedRDAs.Add(filename, output);
                }
                else
                    return null;
            }

            return output;
        }

        public RDAReader LookForFile(string file)
        {
            file = file.ToLower().Replace("/", "\\").Trim('\\');

            var list = OrderRDAs(_OpenedRDAs.Values.ToList());

            foreach (var rda in list)
            {
                var found = rda.rdaFileEntries.Any(RDAFile =>
                {
                    var fx = RDAFile.FileName.ToLower().Replace("/", "\\").Trim('\\');
                    return file == fx;
                });

                if (found)
                    return rda;
            }

            return null;
        }

        public RDAReader LookForFileIn(string file, List<string> rdas)
        {
            file = file.ToLower().Replace("/", "\\").Trim('\\');
            var list = OrderRDAs(rdas);

            foreach (var rdaf in list)
            {
                var rda = GetReader(rdaf);

                var found = rda.rdaFileEntries.Any(RDAFile =>
                    {
                        var fx = RDAFile.FileName.ToLower().Replace("/", "\\").Trim('\\');
                        return file == fx;
                    });

                if (found)
                    return rda;
            }

            return null;
        }

        public List<RDAReader> LookForFilesIn(string file, List<string> rdas)
        {
            file = file.ToLower().Replace("/", "\\").Trim('\\');
            var list = OrderRDAs(rdas);
            var output = new List<RDAReader>();

            foreach (var rdaf in list)
            {
                var rda = GetReader(rdaf);

                var found = rda.rdaFileEntries.Any(RDAFile =>
                {
                    var fx = RDAFile.FileName.ToLower().Replace("/", "\\").Trim('\\');
                    return file == fx;
                });

                if (found)
                    output.Add(rda);
            }

            return output;
        }

        public bool FileExists(string file, RDAReader reader)
        {
            file = StringExtension.RDANormalize(file);

            return reader.rdaFileEntries.Any(w => StringExtension.RDANormalize(w.FileName) == file);
        }

        public static List<RDAReader> OrderRDAs(List<RDAReader> input)
        {
            return OrderRDAs(input, 1);
        }

        public static List<RDAReader> OrderRDAs(List<RDAReader> input, int preferaddon)
        {
            return input.OrderByDescending(str =>
            {
                string s = str.FileName.ToLower();

                int index = -1;
                {
                    char c = Path.GetFileNameWithoutExtension(s).Last();

                    index = (int)char.GetNumericValue(c);
                }

                if (s.Contains("addon"))
                    index += 50 * preferaddon;
                if (s.Contains("patch"))
                    return 10 + index;
                return index;
            }).ToList();
        }

        public static List<string> OrderRDAs(List<string> input)
        {
            return OrderRDAs(input, 1);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <param name="preferaddon">0: ignore, 1:prefer, -1:dont prefer</param>
        /// <returns></returns>
        public static List<string> OrderRDAs(List<string> input, int preferaddon)
        {
            return input.OrderByDescending(str =>
                {
                    string s = str.ToLower();

                    int index = -1;
                    {
                        char c = Path.GetFileNameWithoutExtension(s).Last();

                        index = (int)char.GetNumericValue(c);
                    }

                    if (s.Contains("addon"))
                        index += 50 * preferaddon;
                    if (s.Contains("patch"))
                        return 10 + index;
                    return index;
                }).ToList();
        }
    }
}
