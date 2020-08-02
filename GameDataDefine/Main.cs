
using System.IO;
using System.Security;
using System.Text;
using System.Xml.Linq;

namespace Nullspace
{
    public class MainEntry
    {
        public static void Main(string[] argvs)
        {
            string filePath = "test_data.xml";
            Properties prop = DataFormatConvert.ConvertXMLToPropertiesFromFile(filePath);
            StringBuilder sb = new StringBuilder();
            prop.PrintAll(sb);
            File.WriteAllText("xml_2_properties.txt", sb.ToString());

            SecurityElement root = Properties.ConvertPropertiesToXML(prop);
            // 格式化
            XElement element = XElement.Parse(root.ToString());
            File.WriteAllText("properties_2_xml.xml", element.ToString());

        }
    }
}
