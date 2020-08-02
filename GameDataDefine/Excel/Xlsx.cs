using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Collections;
using System.Text;

namespace Nullspace
{
    public class Xlsx : IEnumerable<XlsxSheet>, IExportCSharp
    {
        public static Xlsx Create(string filePath)
        {
            string name = Path.GetFileName(filePath);
            Xlsx excel = new Xlsx(name);
            FileInfo newFile = new FileInfo(filePath);
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
                            excel.AddSeet(XlsxSheet.Create(tDS, excel));
                        }
                    }
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
            return excel;
        }

        private List<XlsxSheet> mSheets;

        private Xlsx(string fileName)
        {
            FileName = fileName;
            mSheets = new List<XlsxSheet>();
        }
        private void AddSeet(XlsxSheet sheet)
        {
            mSheets.Add(sheet);
        }

        public string FileName { get; set; }

        public void ExportCSharp(StringBuilder sb)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("using System;");
            builder.AppendLine("using System.Collections.Generic;");
            builder.AppendLine("using System.Text;");
            builder.AppendLine("namespace Nullspace");
            builder.AppendLine("{");
            foreach (XlsxSheet sheet in this)
            {
                sheet.ExportCSharp(builder);
                builder.AppendLine();
            }
            builder.AppendLine("}");
        }

        public IEnumerator<XlsxSheet> GetEnumerator()
        {
            return mSheets.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return mSheets.GetEnumerator();
        }
    }
}
