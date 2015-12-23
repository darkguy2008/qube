using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Qube.Diagnostics
{
    public enum QubeLogLevel
    {
        Debug = 0,
        Error = 1,
        Warning = 2,
        Information = 3,
        Verbose = 4
    }

    public static class QubeLogShared
    {
        public static String Buf;
        public static String fName;
        public static readonly object ObjMutex = new Object();
    }
    public class QubeLogger : IDisposable
    {
        public DateTime LogDate { get; set; }
        public bool EnableConsole { get; set; }
        private Dictionary<Object, QubeLog> Logs = new Dictionary<Object, QubeLog>();

        private String _PathLog;
        public String PathLog
        {
            get { return _PathLog; }
            set
            {
                _PathLog = value;
                foreach (KeyValuePair<Object, QubeLog> kv in Logs)
                    kv.Value.Refresh();
            }
        }

        private int _MaxDaysToKeep;
        public int MaxDaysToKeep
        {
            get { return _MaxDaysToKeep; }
            set
            {
                _MaxDaysToKeep = value;
                foreach (KeyValuePair<Object, QubeLog> kv in Logs)
                    kv.Value.Refresh();
            }
        }
        
        public QubeLogger()
        {
            LogDate = DateTime.MinValue;
            PathLog = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            MaxDaysToKeep = 365;
            EnableConsole = true;
        }

        public QubeLog GetLog(Object key)
        {
            String sKey = key.ToString();
            if (!Logs.ContainsKey(sKey))
            {
                Logs[sKey] = new QubeLog()
                {
                    Parent = this,
                    Date = LogDate,
                    Tag = sKey.ToString(),
                };
                Logs[sKey].Refresh();
            }

            return Logs[sKey];
        }

        public QubeLog AddLog(Object key, String filename)
        {
            String sKey = key.ToString();
            if (!Logs.ContainsKey(sKey))
            {
                Logs[sKey] = new QubeLog()
                {
                    Parent = this,
                    Date = LogDate,
                    Tag = sKey.ToString(),
                };
                Logs[sKey].SetFilename(filename);
            }

            return Logs[sKey];
        }

        public QubeLog this[Object key]
        {
            get
            {
                return GetLog(key);
            }
        }

        public void Dispose()
        {
            foreach (KeyValuePair<Object, QubeLog> kv in Logs)
                kv.Value.Close();

            while (true)
            {
                bool bFinished = true;
                foreach (KeyValuePair<Object, QubeLog> kv in Logs)
                    if (bFinished && !kv.Value.IsFinished)
                        bFinished = false;
                if (bFinished)
                    break;
            }

            Logs.Clear();
            Logs = null;
        }
    }

    public class QubeLog
    {
        public bool HasCustomFilename { get; set; }
        public QubeLogger Parent { get; set; }

        public List<Object> LogQueue = new List<Object>();
        public String Tag { get; set; }
        private String Prefix { get; set; }

        public bool IsDisposing { get; set; }
        public bool IsFinished { get; set; }
        public StreamWriter sw { get; set; }
        public DateTime Date { get; set; }

        public QubeLog()
        {
            HasCustomFilename = false;
            IsFinished = false;
        }

        public void Close()
        {
            IsFinished = true;
        }

        public void Refresh()
        {
            if (sw != null)
                sw.Close();

            if (DateTime.Now.Day != Date.Day || DateTime.Now.Month != Date.Month)
            {
                if (!Directory.Exists(Parent.PathLog))
                    Directory.CreateDirectory(Parent.PathLog);

                Date = DateTime.Now;
                Prefix = Date.ToString("yyyy-MM-dd_HH.mm.ss_");

                String[] sFiles = Directory.GetFiles(Parent.PathLog, "*" + Tag + ".log");
                foreach (String f in sFiles)
                    if ((Date - (new FileInfo(f)).CreationTime).TotalDays > Parent.MaxDaysToKeep)
                        File.Delete(f);
            }

            sw = new StreamWriter(Parent.PathLog + "\\" + Prefix + "." + Tag + ".log", true)
            {
                AutoFlush = true
            };
        }

        public void SetFilename(String fName)
        {
            HasCustomFilename = true;

            if (sw != null)
                sw.Close();

            sw = new StreamWriter(fName, true)
            {
                AutoFlush = true
            };
        }

        public void Write(Object s)
        {
            Write(QubeLogLevel.Debug, s);
        }

        public void Write(QubeLogLevel lvl, Object s)
        {
            Write(lvl, null, s);
        }

        public void Write(QubeLogLevel lvl, ConsoleColor? col, Object s)
        {
            lock (QubeLogShared.ObjMutex)
            {
                if(!HasCustomFilename)
                    Refresh();

                QubeLogShared.Buf = String.Format("{0} [{1}] [{2}] > {3}",
                                                DateTime.Now.ToString("MM-dd HH:mm:ss"),
                                                lvl.ToString(),
                                                Tag,
                                                s
                                            );

                if (Parent.EnableConsole)
                {
                    if (col == null)
                    {
                        Console.ResetColor();
                        switch (lvl)
                        {
                            default:
                                break;
                            case QubeLogLevel.Error:
                                Console.ForegroundColor = ConsoleColor.Red;
                                break;
                            case QubeLogLevel.Information:
                                Console.ForegroundColor = ConsoleColor.Magenta;
                                break;
                            case QubeLogLevel.Verbose:
                                Console.ForegroundColor = ConsoleColor.Green;
                                break;
                            case QubeLogLevel.Warning:
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                break;
                        }
                    }
                    else
                        Console.ForegroundColor = col.Value;

                    Console.WriteLine(QubeLogShared.Buf);
                }

                if (sw != null)
                    sw.WriteLine(QubeLogShared.Buf);
            }
        }
    }
}
