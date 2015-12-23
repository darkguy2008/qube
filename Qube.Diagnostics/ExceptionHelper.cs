using System;
using System.Text;

namespace Qube.Diagnostics
{
    public static class ExceptionHelper
    {
        public static String GetFullException(Exception e)
        {
            String rv;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(GetException(0, e));
            Exception ex = e;
            int pad = 1;
            while (ex != null)
            {
                sb.AppendLine(GetException(pad, ex));
                ex = ex.InnerException;
                pad++;
            }
            rv = sb.ToString();
            return rv;
        }

        public static String GetException(int padding, Exception e)
        {
            String rv;
            StringBuilder sb = new StringBuilder();

            String pad = String.Empty;
            for (int i = 0; i < padding; i++)
                pad += ' ';

            sb.AppendLine(pad + "Exception: '" + e.Message + "'");
            sb.AppendLine(pad + "  Source:      '" + e.Source + "'");
            sb.AppendLine(pad + "  Target Site: '" + e.TargetSite + "'");
            sb.AppendLine(pad + "  Stack Trace: '" + e.StackTrace + "'");
            rv = sb.ToString();
            sb = null;
            return rv;
        }
    }
}
