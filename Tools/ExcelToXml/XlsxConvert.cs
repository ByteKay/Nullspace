﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using OfficeOpenXml;
using System.Xml.Linq;

namespace Nullspace
{
    public class XlsxConvert
    {
        private static string CSharpOutDir;
        private static string XMLOutDir;
        private static bool AllInOne;
        private static StringBuilder CSharpBuilder;

        public static void Convert(string xlsxDir, string outCshapDir, string outXmlDir, bool allInOne, HashSet<string> ignore)
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
            AllInOne = allInOne;
            CSharpBuilder = new StringBuilder();
            string[] files = Directory.GetFiles(xlsxDir, "*.xlsx");

            if(AllInOne)
            {
                CSharpBuilder.AppendLine("using System;");
                CSharpBuilder.AppendLine("using System.Collections.Generic;");
                CSharpBuilder.AppendLine("using System.Text;");
                CSharpBuilder.AppendLine("namespace Nullspace");
                CSharpBuilder.AppendLine("{");
            }
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
            if(AllInOne)
            {
                CSharpBuilder.AppendLine("}");
                string filePath = CSharpOutDir + "/ResourceDatas.cs";
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
                File.WriteAllText(filePath, CSharpBuilder.ToString());
            }
        }

        private static void ExportOneSheet(ExcelWorksheet tDS, int colName, int colType, ref List<string> names,  ref List<string> nameTypes)
        {
            ExcelRange range = tDS.Cells;
            object[,] values = (object[,])range.Value;
            int rows = values.GetLength(0);
            int cols = values.GetLength(1);
            int nameRow = colName - 1;
            for (int j = 0; j < cols; ++j)
            {
                if (values[nameRow, j] == null)
                {
                    break;
                }
                string name = values[nameRow, j].ToString();
                names.Add(name.ToString());
            }
            int nameTypeRow = colType - 1;
            for (int j = 0; j < names.Count; ++j)
            {
                if (values[nameTypeRow, j] == null)
                {
                    values[nameTypeRow, j] = "";
                    return;
                }
                string name = values[nameTypeRow, j].ToString();
                nameTypes.Add(name.ToString());
            }
        }

        public static void GeneratorCS(string fileFullPath, int colName = 1, int colType = 3)
        {
            FileInfo newFile = new FileInfo(fileFullPath);
            string fileName = Path.GetFileNameWithoutExtension(fileFullPath);
            fileName = fileName.Trim();
            List<string> names = new List<string>();
            List<string> nameTypes = new List<string>();
            using (ExcelPackage pck = new ExcelPackage(newFile))
            {
                try
                {
                    ExcelWorkbook workBook = pck.Workbook;
                    if (workBook != null)
                    {
                        int cnt = workBook.Worksheets.Count;
                        var enumerator = workBook.Worksheets.GetEnumerator();
                        while (enumerator.MoveNext())
                        {
                            ExcelWorksheet tDS = enumerator.Current;
                            string sheetName = tDS.Name;
                            ExportOneSheet(tDS, colName, colType, ref names, ref nameTypes);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message + " " + fileFullPath);
                }
            }
            HashSet<string> removeField = new HashSet<string>();
            removeField.Add("id");
            fileName = Regex.Replace(fileName, @"\d", "");
            string desc = Regex.Replace(fileName, @"[a-zA-Z]+", "");
            fileName = Regex.Replace(fileName, @"[\u4e00-\u9fa5]+", "");
            fileName = "Resource" + char.ToUpper(fileName[0]) + fileName.Substring(1);

            if (AllInOne)
            {
                MakeCS(fileName, desc, names, nameTypes, CSharpBuilder, removeField);
            }
            else
            {
                StringBuilder builder = new StringBuilder();
                builder.AppendLine("using System;");
                builder.AppendLine("using System.Collections.Generic;");
                builder.AppendLine("using System.Text;");
                builder.AppendLine("namespace Nullspace");
                builder.AppendLine("{");
                MakeCS(fileName, desc, names, nameTypes, builder, removeField);
                builder.AppendLine("}");
                string filePath = CSharpOutDir + "/" + fileName + ".cs";
                MakeCS(fileName, desc, names, nameTypes, CSharpBuilder, removeField);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
                File.WriteAllText(filePath, builder.ToString());
            }
        }

        static void MakeCS(string fileName, string desc, List<string> names, List<string> nameTypes, StringBuilder builder, HashSet<string> removeField)
        {
            string tab = "    ";
            string doubleTab = tab + tab;
            builder.Append(tab).Append("[XmlData(\"").Append(fileName + ".xml\"").Append(", \"").Append(desc).Append("\")]").AppendLine("");
            builder.Append(tab).Append("public class ").Append(fileName).Append(" : XmlData<").Append(fileName).Append(">").Append(", IStream").AppendLine("");
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
            builder.Append(doubleTab).Append(tab).Append("int size = 0;").AppendLine();
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
            builder.Append(doubleTab).Append(tab).Append("bool res = true;").AppendLine();
            for (int i = 0; i < count; ++i)
            {
                if (removeField.Contains(names[i]))
                {
                    continue;
                }
                builder.Append(doubleTab).Append(tab).Append("res &= ").Append(GetReadString(names[i], nameTypes[i])).AppendLine();
            }
            builder.Append(doubleTab).Append(tab).Append("return res;").AppendLine();
            builder.Append(doubleTab).AppendLine("}");

            builder.Append(tab).AppendLine("}");
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

        public static void ExcelToXml(string fileFullPath, string sheetName = "Sheet1", int colName = 1, int startDataRow = 4)
        {
            FileInfo newFile = new FileInfo(fileFullPath);
            string fileName = Path.GetFileNameWithoutExtension(fileFullPath);
            fileName = fileName.Trim();
            fileName = Regex.Replace(fileName, @"\d", "");
            string desc = Regex.Replace(fileName, @"[a-zA-Z]+", "");
            fileName = Regex.Replace(fileName, @"[\u4e00-\u9fa5]+", "");
            fileName = "Resource" + char.ToUpper(fileName[0]) + fileName.Substring(1);
            using (ExcelPackage pck = new ExcelPackage(newFile))
            {
                try
                {
                    ExcelWorkbook workBook = pck.Workbook;
                    if (workBook != null)
                    {
                        if (workBook.Worksheets.Count > 0)
                        {
                            var root = new System.Security.SecurityElement("root");
                            ExcelWorksheet tDS = workBook.Worksheets[sheetName];
                            ExcelRange range = tDS.Cells;
                            object[,] values = (object[,])range.Value;
                            int rows = values.GetLength(0);
                            int cols = values.GetLength(1);
                            int nameRow = colName - 1;
                            List<string> names = new List<string>();
                            for (int j = 0; j < cols; ++j)
                            {
                                if (values[nameRow, j] == null)
                                {
                                    break;
                                }
                                string name = values[nameRow, j].ToString();
                                names.Add(name.ToString());
                            }
                            cols = names.Count;
                            for (int i = startDataRow - 1; i < rows; ++i)
                            {
                                var xml = new System.Security.SecurityElement(fileName);
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
                            string filePath = XMLOutDir + "/" + fileName + ".xml";
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
