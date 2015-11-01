using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Web;
using System.Web.Configuration;

namespace Qube.Globalization
{
    public class GlobalizedStrings
    {
        private Dictionary<string, string> _dict = new Dictionary<string, string>();
        public Dictionary<string, string>.KeyCollection Keys { get { return _dict.Keys; } }

        public GlobalizedStrings()
        {
            Init(HttpContext.Current.Server.MapPath(WebConfigurationManager.AppSettings["QubeRoot"] + "/res/lang/Qube." + CultureInfo.CurrentUICulture.TwoLetterISOLanguageName + ".txt"));
        }

        public GlobalizedStrings(string filename)
        {
            Init(filename);
        }

        public void Init(string filename)
        {
            bool add = false;
            string dictKey = string.Empty;
            string finalString = string.Empty;
            foreach (string l in File.ReadAllLines(filename))
            {
                string line = l.Trim();
                if (line.StartsWith("#") && string.IsNullOrEmpty(dictKey))
                    continue;

                if (line.Contains("=") && string.IsNullOrEmpty(dictKey))
                {
                    dictKey = line.Substring(0, line.IndexOf("="));
                    line = line.Substring(line.IndexOf("=") + 1);
                }

                if (!line.EndsWith("\\") && !string.IsNullOrEmpty(dictKey))
                {
                    add = true;
                    if (!string.IsNullOrEmpty(finalString))
                    {
                        finalString += line.TrimEnd('\\');
                        finalString += Environment.NewLine;
                    }
                    else
                        finalString = line;
                }

                if (line.EndsWith("\\") && !string.IsNullOrEmpty(dictKey))
                {
                    finalString += line.TrimEnd('\\');
                    finalString += Environment.NewLine;
                }

                if (string.IsNullOrEmpty(line.Trim()) && !string.IsNullOrEmpty(dictKey))
                {
                    add = true;
                }

                if (add)
                {
                    dictKey = dictKey.Trim();
                    _dict[dictKey] = finalString.Trim();
                    add = false;
                    dictKey = string.Empty;
                    finalString = string.Empty;
                }
            }
        }

        public string this[string key]
        {
            get
            {
                return _dict[key];
            }
            set
            {
                _dict[key] = value;
            }
        }
    }
}
