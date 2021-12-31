using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace TML.Patcher.CLI.Platform.Windows
{
    internal static class Explorer
    {
        [DllImport("shell32.dll", SetLastError = true)]
        private static extern int SHOpenFolderAndSelectItems(
            IntPtr pidlFolder,
            uint cidl,
            [In, MarshalAs(UnmanagedType.LPArray)] IntPtr[] apidl,
            uint dwFlags
        );

        [DllImport("shell32.dll", SetLastError = true)]
        private static extern void SHParseDisplayName(
            [MarshalAs(UnmanagedType.LPWStr)] string name,
            IntPtr bindingContext,
            [Out] out IntPtr pidl,
            uint sfgaoIn,
            [Out] out uint psfgaoOut
        );

        internal static void OpenDirectory(string filename)
        {
            Task.Run(() =>
            {
                IntPtr nativeFile = IntPtr.Zero;
                IntPtr nativeFolder = IntPtr.Zero;

                try
                {
                    filename = filename.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

                    string folderPath = Path.GetDirectoryName(filename) ?? "/";

                    SHParseDisplayName(folderPath, IntPtr.Zero, out nativeFolder, 0, out _);

                    if (nativeFolder == IntPtr.Zero)
                        return;

                    SHParseDisplayName(filename, IntPtr.Zero, out nativeFile, 0, out _);

                    IntPtr[] files = nativeFile != IntPtr.Zero ? new[] {nativeFile} : new[] {nativeFolder};

                    SHOpenFolderAndSelectItems(nativeFolder, (uint)files.Length, files, 0);
                }
                finally
                {
                    if (nativeFolder != IntPtr.Zero)
                        Marshal.FreeCoTaskMem(nativeFolder);

                    if (nativeFile != IntPtr.Zero)
                        Marshal.FreeCoTaskMem(nativeFile);
                }
            });
        }
    }
}