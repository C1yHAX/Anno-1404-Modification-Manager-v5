using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace AnnoModificationManager4.Misc
{
    public class FileExtension
    {
        const int BYTES_TO_READ = sizeof(Int64);

        public static bool FilesAreEqual(FileInfo first, FileInfo second)
        {
            if (first.Length != second.Length)
                return false;

            int iterations = (int)Math.Ceiling((double)first.Length / BYTES_TO_READ);

            using (FileStream fs1 = first.OpenRead())
            using (FileStream fs2 = second.OpenRead())
            {
                byte[] one = new byte[BYTES_TO_READ];
                byte[] two = new byte[BYTES_TO_READ];

                for (int i = 0; i < iterations; i++)
                {
                    fs1.Read(one, 0, BYTES_TO_READ);
                    fs2.Read(two, 0, BYTES_TO_READ);

                    if (BitConverter.ToInt64(one, 0) != BitConverter.ToInt64(two, 0))
                        return false;
                }
            }

            return true;
        }

        public static bool FilesAreEqual(FileInfo first, byte[] second, ref int nonequalposition)
        {
            if (first.Length != second.Length)
                return false;

            int iterations = (int)Math.Ceiling((double)first.Length / BYTES_TO_READ);

            using (FileStream fs1 = first.OpenRead())
            {
                byte[] one = new byte[BYTES_TO_READ];
                byte[] two = new byte[BYTES_TO_READ];

                for (int i = 0; i < iterations; i++)
                {
                    fs1.Read(one, 0, BYTES_TO_READ);

                    int position = i * BYTES_TO_READ;

                    if (position + BYTES_TO_READ < second.Length)
                    {
                        Buffer.BlockCopy(second, position, two, 0, BYTES_TO_READ);
                    }
                    else
                    {
                        int diff = second.Length - position;
                        one.CopyTo(two, 0);
                        Buffer.BlockCopy(second, position, two, 0, diff);

                        //if (diff != 0)
                        //{
                        //    Buffer.BlockCopy(second, position, two, 0, diff);
                        //}
                        //int bid = 0;

                        //for (int x = position; x < second.Length; x++)
                        //{
                        //    two[bid] = second[x];

                        //    bid++;
                        //}
                    }

                    if (BitConverter.ToInt64(one, 0) != BitConverter.ToInt64(two, 0))
                    {
                        nonequalposition = position;
                        return false;
                    }
                }
            }

            return true;
        }

        public static bool FileIsInUse(string file)
        {
            try
            {
                FileStream stream = new FileStream(file, FileMode.Open, FileAccess.ReadWrite);
                stream.Close();
            }
            catch (IOException)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Same as Directory.Unify
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static string Unify(string filename)
        {
            string op = filename;

            while (File.Exists(op))
            {
                op = filename + RandomProvider.Random.Next(111111, 9999999);
            }

            return op;
        }

        /// <summary>
        /// Make unique and preserve extension
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static string MakeFileUnique(string filename)
        {
            return StringExtension.MakeUnique(Path.ChangeExtension(filename, null), Path.GetExtension(filename),
                file => File.Exists(file));
        }

        public static bool CompareFiles(string file1, string file2)
        {
            if (!File.Exists(file1) || !File.Exists(file2))
                return false;
            #region Lenght Compare
            if ((new FileInfo(file1)).Length != (new FileInfo(file2)).Length)
                return false;
            #endregion
            #region MD5 Compare
            FileStream f1 = new FileStream(file1, FileMode.Open);
            FileStream f2 = new FileStream(file2, FileMode.Open);



            /*string md51 = GetMD5(f1);
            string md52 = GetMD5(f2);
            bool n = (md51 == md52);*/
            bool n = CompareBinary(f1, f2);

            f1.Close();
            f2.Close();
            #endregion

            return n;
        }

        public static bool CompareBinary(FileStream f1, FileStream f2)
        {
            f1.Position = 0;
            f2.Position = 0;

            for (int i = 0; i < f1.Length; i++)
            {
                if (f1.ReadByte() != f2.ReadByte())
                    return false;
            }

            return true;
        }

        public static string GetMD5(FileStream FileCheck)
        {
            FileCheck.Position = 0;

            // MD5-Hash aus dem Byte-Array berechnen
            System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] md5Hash = md5.ComputeHash(FileCheck);
            FileCheck.Close();

            //in string wandeln
            return BitConverter.ToString(md5Hash).Replace("-", "").ToLower();
        }

        public static string RDANormalize(string file)
        {
            return file.ToLower().Replace("/", "\\").Trim('\\');
        }
    }
}
