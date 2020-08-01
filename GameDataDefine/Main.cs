
using System.IO;
using System.Text;

namespace Nullspace
{
    public class MainEntry
    {
        public static void Main(string[] argvs)
        {
            string filePath = "test_data.xml";
            Properties prop = XMLConvertToProperties.ConvertXMLFromFile(filePath);
            StringBuilder sb = new StringBuilder();
            prop.PrintAll(sb);
            File.WriteAllText("xml_2_properties.txt", sb.ToString());
        }
    }
}
