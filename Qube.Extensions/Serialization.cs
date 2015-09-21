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
        public static String ToJSON(this object obj)
        {
            return new JavaScriptSerializer().Serialize(obj);
        }

        public static String ToJSON(this DataTable dt)
        {
            List<Dictionary<String, String>> rv = new List<Dictionary<String, String>>();

            foreach (DataRow dr in dt.Rows)
            {
                Dictionary<String, String> row = new Dictionary<String, String>();
                foreach (DataColumn c in dt.Columns)
                    row[c.ColumnName] = dr[c].ToString();
                rv.Add(row);
            }

            return rv.ToJSON();
        }

        public static String ToJSON(this DataRow dr)
        {
            List<Dictionary<String, String>> rv = new List<Dictionary<String, String>>();

            Dictionary<String, String> row = new Dictionary<String, String>();
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
