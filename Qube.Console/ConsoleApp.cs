using Qube.Diagnostics;
using System;
using System.IO;
using System.Reflection;

namespace Qube.ConsoleApp
{
    // http://stackoverflow.com/a/21425134
    public abstract class ConsoleApplication
    {

        public static QubeLogger Log = new QubeLogger();
        public static CommandLine CmdLine { get; set; }
        public static string AppPath { get { return new FileInfo(Assembly.GetEntryAssembly().Location).DirectoryName + "\\"; } }

        public virtual void Run(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            CmdLine = new CommandLine(args);
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Log["Exception"].Write(QubeLogLevel.Error, "Unhandled exception: " + ExceptionHelper.GetFullException((Exception)e.ExceptionObject));
            throw (Exception)e.ExceptionObject;
        }

    }
}
