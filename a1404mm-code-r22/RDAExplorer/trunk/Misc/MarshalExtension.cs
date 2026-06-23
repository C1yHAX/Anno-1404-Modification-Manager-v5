using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;

namespace RDAExplorer.Misc
{
    public class MarshalExtension
    {
        public static object ReadToStructure(BinaryReader reader, Type outputtype)
        {
            unsafe
            {
                object output;

                object testInstance = Activator.CreateInstance(outputtype);
                int sz = Marshal.SizeOf(testInstance);

                IntPtr buffer = Marshal.AllocCoTaskMem(sz);
                Marshal.Copy(reader.ReadBytes(sz), 0, buffer, sz);

                output = Marshal.PtrToStructure(buffer, outputtype);

                Marshal.FreeCoTaskMem(buffer);

                return output;
            }
        }

        public static object ReadToStructure(BinaryReader reader, Type outputtype, int sz)
        {
            unsafe
            {
                object output;

                IntPtr buffer = Marshal.AllocCoTaskMem(sz);
                Marshal.Copy(reader.ReadBytes(sz), 0, buffer, sz);

                output = Marshal.PtrToStructure(buffer, outputtype);

                Marshal.FreeCoTaskMem(buffer);

                return output;
            }
        }

        public static byte[] WriteToByte(object obj)
        {
            int rawsize = Marshal.SizeOf(obj);
            IntPtr buffer = Marshal.AllocHGlobal(rawsize);
            Marshal.StructureToPtr(obj, buffer, false);
            byte[] rawdatas = new byte[rawsize];
            Marshal.Copy(buffer, rawdatas, 0, rawsize);
            Marshal.FreeHGlobal(buffer);

            return rawdatas;
        }
    }
}
