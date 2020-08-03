
using System.Collections.Generic;

namespace Nullspace
{
    public partial class Properties
    {
        public static Properties CreateFromXlsx(Xlsx root)
        {
            if (root == null)
            {
                return null;
            }
            Properties prop = new Properties(root);
            return prop;
        }

        private Properties(Xlsx root) : this()
        {
            ReadProperties(root);
            Rewind();
        }
        private Properties(XlsxSheet sheet) : this()
        {
            ReadProperties(sheet);
            Rewind();
        }

        private Properties(XlsxSheet sheet, DataSideEnum side, string name) : this()
        {
            mNamespace = name;
            mId = name;
            List<int> cols = sheet.GetColumns(side);
            for (int i = 0; i < sheet.RowCount; ++i)
            {
                Properties rowProp = new Properties(sheet, i, cols, name + i);
                mNamespaces.Add(rowProp);
            }
        }

        private Properties(XlsxSheet sheet, int row, List<int> cols, string name) : this()
        {
            mNamespace = name;
            mId = name;
            for (int i = 0; i < cols.Count; ++i)
            {
                int colIndex = cols[i];
                string varName = null;
                DataTypeEnum varType = DataTypeEnum.NONE;
                sheet.GetCol(colIndex, ref varName, ref varType);
                string value = sheet[row, colIndex];
                mProperties.Add(new Property(varName, value));
            }
        }

        private void ReadProperties(Xlsx root)
        {
            mNamespace = root.FileName;
            mId = root.FileName;
            foreach (XlsxSheet sheet in root)
            {
                Properties prop = new Properties(sheet);
                mNamespaces.Add(prop);
            }
        }

        private void ReadProperties(XlsxSheet sheet)
        {
            mNamespace = sheet.SheetName;
            mId = sheet.SheetName;
            Properties client = new Properties(sheet, DataSideEnum.C, "client");
            Properties server = new Properties(sheet, DataSideEnum.S, "server");
            mNamespaces.Add(client);
            mNamespaces.Add(server);
        }



    }
}
