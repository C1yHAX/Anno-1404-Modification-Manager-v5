using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

namespace RDAExplorer.Misc
{
    public static class StringExtension
    {
        public static string PutTogether(IEnumerable<string> input, char separator)
        {
            string i = "";
            foreach (string str in input)
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="extension"></param>
        /// <param name="condition">Add counter while Condition == true</param>
        /// <returns></returns>
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

        public static string Replace(this string str, string[] replacedstrings, string by)
        {
            foreach (string i in replacedstrings)
                str = str.Replace(i, by);
            return str;
        }

        public static string Replace(this string str, char[] replacedchars, string by)
        {
            foreach (char i in replacedchars)
                str = str.Replace(i.ToString(), by);
            return str;
        }

        public static string Extend(this string str, char by, int destinationlenght)
        {
            for (int i = str.Length; i < destinationlenght; i++)
                str += by;

            return str;
        }

        public static string RDANormalize(string file)
        {
            return file.ToLower().Replace("/", "\\").Trim('\\');
        }
    }
}
