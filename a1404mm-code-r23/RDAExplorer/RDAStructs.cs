using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace RDAExplorer
{
    [StructLayout(LayoutKind.Sequential)]
    public struct FileHeader
    {
        [MarshalAs(UnmanagedType.ByValArray,
            SizeConst = 18,
            ArraySubType = UnmanagedType.I2)]
        public char[] magic;       

        [MarshalAs(UnmanagedType.ByValArray,
            SizeConst = 484, //1007
            ArraySubType = UnmanagedType.U1)]
        public byte[] uk1;

        //Unknown
        [MarshalAs(UnmanagedType.I4,
           SizeConst = 4)]
        public int u1;
        [MarshalAs(UnmanagedType.I4,
           SizeConst = 4)]
        public int u2;
        [MarshalAs(UnmanagedType.I4,
           SizeConst = 4)]
        public int u3;
        [MarshalAs(UnmanagedType.I4,
           SizeConst = 4)]
        public int u4;
        [MarshalAs(UnmanagedType.I4,
          SizeConst = 4)]
        public int u5;
        [MarshalAs(UnmanagedType.I4,
          SizeConst = 4)]
        public int u6;

        [MarshalAs(UnmanagedType.ByValArray,
           SizeConst = 499,
           ArraySubType = UnmanagedType.U1)]
        public byte[] uk2;

        [MarshalAs(UnmanagedType.U4,
            SizeConst = 4)]
        public int firstBlock;

        public static FileHeader Create()
        {
            FileHeader hd = new FileHeader();

            hd.magic = "Resource File V2.0".ToCharArray();
            hd.uk1 = new byte[484];
            hd.uk2 = new byte[499];
            hd.firstBlock = 0;

            return hd;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct BlockInfo
    {
        [MarshalAs(UnmanagedType.U4,
            SizeConst = 4)]
        public int flags;

        [MarshalAs(UnmanagedType.U4,
            SizeConst = 4)]
        public int fileCount;

        [MarshalAs(UnmanagedType.U4,
            SizeConst = 4)]
        public int directorySize;

        [MarshalAs(UnmanagedType.U4,
            SizeConst = 4)]
        public int decompressedSize;

        [MarshalAs(UnmanagedType.U4,
            SizeConst = 4)]
        public int nextBlock; //If last, = filesize
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DirEntry
    {
        [MarshalAs(UnmanagedType.ByValArray,
           SizeConst = 260,
           ArraySubType = UnmanagedType.I2)]
        public char[] filename;

        [MarshalAs(UnmanagedType.U4,
           SizeConst = 4)]
        public int offset;

        [MarshalAs(UnmanagedType.U4,
           SizeConst = 4)]
        public int compressed;

        [MarshalAs(UnmanagedType.U4,
           SizeConst = 4)]
        public int filesize;

        [MarshalAs(UnmanagedType.I4,
           SizeConst = 4)]
        public int timestamp;

        [MarshalAs(UnmanagedType.U4,
           SizeConst = 4)]
        public int uk;

        public string GetFileName()
        {
            return (new String(filename)).Replace("\0", "");
        }
    }
}
