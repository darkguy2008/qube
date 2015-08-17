﻿using System;
using System.Collections.Generic;
using System.IO;

namespace Qube.Globalization
{
    public class GlobalizedStrings
    {
        private Dictionary<String, String> _dict = new Dictionary<String, String>();

        public GlobalizedStrings(String filename)
        {
            bool add = false;
            String dictKey = String.Empty;
            String finalString = String.Empty;
            foreach(String l in File.ReadAllLines(filename))
            {
                String line = l.Trim();
                if(line.Contains("=") && String.IsNullOrEmpty(dictKey))
                {
                    dictKey = line.Substring(0, line.IndexOf("="));
                    line = line.Substring(line.IndexOf("=") + 1);
                }

                if (!line.EndsWith("\\") && !String.IsNullOrEmpty(dictKey))
                {
                    add = true;
                    finalString = line;
                }

                if(line.EndsWith("\\") && !String.IsNullOrEmpty(dictKey))
                {
                    finalString += line;
                    finalString += Environment.NewLine;
                }

                if(String.IsNullOrEmpty(line.Trim()) && !String.IsNullOrEmpty(dictKey))
                {
                    add = true;                    
                }

                if(add)
                {
                    dictKey = dictKey.Trim();
                    _dict[dictKey] = finalString;
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
