
using System.Collections.Generic;
using System.Security;

namespace Nullspace
{
    public partial class Properties
    {
        public const string CLIENT_XLSX_NODE_TAG = "client_xlsx_datas";
        public const string SERVER_XLSX_NODE_TAG = "server_xlsx_datas";

        public static bool ConvertXlsxPropertiesToXML(Properties prop, out SecurityElement client, out SecurityElement server)
        {
            List<Properties> clientProps = new List<Properties>();
            prop.GetNamespaces(CLIENT_XLSX_NODE_TAG, ref clientProps, false, true);
            List<Properties> serverProps = new List<Properties>();
            prop.GetNamespaces(SERVER_XLSX_NODE_TAG, ref serverProps, false, true);

            client = new SecurityElement(prop.mNamespace);
            foreach (Properties clientProp in clientProps)
            {
                SecurityElement parent = new SecurityElement(clientProp.Parent.mNamespace + "s");
                client.AddChild(parent);
                clientProp.WriteProperties(parent);
            }

            server = new SecurityElement(prop.mNamespace);
            foreach (Properties serverProp in serverProps)
            {
                SecurityElement parent = new SecurityElement(serverProp.Parent.mNamespace + "s");
                server.AddChild(parent);
                serverProp.WriteProperties(parent);
            }
            return true;
        }

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
        private Properties(XlsxSheet sheet, Properties parent) : this()
        {
            mParent = parent;
            ReadProperties(sheet);
            Rewind();
        }

        private Properties(XlsxSheet sheet, DataSideEnum side, string name, Properties parent) : this()
        {
            mNamespace = name;
            mId = name;
            mParent = parent;
            List<int> cols = sheet.GetColumns(side);
            for (int i = 0; i < sheet.RowCount; ++i)
            {
                Properties rowProp = new Properties(sheet, i, cols, sheet.SheetName, this);
                mNamespaces.Add(rowProp);
            }
        }

        private Properties(XlsxSheet sheet, int row, List<int> cols, string name, Properties parent) : this()
        {
            mNamespace = name;
            mId = name;
            mParent = parent;
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
                Properties prop = new Properties(sheet, this);
                mNamespaces.Add(prop);
            }
        }

        private void ReadProperties(XlsxSheet sheet)
        {
            mNamespace = sheet.SheetName;
            mId = sheet.SheetName;
            Properties client = new Properties(sheet, DataSideEnum.C, CLIENT_XLSX_NODE_TAG, this);
            Properties server = new Properties(sheet, DataSideEnum.S, SERVER_XLSX_NODE_TAG, this);
            mNamespaces.Add(client);
            mNamespaces.Add(server);
        }
    }
}
