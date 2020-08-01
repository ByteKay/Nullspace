
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security;

namespace Nullspace
{
    public static class XMLConvertToProperties
    {
        public static Properties ConvertXMLFromFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return null;
            }
            string content = File.ReadAllText(filePath);
            return ConvertXMLFromContent(content, filePath);
        }

        public static Properties ConvertXMLFromContent(string content, string filePath = null)
        {
            if (string.IsNullOrEmpty(content))
            {
                Debug.Assert(false, "Attempting to create a Properties object from an empty URL!");
                return null;
            }
            SecurityParser securityParser = new SecurityParser();
            securityParser.LoadXml(content);
            SecurityElement xml = securityParser.ToXml();
            return Properties.CreateFromXml(xml);
        }
    }
}
