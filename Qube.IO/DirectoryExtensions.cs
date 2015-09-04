using System.IO;

namespace Qube.IO
{
    public static class DirectoryExtensions
    {
        public static void Clean(this DirectoryInfo dir)
        {
            foreach (FileInfo fi in dir.GetFiles())
            {
                fi.IsReadOnly = false;
                fi.Delete();
            }
            foreach (DirectoryInfo di in dir.GetDirectories())
            {
                di.Clean();
                di.Delete();
            }
        }
    }
}
