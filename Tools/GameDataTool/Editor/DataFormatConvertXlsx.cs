
using System.Diagnostics;
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
            ExportEmpty();
            ExportXlsxDir(Config.GetString("xlsx_dir"), Config.GetBool("recursive", false));
        }

        public static void BuildDll()
        {
            if (Config.GetBool("export_cs", false))
            {
                string csDir = Config.GetString("cs_dir");
                string batFilePath = string.Format("{0}/compile_runtime.bat", csDir);
                StringBuilder sb = new StringBuilder();
                sb.Append("set UnityEngine=../UnityEngine.CoreModule.dll").AppendLine();
                sb.Append("set GameDataRuntime=../GameDataRuntime.dll").AppendLine();
                sb.Append("call \"C:/Windows/Microsoft.NET/Framework/v3.5/csc.exe\" -target:library /r:%UnityEngine% /r:%GameDataRuntime% /out:GameDataDefine.dll *.cs /recurse:*.cs").AppendLine();
                File.WriteAllText(batFilePath, sb.ToString());
                RunCmd(string.Format("cd /d {0} &exit", csDir) , "\"cmd.exe\" /c \"compile_runtime.bat\" &exit");
            }
        }

        public static void CheckData()
        {

        }

        /// <summary>
        /// 运行cmd命令
        /// 不显示命令窗口
        /// </summary>
        /// <param name="cmdExe">指定应用程序的完整路径</param>
        /// <param name="cmdStr">执行命令行参数</param>
        private static bool RunCmd(string cdDir, string cmdStr)
        {
            bool result = false;
            try
            {
                using (Process myPro = new Process())
                {
                    myPro.StartInfo.FileName = "cmd.exe";
                    myPro.StartInfo.UseShellExecute = false;
                    myPro.StartInfo.RedirectStandardInput = true;
                    myPro.StartInfo.RedirectStandardOutput = true;
                    myPro.StartInfo.RedirectStandardError = true;
                    myPro.StartInfo.CreateNoWindow = true;
                    myPro.Start();
                    myPro.StandardInput.AutoFlush = true;
                    myPro.StandardInput.WriteLine(cdDir);
                    myPro.WaitForExit();
                    myPro.StandardInput.WriteLine(cmdStr);
                    myPro.WaitForExit();
                    result = true;
                }
            }
            catch
            {

            }
            return result;
        }

        private static Properties ConvertXlsxToProperties(Xlsx excel)
        {
            return Properties.CreateFromXlsx(excel);
        }

        private static void ExportEmpty()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("namespace GameData");
            builder.AppendLine("{");
            string tab = "    ";
            builder.Append(tab).Append("public class EmptyGameData {}").AppendLine();
            builder.AppendLine("}");
            File.WriteAllText(string.Format("{0}/EmptyGameData.cs", MakeDir(Config.GetString("cs_dir"))), builder.ToString());
        }

        private static void ExportXlsxDir(string rootDir, bool recursive = true)
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

        private static void ExportXlsxFile(string filePath)
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
    }
}
