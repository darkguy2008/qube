using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Web;

namespace Qube.Globalization
{
    public class GlobalizedStrings
    {
        private Dictionary<String, String> _dict = new Dictionary<String, String>();
        public Dictionary<String, String>.KeyCollection Keys { get { return _dict.Keys; } }

        public GlobalizedStrings()
        {
            Init(HttpContext.Current.Server.MapPath("~/App_GlobalResources/Qube." + CultureInfo.CurrentUICulture.TwoLetterISOLanguageName + ".txt"));
        }

        public GlobalizedStrings(String filename)
        {
            Init(filename);
        }

        public void Init(String filename)
        {
            bool add = false;
            String dictKey = String.Empty;
            String finalString = String.Empty;
            foreach (String l in File.ReadAllLines(filename))
            {
                String line = l.Trim();
                if (line.StartsWith("#") && String.IsNullOrEmpty(dictKey))
                    continue;

                if (line.Contains("=") && String.IsNullOrEmpty(dictKey))
                {
                    dictKey = line.Substring(0, line.IndexOf("="));
                    line = line.Substring(line.IndexOf("=") + 1);
                }

                if (!line.EndsWith("\\") && !String.IsNullOrEmpty(dictKey))
                {
                    add = true;
                    if (!String.IsNullOrEmpty(finalString))
                    {
                        finalString += line.TrimEnd('\\');
                        finalString += Environment.NewLine;
                    }
                    else
                        finalString = line;
                }

                if (line.EndsWith("\\") && !String.IsNullOrEmpty(dictKey))
                {
                    finalString += line.TrimEnd('\\');
                    finalString += Environment.NewLine;
                }

                if (String.IsNullOrEmpty(line.Trim()) && !String.IsNullOrEmpty(dictKey))
                {
                    add = true;
                }

                if (add)
                {
                    dictKey = dictKey.Trim();
                    _dict[dictKey] = finalString.Trim();
                    add = false;
                    dictKey = String.Empty;
                    finalString = String.Empty;
                }
            }
        }

        public String this[String key]
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
