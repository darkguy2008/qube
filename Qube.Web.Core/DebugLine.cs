﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;

namespace Qube.Web.Core
{
    public static class DebugLine
    {
        public static int line = 0;

        public static void Write(object obj)
        {
            HttpContext.Current.Response.Write("<div style='position: relative; top: 0; left: 0; border: 2px solid red; background: #d00;'>" + obj.ToString() + "</div>");
            line++;
        }

    }
}
