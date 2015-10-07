using System;
using System.Reflection;
using System.Threading;
using System.Linq;

namespace Qube.Extensions
{
    public static class Object
    {
        public static void CopyObject(this object src, object dst, string[] propsToCopy = null)
        {
            PropertyInfo[] piSrc = src.GetType().GetProperties();
            PropertyInfo[] piDst = dst.GetType().GetProperties();
            foreach (PropertyInfo pd in piDst)
                foreach (PropertyInfo ps in piSrc)
                    if (ps.Name == pd.Name)
                        if(propsToCopy == null || (propsToCopy != null && propsToCopy.Contains(pd.Name)))
                            if (pd.CanWrite)
                                pd.SetValue(dst, ps.GetValue(src, null), null);
        }
    }

    public static class Actions
    {
        public static void Retry(Action action, TimeSpan sleepPeriod, int retryCount = 3)
        {
            while (true)
            {
                try
                {
                    action();
                    break;
                }
                catch
                {
                    if (--retryCount == 0) throw;
                    else Thread.Sleep(sleepPeriod);
                }
            }
        }
    }

}
