using System;
using System.Collections.Generic;
using System.IO;

namespace Qube.IO
{
    public static class INIFile
    {
        public static Dictionary<String, Dictionary<String, String>> Read(String iniFile)
        {
            Dictionary<String, Dictionary<String, String>> rv = new Dictionary<String, Dictionary<String, String>>();

            String lastKey = String.Empty;
            foreach (String l in File.ReadAllLines(iniFile))
            {
                if (l.StartsWith(";") || l.StartsWith("#"))
                    continue;
                if (l.StartsWith("["))
                {
                    lastKey = l.Substring(l.IndexOf('[') + 1, l.LastIndexOf(']') - 1);
                    rv[lastKey] = new Dictionary<String, String>();
                }
                else
                    if (l.Contains("="))
                    rv[lastKey][l.Substring(0, l.IndexOf('='))] = l.Substring(l.IndexOf('=') + 1);
            }

            return rv;
        }

        public static void Save(this Dictionary<String, Dictionary<String, String>> ini, String filename)
        {
            using (StreamWriter sw = new StreamWriter(filename, false))
                foreach (String key in ini.Keys)
                {
                    sw.WriteLine("[" + key + "]");
                    foreach (KeyValuePair<String, String> kvp in ini[key])
                        sw.WriteLine(kvp.Key + "=" + kvp.Value);
                    sw.WriteLine();
                }
        }
    }
}
