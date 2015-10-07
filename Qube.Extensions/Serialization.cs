using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using System.Data;
using System.IO;
using System.Text;
using System.Runtime.Serialization.Json;

namespace Qube.Extensions
{
    public static class Serialization
    {
        public static string ToJSON(this object obj)
        {
            return new JavaScriptSerializer().Serialize(obj);
        }

        public static string ToJSON(this DataTable dt)
        {
            List<Dictionary<string, string>> rv = new List<Dictionary<string, string>>();

            foreach (DataRow dr in dt.Rows)
            {
                Dictionary<string, string> row = new Dictionary<string, string>();
                foreach (DataColumn c in dt.Columns)
                    row[c.ColumnName] = dr[c].ToString();
                rv.Add(row);
            }

            return rv.ToJSON();
        }

        public static string ToJSON(this DataRow dr)
        {
            List<Dictionary<string, string>> rv = new List<Dictionary<string, string>>();

            Dictionary<string, string> row = new Dictionary<string, string>();
            foreach (DataColumn c in dr.Table.Columns)
                row[c.ColumnName] = dr[c].ToString();
            rv.Add(row);

            return rv.ToJSON();            
        }

        public static T FromJSON<T>(string json)
        {
            T obj = Activator.CreateInstance<T>();
            MemoryStream ms = new MemoryStream(Encoding.Unicode.GetBytes(json));
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(obj.GetType());
            obj = (T)serializer.ReadObject(ms);
            ms.Close();
            return obj;
        }
    }
}
