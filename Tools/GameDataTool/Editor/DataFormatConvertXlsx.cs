
using System;
using System.Collections.Generic;
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
                string refDll = Path.GetFullPath(".");
                csDir = Path.GetFullPath(csDir);
                List<string> cmdList = new List<string>()
                {
                    string.Format("cd /d {0}", csDir),
                    string.Format("set UnityEngine={0}/UnityEngine.CoreModule.dll", refDll),
                    string.Format("set GameDataRuntime={0}/GameDataRuntime.dll", refDll),
                    string.Format("call \"C:/Windows/Microsoft.NET/Framework/v3.5/csc.exe\" -target:library /r:%UnityEngine% /r:%GameDataRuntime% /out:{0}/GameDataDefine.dll *.cs /recurse:*.cs", refDll),
                    "exit"
                };
                string result = RunCmd(cmdList);
                Console.WriteLine(result);
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
        private static string RunCmd(List<string> cmdList)
        {
            string result = null;
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
                    for (int i = 0; i < cmdList.Count; ++i)
                    {
                        myPro.StandardInput.WriteLine(cmdList[i]);
                    }
                    result = myPro.StandardOutput.ReadToEnd();
                    myPro.WaitForExit();
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
