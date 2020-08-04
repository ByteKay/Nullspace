
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security;
using System.Text;
using System.Xml.Linq;

namespace Nullspace
{
    public static class DataFormatConvert
    {
        public static Properties ConvertXMLToPropertiesFromFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return null;
            }
            string content = File.ReadAllText(filePath);
            return ConvertXMLToPropertiesFromContent(content, filePath);
        }

        public static Properties ConvertXMLToPropertiesFromContent(string content, string filePath = null)
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

        public static SecurityElement ConvertPropertiesToXMLFromContent(string content, string filePath = null)
        {
            Properties prop = Properties.CreateFromContent(content, filePath);
            return Properties.ConvertPropertiesToXML(prop);
        }

        public static SecurityElement ConvertPropertiesToXMLFromFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return null;
            }
            Properties prop = Properties.Create(filePath);
            return Properties.ConvertPropertiesToXML(prop);
        }

        public static Properties ConvertXlsxToProperties(Xlsx excel)
        {
            return Properties.CreateFromXlsx(excel);
        }

        public static Properties Config;
        public static void ExportXlsx(string config)
        {
            Config = Properties.Create(config);
            ExportXlsxDir(Config.GetString("xlsx_dir"), Config.GetBool("recursive", false));
        }

        public static void ExportXlsxDir(string rootDir, bool recursive = true)
        {
            string[] files = Directory.GetFiles(rootDir, "*.xlsx");
            foreach (string file in files)
            {
                ExportXlsxFile(file);
            }
            if (recursive)
            {
                string[] dirs = Directory.GetDirectories(rootDir);
                foreach (string dir in dirs)
                {
                    ExportXlsxDir(dir, recursive);
                }
            }
        }

        public static void ExportXlsxFile(string filePath)
        {
            Xlsx xlsx = Xlsx.Create(filePath);
            StringBuilder builder = new StringBuilder();
            xlsx.ExportCSharp(builder);
            File.WriteAllText(string.Format("{0}/{1}.cs", MakeDir(Config.GetString("cs_dir")), xlsx.FileName), builder.ToString());

            Properties prop = Properties.CreateFromXlsx(xlsx);
            SecurityElement clientRoot;
            SecurityElement serverRoot;
            Properties.ConvertXlsxPropertiesToXML(prop, out clientRoot, out serverRoot);
            XElement element = XElement.Parse(clientRoot.ToString());
            File.WriteAllText(string.Format("{0}/{1}_client.xml", MakeDir(Config.GetString("xml_dir")), xlsx.FileName), element.ToString());
            element = XElement.Parse(serverRoot.ToString());
            File.WriteAllText(string.Format("{0}/{1}_server.xml", MakeDir(Config.GetString("xml_dir")), xlsx.FileName), element.ToString());
        }

        private static string MakeDir(string dir)
        {
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir.Replace("\\", "/"));
            }
            return dir;
        }

        private static bool mIsLoadDll = false;
        public static void BuildDll()
        {
            mIsLoadDll = false;
        }

        public static void LoadDll()
        {
            if (mIsLoadDll)
            {
                return;
            }
            mIsLoadDll = true;
        }
    }
}
