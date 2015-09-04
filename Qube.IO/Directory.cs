using System;
using System.IO;

namespace Qube.IO
{
    public static class DirectoryUtils
    {
        public static Exception LastError { get; private set; }
        public static void ClearLastError()
        {
            LastError = null;
        }

        // http://www.codeproject.com/Tips/279969/Recursively-Copy-folder-contents-to-another-in-C
        public static bool CopyContents(string sourcePath, string destinationPath)
        {
            string slashEndingSourcePath = sourcePath.EndsWith(@"\") ? sourcePath : sourcePath + @"\";
            string slashEndingDestinationPath = destinationPath.EndsWith(@"\") ? destinationPath : destinationPath + @"\";
            try
            {
                if (Directory.Exists(slashEndingSourcePath))
                {
                    if (!Directory.Exists(slashEndingDestinationPath))
                        Directory.CreateDirectory(slashEndingDestinationPath);

                    foreach (string files in Directory.GetFiles(slashEndingSourcePath))
                        new FileInfo(files).CopyTo(slashEndingDestinationPath + new FileInfo(files).Name, true);

                    foreach (string drs in Directory.GetDirectories(slashEndingSourcePath))
                        if (!CopyContents(drs, slashEndingDestinationPath + new DirectoryInfo(drs).Name))
                            return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                LastError = ex;
                return false;
            }
        }
    }
}
