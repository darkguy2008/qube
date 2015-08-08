using System;
using System.Threading;

namespace Qube.Extensions
{
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
