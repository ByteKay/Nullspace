using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using OfficeOpenXml;
using System.Xml.Linq;

namespace Nullspace
{       
    /// <summary>
    /// 一个文件 一个 CS
    /// 一个 sheet 一个 class
    /// </summary>
    public class XlsxConvert
    {
        private static string CSharpOutDir;
        private static string XMLOutDir;
        private static int NameRow = 1;
        private static int TypeRow = 3;
        private static int DataRow = 4;

        public static void Convert(string xlsxDir, string outCshapDir, string outXmlDir, HashSet<string> ignore)
        {
            CSharpOutDir = outCshapDir;
            if (!Directory.Exists(CSharpOutDir))
            {
                Directory.CreateDirectory(CSharpOutDir);
            }
            XMLOutDir = outXmlDir;
            if (!Directory.Exists(XMLOutDir))
            {
                Directory.CreateDirectory(XMLOutDir);
            }
            string[] files = Directory.GetFiles(xlsxDir, "*.xlsx");
            foreach (string file in files)
            {
                string fileName = Path.GetFileNameWithoutExtension(file).Trim();
                fileName = Regex.Replace(fileName, @"\d", "");
                fileName = Regex.Replace(fileName, @"[\u4e00-\u9fa5]+", "");
                if (ignore.Contains(fileName))
                {
                    continue;
                }
                GeneratorCS(file);
                ExcelToXml(file);
            }
        }

        private static void GetSheetNameAndTypes(ExcelWorksheet tDS, ref List<string> names,  ref List<string> nameTypes)
        {
            ExcelRange range = tDS.Cells;
            object[,] values = (object[,])range.Value;
            int rows = values.GetLength(0);
            int cols = values.GetLength(1);
            int nameRow = NameRow - 1;
            for (int j = 0; j < cols; ++j)
            {
                if (values[nameRow, j] == null)
                {
                    break;
                }
                string name = values[nameRow, j].ToString();
                names.Add(name.ToString());
            }
            int nameTypeRow = TypeRow - 1;
            for (int j = 0; j < names.Count; ++j)
            {
                if (values[nameTypeRow, j] == null)
                {
                    values[nameTypeRow, j] = "";
                    throw new Exception("empty " + j);
                }
                string name = values[nameTypeRow, j].ToString();
                nameTypes.Add(name.ToString());
            }
        }

        public static void GeneratorCS(string fileFullPath)
        {
            FileInfo newFile = new FileInfo(fileFullPath);
            using (ExcelPackage pck = new ExcelPackage(newFile))
            {
                try
                {
                    ExcelWorkbook workBook = pck.Workbook;
                    if (workBook != null)
                    {

                        HashSet<string> removeField = new HashSet<string>();
                        removeField.Add("id");
                        string fileName = GetFileName(fileFullPath);
                        StringBuilder builder = new StringBuilder();
                        builder.AppendLine("using System;");
                        builder.AppendLine("using System.Collections.Generic;");
                        builder.AppendLine("using System.Text;");
                        builder.AppendLine("namespace Nullspace");
                        builder.AppendLine("{");

                        int cnt = workBook.Worksheets.Count;
                        var enumerator = workBook.Worksheets.GetEnumerator();
                        while (enumerator.MoveNext())
                        {
                            ExcelWorksheet tDS = enumerator.Current;
                            string sheetName = fileName + UpperFirst(tDS.Name);
                            List<string> names = new List<string>();
                            List<string> nameTypes = new List<string>();
                            GetSheetNameAndTypes(tDS, ref names, ref nameTypes);
                            MakeCS(sheetName, names, nameTypes, builder, removeField);
                            builder.AppendLine();
                        }
                        string filePath = CSharpOutDir + "/Resource" + fileName + ".cs";
                        if (File.Exists(filePath))
                        {
                            File.Delete(filePath);
                        }
                        File.WriteAllText(filePath, builder.ToString());
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message + " " + fileFullPath);
                }
            }
        }

        static void MakeCS(string fileName, List<string> names, List<string> nameTypes, StringBuilder builder, HashSet<string> removeField)
        {
            string tab = "    ";
            string doubleTab = tab + tab;
            builder.Append(tab).Append("[XmlData(\"").Append(fileName + "\"]").AppendLine();
            builder.Append(tab).Append("public class ").Append(fileName).Append(" : XmlData<").Append(fileName).Append(">").Append(", IStream").AppendLine();
            builder.Append(tab).AppendLine("{");
            int count = nameTypes.Count;
            for (int i = 0; i < count; ++i)
            {
                if (removeField.Contains(names[i]))
                {
                    continue;
                }
                builder.Append(doubleTab).Append("public ").Append(nameTypes[i]).Append(" ").Append(names[i]).Append(" { get; set; }").AppendLine();
            }
            builder.AppendLine();

            builder.Append(doubleTab).Append("public int SaveToStream(NullMemoryStream stream)").AppendLine();
            builder.Append(doubleTab).Append("{").AppendLine();
            builder.Append(doubleTab).Append(tab).Append("int size = stream.WriteInt(id);").AppendLine();
            for (int i = 0; i < count; ++i)
            {
                if (removeField.Contains(names[i]))
                {
                    continue;
                }
                builder.Append(doubleTab).Append(tab).Append("size += ").Append(GetWriteString(names[i], nameTypes[i])).AppendLine();
            }
            builder.Append(doubleTab).Append(tab).Append("return size;").AppendLine();
            builder.Append(doubleTab).Append("}").AppendLine();

            builder.AppendLine();
            builder.Append(doubleTab).Append("public bool LoadFromStream(NullMemoryStream stream)").AppendLine();
            builder.Append(doubleTab).Append("{").AppendLine();
            builder.Append(doubleTab).Append(tab).Append("bool res = stream.ReadInt(out id);").AppendLine();
            for (int i = 0; i < count; ++i)
            {
                if (removeField.Contains(names[i]))
                {
                    continue;
                }
                builder.Append(doubleTab).Append(tab).Append("res &= ").Append(GetReadString(names[i], nameTypes[i])).AppendLine();
            }
            builder.Append(doubleTab).Append(tab).Append("return res;").AppendLine();
            builder.Append(doubleTab).Append("}").AppendLine();
            builder.Append(tab).Append("}").AppendLine();
        }

        public static string GetWriteString(string name, string type)
        {
            if (type == "bool")
            {
                return string.Format("stream.WriteBool({0})", name);
            }
            else if(type == "int")
            {
                return string.Format("stream.WriteInt({0})", name);
            }
            else if (type == "short")
            {
                return string.Format("stream.WriteShort({0})", name);
            }
            else if (type == "string")
            {
                return string.Format("stream.WriteString({0})", name);
            }
            // todo
            return null;
        }

        public static string GetReadString(string name, string type)
        {
            if (type == "bool")
            {
                return string.Format("stream.ReadBool(out {0})", name);
            }
            else if (type == "int")
            {
                return string.Format("stream.ReadInt(out {0})", name);
            }
            else if (type == "short")
            {
                return string.Format("stream.ReadShort(out {0})", name);
            }
            else if (type == "string")
            {
                return string.Format("stream.ReadString(out {0})", name);
            }
            // todo
            return null;
        }

        public static string GetFileName(string fullPath)
        {
            string fileName = Path.GetFileNameWithoutExtension(fullPath);
            fileName = fileName.Trim();
            fileName = Regex.Replace(fileName, @"\d", "");
            string desc = Regex.Replace(fileName, @"[a-zA-Z]+", "");
            fileName = Regex.Replace(fileName, @"[\u4e00-\u9fa5]+", "");
            fileName = char.ToUpper(fileName[0]) + fileName.Substring(1);
            return fileName;
        }

        public static string UpperFirst(string str)
        {
            return char.ToUpper(str[0]) + str.Substring(1);
        }

        public static void ExcelToXml(string fileFullPath)
        {
            FileInfo newFile = new FileInfo(fileFullPath);
            string fileName = GetFileName(fileFullPath);

            using (ExcelPackage pck = new ExcelPackage(newFile))
            {
                try
                {
                    ExcelWorkbook workBook = pck.Workbook;
                    if (workBook != null && workBook.Worksheets.Count > 0)
                    {
                        int cnt = workBook.Worksheets.Count;
                        var enumerator = workBook.Worksheets.GetEnumerator();
                        while (enumerator.MoveNext())
                        {
                            var root = new System.Security.SecurityElement("root");
                            ExcelWorksheet tDS = enumerator.Current;
                            ExcelRange range = tDS.Cells;
                            object[,] values = (object[,])range.Value;
                            int rows = values.GetLength(0);
                            int cols = values.GetLength(1);
                            int nameRow = NameRow - 1;
                            List<string> names = new List<string>();
                            List<string> nameTypes = new List<string>();
                            GetSheetNameAndTypes(tDS, ref names, ref nameTypes);
                            string xmlFileName = fileName + UpperFirst(tDS.Name);

                            cols = names.Count;
                            for (int i = DataRow - 1; i < rows; ++i)
                            {
                                var xml = new System.Security.SecurityElement(xmlFileName);
                                if (values[i, 0] == null)
                                {
                                    break;
                                }
                                for (int j = 0; j < cols; ++j)
                                {
                                    if (values[i, j] == null)
                                    {
                                        values[i, j] = "";
                                    }
                                    xml.AddChild(new System.Security.SecurityElement(names[j], values[i, j].ToString()));
                                }
                                root.AddChild(xml);
                            }
                            string filePath = XMLOutDir + "/" + xmlFileName + ".xml";
                            if (File.Exists(filePath))
                            {
                                File.Delete(filePath);
                            }
                            XElement element = XElement.Parse(root.ToString());
                            File.WriteAllText(filePath, element.ToString());
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message + " " + fileFullPath);
                }
            }
        }

    }
}
