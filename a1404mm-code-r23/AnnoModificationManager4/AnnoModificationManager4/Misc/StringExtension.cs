using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using AnnoModificationManager4.ModificationTypes;
using AnnoModificationManager4.Components;
using System.IO;

namespace AnnoModificationManager4.Misc
{
    public static class StringExtension
    {
        /// <summary>
        /// eg. Replace %Project% with folder
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string FormatDevelopmentFolders(this string str)
        {
            if (Modification.Development_CurrentModification != null)
            {
                str = str.Replace("%Project%", Modification.Development_CurrentModification.Folder);
            }
            return str;
        }

        /// <summary>
        /// eg. Replace folder with %Project% 
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ShortenDevelopmentFolders(this string str)
        {
            if (Modification.Development_CurrentModification != null)
            {
                str = str.Replace(Modification.Development_CurrentModification.Folder, "%Project%");
            }
            return str;
        }

        /// <summary>
        /// /// 
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string FormatGlobalFolders(this string str)
        {
            return str.Replace("%Anno%", AnnoDirectoryHandler.GetCurrent()).
                Replace("%AppData%", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).Trim('\\'));
        }

        public static string DeFormatGlobalFolders(this string str)
        {
            return str.Replace( AnnoDirectoryHandler.GetCurrent(),"%Anno%").
                Replace(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).Trim('\\'), "%AppData%");
        }

        /// <summary>
        /// Formats for %Project%\\OriginalFiles
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string FormatProjectPath(this string str)
        {
            return str.Replace("/", "_").Replace("\\", "_").Replace("%", "#");
        }

        public static string PutTogether(IEnumerable<string> input, char separator)
        {
            string i = "";
            foreach(string str in input)
            {
                i += str + separator;
            }

            return i.Trim(separator);
        }

        public static string Short(this string str, int count)
        {
            if (str.Length > count)
                return str.Remove(count);
            return str;
        }

        public static string PutTogetherComma(IEnumerable<string> input)
        {
            string i = "";
            foreach (string str in input)
            {
                i += str + ", ";
            }

            return i.Trim(new char[] { ',', ' ' });
        }

        public static string PutTogetherReversed(IEnumerable<string> input, char separator)
        {
            string i = "";
            foreach (string str in input.Reverse())
            {
                i += str + separator;
            }

            return i.Trim(separator);
        }       

        public static string MakeUnique(string filename, string extension, Func<string, bool> condition)
        {           
            int current = 0;
            string currentadd = "";

            while (condition(filename + currentadd + extension))
            {
                current++;
                currentadd = current.ToString();
            }

            return filename + currentadd + extension;
        }
    }
}
