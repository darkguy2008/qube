using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Qube.Console
{
    public class CommandLine
    {
        private Dictionary<String, String> _cmd = new Dictionary<String, String>();

        public CommandLine(string[] args)
        {
            String lastKey = String.Empty;
            for (int i = 0; i < args.Length; i++)
            {
                String arg = args[i];
                if (arg.StartsWith("\"")) { arg = arg.Substring(1); }
                if (arg.EndsWith("\"")) { arg = arg.Substring(0, arg.Length - 1); }
                if (arg.StartsWith("-"))
                {
                    lastKey = arg.Substring(1);
                    _cmd.Add(lastKey, null);
                }
                else
                {
                    _cmd[lastKey] = arg;
                }
            }
        }

        public bool ContainsArg(String key)
        {
            return _cmd.ContainsKey(key);
        }
        public int Length { get { return _cmd.Count; } }

        public String this[String key]
        {
            get
            {
                return _cmd[key];
            }
            set
            {
                _cmd[key] = value;
            }
        }

        public String this[int index]
        {
            get
            {
                return _cmd.ToList()[index].Value;
            }
        }
    }
}
