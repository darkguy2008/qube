using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Qube.ConsoleApp
{
    // http://stackoverflow.com/a/21425134
    public abstract class ConsoleApplication
    {

        public static string AppPath { get { return new FileInfo(Assembly.GetEntryAssembly().Location).DirectoryName + "\\"; } }

        public virtual void Run(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            throw (Exception)e.ExceptionObject;
        }

    }
}
