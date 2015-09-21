using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Qube.Extensions
{
    public static class DateTimeExtensions
    {

        public static DateTime MinSQLValue(this DateTime d)
        {
            return new DateTime(1753, 1, 1);
        }

        public static DateTime MaxSQLValue(this DateTime d)
        {
            return new DateTime(9999, 12, 31);
        }

    }
}
