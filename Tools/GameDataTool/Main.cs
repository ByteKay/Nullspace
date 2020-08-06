
using System;
using System.IO;
using System.Security;
using System.Text;
using System.Xml.Linq;

namespace Nullspace
{
    public class MainEntry
    {
        public static Properties Config;
        public static void Main(string[] argvs)
        {
            Config = Properties.Create("config.txt");
            DataFormatConvert.ExportXlsx();
            DataFormatConvert.BuildDll();
            Console.ReadLine();
        }
        private static void TestXmlAndProperties()
        {
            string filePath = "test_data.xml";
            Properties prop = DataFormatConvert.ConvertXMLToPropertiesFromFile(filePath);
            StringBuilder sb = new StringBuilder();
            prop.PrintAll(sb, 0);
            File.WriteAllText("xml_2_properties.txt", sb.ToString());

            SecurityElement root = Properties.ConvertPropertiesToXML(prop);
            // 格式化
            XElement element = XElement.Parse(root.ToString());
            File.WriteAllText("properties_2_xml.xml", element.ToString());
        }
        private static void TestXlsx()
        {
            string filePath = "test.xlsx";
            Xlsx xlsx = Xlsx.Create(filePath);
            StringBuilder sb = new StringBuilder();
            xlsx.ExportCSharp(sb);
            File.WriteAllText(string.Format("GameData/{0}.cs", xlsx.FileName), sb.ToString());

            Properties prop = Properties.CreateFromXlsx(xlsx);
            SecurityElement root = Properties.ConvertPropertiesToXML(prop);
            // 格式化
            XElement element = XElement.Parse(root.ToString());
            File.WriteAllText("xlsx_2_properties_2_xml.xml", element.ToString());
        }
    }
}
