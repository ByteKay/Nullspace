using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Nullspace
{
    public class XlsxSheet : IExportCSharp
    {
        private const string SKIP_ROW = "#skip";
        private const int VARIABLE_NAME_ROW = 0;
        private const int VARIABLE_TYPE_ROW = 1;
        private const int VARIABLE_SIDE_ROW = 2;
        private const int VARIABLE_DESCRIPTION_ROW = 3;
        private const int VALUE_START_ROW = 4;

        public static XlsxSheet Create(ExcelWorksheet tDS, Xlsx parent)
        {
            XlsxSheet sheet = new XlsxSheet(parent, tDS.Name);
            List<string> variableNames;
            List<DataTypeEnum> variableTypes;
            List<DataSideEnum> variableSides;
            List<XlsxRow> datas = CleanRowAndCols(tDS, sheet, out variableNames, out variableTypes, out variableSides);
            sheet.Set(variableNames, variableTypes, variableSides, datas);
            return sheet;
        }
        private static List<XlsxRow> CleanRowAndCols(ExcelWorksheet tDS, XlsxSheet sheet, out List<string> variableNames, out List<DataTypeEnum> variableTypes, out List<DataSideEnum> variableSides)
        {
            variableNames = new List<string>();
            variableTypes = new List<DataTypeEnum>();
            variableSides = new List<DataSideEnum>();

            ExcelRange range = tDS.Cells;
            object[,] values = (object[,])range.Value;
            int rows = values.GetLength(0);
            int cols = values.GetLength(1);
            HashSet<int> skipCols = new HashSet<int>();
            HashSet<int> skipRows = new HashSet<int>();
            for (int i = 0; i < cols; ++i)
            {
                if (values[VARIABLE_NAME_ROW, i] == null || values[VARIABLE_TYPE_ROW, i] == null || values[VARIABLE_SIDE_ROW, i] == null)
                {
                    // log
                    throw new Exception("not right data");
                }
                string v = values[VARIABLE_SIDE_ROW, i].ToString().Trim().ToLower();
                DataSideEnum side = EnumUtils.StringToEnum<DataSideEnum>(v);
                if (side == DataSideEnum.skip)
                {
                    skipCols.Add(i);
                    continue;
                }
                variableSides.Add(side);
                v = values[VARIABLE_NAME_ROW, i].ToString().Trim();
                variableNames.Add(v);
                v = values[VARIABLE_TYPE_ROW, i].ToString().Trim();
                DataTypeEnum dataType = EnumUtils.StringToEnum<DataTypeEnum>(v);
                variableTypes.Add(dataType);
            }
            for (int i = VALUE_START_ROW; i < rows; ++i)
            {
                if (values[i, 0] != null)
                {
                    string v = values[i, 0].ToString().Trim();
                    if (v.StartsWith(SKIP_ROW))
                    {
                        skipRows.Add(i);
                    }
                }
                else
                {
                    // log
                    throw new Exception(string.Format("null data row[{0}, 0]: ", i + 1));
                }
            }
            List<XlsxRow> originalDatas = new List<XlsxRow>();
            int rowIndex = 0;
            for (int i = VALUE_START_ROW; i < rows; ++i)
            {
                if (skipRows.Contains(i))
                {
                    continue;
                }
                List<string> rowData = new List<string>();
                XlsxRow row = new XlsxRow(rowIndex++, rowData, sheet);
                originalDatas.Add(row);
                for (int j = 0; j < cols; ++j)
                {
                    if (skipCols.Contains(j))
                    {
                        continue;
                    }
                    rowData.Add(values[i, j] != null ? values[i, j].ToString() : "");
                }
            }
            return originalDatas;
        }
        public string SheetName { get; set; }
        private List<string> mVariableNames { get; set; }
        private List<DataTypeEnum> mVariableTypes { get; set; }
        private List<DataSideEnum> mVariableSides { get; set; }
        private List<XlsxRow> mDatas { get; set; }
        private List<int> mClientColumns { get; set; }
        private List<int> mServerColumns { get; set; }
        public Xlsx Parent { get; set; }
        private XlsxSheet(Xlsx parent, string sheetName)
        {
            Parent = parent;
            SheetName = sheetName;
        }
        private void Set(List<string> variableNames, List<DataTypeEnum> variableTypes, List<DataSideEnum> variableSides, List<XlsxRow> datas)
        {
            mVariableNames = variableNames;
            mVariableTypes = variableTypes;
            mVariableSides = variableSides;
            mDatas = datas;
            SplitCS();
        }
        private void SplitCS()
        {
            mClientColumns = new List<int>();
            mServerColumns = new List<int>();
            int count = mVariableSides.Count;
            for (int i = 0; i < count; ++i)
            {
                switch (mVariableSides[i])
                {
                    case DataSideEnum.cs:
                        mClientColumns.Add(i);
                        mServerColumns.Add(i);
                        break;
                    case DataSideEnum.c:
                        mClientColumns.Add(i);
                        break;
                    case DataSideEnum.s:
                        mServerColumns.Add(i);
                        break;
                    case DataSideEnum.skip:
                        throw new Exception("should never be happened here");
                }
            }
        }
        public string this[int row, int col]
        {
            get
            {
                Debug.Assert(row < RowCount && col < ColCount);
                return mDatas[row][col];
            }
        }
        public XlsxRow this[int row]
        {
            get
            {
                return mDatas[row];
            }
        }
        public int RowCount { get { return mDatas.Count; } }
        public int ColCount { get { return RowCount > 0 ? mDatas[0].Count : 0; } }
        public bool GetCol(int i, ref string varName, ref DataTypeEnum varType)
        {
            varName = mVariableNames[i];
            varType = mVariableTypes[i];
            return true;
        }
        public List<int> GetColumns(DataSideEnum side)
        {
            switch (side)
            {
                case DataSideEnum.c:
                    return mClientColumns;
                case DataSideEnum.s:
                    return mServerColumns;
                default:
                    throw new Exception("wrong");
            }
        }

        public void ExportCSharp(StringBuilder builder)
        {
            ExportCSharp(builder, DataSideEnum.c);
        }

        public void ExportCSharp(StringBuilder builder, DataSideEnum side)
        {
            string tab = "    ";
            string doubleTab = tab + tab;
            builder.Append(tab).Append("[GameData(\"").Append(SheetName + "\"]").AppendLine();
            builder.Append(tab).Append("public class ").Append(SheetName).Append(" : XmlData<").Append(SheetName).Append(">").Append(", INullStream").AppendLine();
            builder.Append(tab).AppendLine("{");

            string dataTypeString = "";
            string readString = "";
            string writeString = "";

            List<int> cols = GetColumns(side);
            int count = cols.Count;
            for (int i = 0; i < count; ++i)
            {
                GetType(mVariableTypes[cols[i]], ref dataTypeString, ref readString, ref writeString);
                builder.Append(doubleTab).Append(string.Format("public {0} {1 }{ get; set; }", dataTypeString, mVariableNames[cols[i]])).AppendLine();
            }
            builder.AppendLine();

            builder.Append(doubleTab).Append("public int SaveToStream(NullMemoryStream stream)").AppendLine();
            builder.Append(doubleTab).Append("{").AppendLine();
            builder.Append(doubleTab).Append(tab).Append("int size = stream.WriteInt(id);").AppendLine();
            for (int i = 0; i < count; ++i)
            {
                GetType(mVariableTypes[cols[i]], ref dataTypeString, ref readString, ref writeString);
                builder.Append(doubleTab).Append(tab).Append(string.Format("size += {0}({1})", readString, mVariableNames[cols[i]])).AppendLine();
            }
            builder.Append(doubleTab).Append(tab).Append("return size;").AppendLine();
            builder.Append(doubleTab).Append("}").AppendLine();

            builder.AppendLine();
            builder.Append(doubleTab).Append("public bool LoadFromStream(NullMemoryStream stream)").AppendLine();
            builder.Append(doubleTab).Append("{").AppendLine();
            builder.Append(doubleTab).Append(tab).Append("bool res = stream.ReadInt(out id);").AppendLine();
            for (int i = 0; i < count; ++i)
            {
                GetType(mVariableTypes[cols[i]], ref dataTypeString, ref readString, ref writeString);
                builder.Append(doubleTab).Append(tab).Append(string.Format("res &= {0}({1})", writeString, mVariableNames[cols[i]])).AppendLine();
            }
            builder.Append(doubleTab).Append(tab).Append("return res;").AppendLine();
            builder.Append(doubleTab).Append("}").AppendLine();
            builder.Append(tab).Append("}").AppendLine();
        }



        public static void GetType(DataTypeEnum dt, ref string dataTypeString, ref string readString, ref string writeString)
        {
            switch (dt)
            {
                case DataTypeEnum.Bool:
                    dataTypeString = "bool";
                    readString = "ReadBool";
                    writeString = "WriteBool";
                    return;
                case DataTypeEnum.BoolList:
                    dataTypeString = "List<bool>";
                    readString = "ReadList";
                    writeString = "WriteList";
                    return;
                case DataTypeEnum.Byte:
                    dataTypeString = "byte";
                    readString = "ReadByte";
                    writeString = "WriteByte";
                    return;
                case DataTypeEnum.ByteList:
                    dataTypeString = "List<byte>";
                    readString = "ReadList";
                    writeString = "WriteList";
                    return;
                case DataTypeEnum.Float:
                    dataTypeString = "float";
                    readString = "ReadFloat";
                    writeString = "WriteFloat";
                    return;
                case DataTypeEnum.FloatList:
                    dataTypeString = "List<float>";
                    readString = "ReadList";
                    writeString = "WriteList";
                    return;
                case DataTypeEnum.Int:
                    dataTypeString = "int";
                    readString = "ReadInt";
                    writeString = "WriteInt";
                    return;
                case DataTypeEnum.IntList:
                    dataTypeString = "List<int>";
                    readString = "ReadList";
                    writeString = "WriteList";
                    return;
                case DataTypeEnum.Long:
                    dataTypeString = "long";
                    readString = "ReadLong";
                    writeString = "WriteLong";
                    return;
                case DataTypeEnum.LongList:
                    dataTypeString = "list<long>";
                    readString = "ReadList";
                    writeString = "WriteList";
                    return;
                case DataTypeEnum.Short:
                    dataTypeString = "short";
                    readString = "ReadShort";
                    writeString = "WriteShort";
                    return;
                case DataTypeEnum.ShortList:
                    dataTypeString = "List<short>";
                    readString = "ReadList";
                    writeString = "WriteList";
                    return;
                case DataTypeEnum.UInt:
                    dataTypeString = "uint";
                    readString = "ReadUInt";
                    writeString = "WriteUInt";
                    return;
                case DataTypeEnum.UIntList:
                    dataTypeString = "List<uint>";
                    readString = "ReadList";
                    writeString = "WriteList";
                    return;
                case DataTypeEnum.ULong:
                    dataTypeString = "ulong";
                    readString = "ReadULong";
                    writeString = "WriteULong";
                    return;
                case DataTypeEnum.ULongList:
                    dataTypeString = "List<ulong>";
                    readString = "ReadList";
                    writeString = "WriteList";
                    return;
                case DataTypeEnum.UShort:
                    dataTypeString = "ushort";
                    readString = "ReadUShort";
                    writeString = "WriteUShort";
                    return;
                case DataTypeEnum.UShortList:
                    dataTypeString = "List<ushort>";
                    readString = "ReadList";
                    writeString = "WriteList";
                    return;
            }
            

        }
    }
}
