using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace RDAExplorer
{
    public class RDAFileStreamCache
    {
        public static Dictionary<string, FileStream> Cache = new Dictionary<string, FileStream>();

        public static FileStream Open(string file)
        {
            try
            {
                if (!Cache.ContainsKey(file))
                {
                    var c = new FileStream(file, FileMode.Open);
                    Cache.Add(file, c);

                    return c;
                }

                return Cache[file];
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
