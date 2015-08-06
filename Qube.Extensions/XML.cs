using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace Qube.Extensions
{
    public static class Xml
    {
        public static T Deserialize<T>(this string toDeserialize)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
            StringReader textReader = new StringReader(toDeserialize);
            return (T)xmlSerializer.Deserialize(textReader);
        }

        public static string Serialize<T>(this T toSerialize)
        {
            XmlSerializer xs = new XmlSerializer(typeof(T));
            StringWriter tw = new StringWriter();
            XmlSerializerNamespaces emptyNs = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });
            XmlWriter xw = XmlWriter.Create(tw, new XmlWriterSettings()
            {
                OmitXmlDeclaration = true,
                Indent = true
            });
            xs.Serialize(xw, toSerialize, emptyNs);
            xw.Close();
            return tw.ToString();
        }
    }
}
