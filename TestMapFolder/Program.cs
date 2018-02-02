using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace TestMapFolder
{
    class Program
    {
        [DllImport("kernel32.dll")]
        static extern bool CreateSymbolicLink(
        string lpSymlinkFileName, string lpTargetFileName, int dwFlags);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        static extern int GetFinalPathNameByHandle(
        SafeFileHandle hFile, [MarshalAs(UnmanagedType.LPTStr)]StringBuilder lpszFilePath, int cchFilePath, int dwFlags);


        static void Main(string[] args)
        {
            string link = args[0];
            string target = args[1];
            string path = "";
            if (Directory.Exists(link))
            {
                using (SafeFileHandle hFile = WinAPI.CreateFile(
                    link,
                    WinAPI.GENERIC_READ, FileShare.Read,
                    IntPtr.Zero,
                    (FileMode)WinAPI.OPEN_EXISTING,
                    WinAPI.FILE_FLAG_BACKUP_SEMANTICS,
                    IntPtr.Zero
                    ))
                {



                    StringBuilder sBuffer = new StringBuilder(255);

                    int i = GetFinalPathNameByHandle(hFile, sBuffer, 255, 0x0);

                    Console.WriteLine("sBuffer: " + sBuffer.ToString());

                    if (sBuffer.Length > 4)
                        path = sBuffer.Remove(0, 4).ToString();
                    else
                        path = sBuffer.ToString();

                 }

                Console.WriteLine("Old folder: " + path);
 
                Directory.Delete(link);
            }

            if (!CreateSymbolicLink(link, target, 0x1))
            {

                Console.WriteLine("Mapping symbolic link " + link + " to target folder " + target + " failed");

            }
            else
                Console.WriteLine("Mapping symbolic link " + link + " to target folder " + target + " done");

        }


    }

    class WinAPI
    {
        internal const int
            GENERIC_READ = unchecked((int)0x80000000),
            FILE_FLAG_BACKUP_SEMANTICS = unchecked((int)0x02000000),
            OPEN_EXISTING = unchecked((int)3);

        [StructLayout(LayoutKind.Sequential)]
        public struct FILE_OBJECTID_BUFFER
        {
            public struct Union
            {
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
                public byte[] BirthVolumeId;

                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
                public byte[] BirthObjectId;

                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
                public byte[] DomainId;
            }

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] ObjectId;

            public Union BirthInfo;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 48)]
            public byte[] ExtendedInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct BY_HANDLE_FILE_INFORMATION
        {
            public uint FileAttributes;
            public FILETIME CreationTime;
            public FILETIME LastAccessTime;
            public FILETIME LastWriteTime;
            public uint VolumeSerialNumber;
            public uint FileSizeHigh;
            public uint FileSizeLow;
            public uint NumberOfLinks;
            public uint FileIndexHigh;
            public uint FileIndexLow;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool DeviceIoControl(
            SafeFileHandle hDevice,
            uint dwIoControlCode,
            IntPtr lpInBuffer,
            uint nInBufferSize,
            [Out] IntPtr lpOutBuffer,
            int nOutBufferSize,
            ref uint lpBytesReturned,
            IntPtr lpOverlapped
            );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern SafeFileHandle CreateFile(
            String fileName,
            int dwDesiredAccess,
            System.IO.FileShare dwShareMode,
            IntPtr securityAttrs_MustBeZero,
            System.IO.FileMode dwCreationDisposition,
            int dwFlagsAndAttributes,
            IntPtr hTemplateFile_MustBeZero
            );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool GetFileInformationByHandle(
            IntPtr hFile, out BY_HANDLE_FILE_INFORMATION lpFileInformation);
    }

}
