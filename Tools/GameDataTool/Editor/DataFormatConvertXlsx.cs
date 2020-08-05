
using System.IO;
using System.Security;
using System.Text;
using System.Xml.Linq;

namespace Nullspace
{
    public partial class DataFormatConvert
    {

        public static Properties Config;
        public static void ExportXlsx(string config)
        {
            Config = Properties.Create(config);
            ExportXlsxDir(Config.GetString("xlsx_dir"), Config.GetBool("recursive", false));
        }

        public static Properties ConvertXlsxToProperties(Xlsx excel)
        {
            return Properties.CreateFromXlsx(excel);
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
            if (!xlsx.CheckData())
            {
                throw new System.Exception("data format wrong");
            }
            if (Config.GetBool("export_cs", false))
            {
                StringBuilder builder = new StringBuilder();
                xlsx.ExportCSharp(builder);
                File.WriteAllText(string.Format("{0}/{1}.cs", MakeDir(Config.GetString("cs_dir")), xlsx.FileName), builder.ToString());
            }
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
