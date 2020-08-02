
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using System.Diagnostics;
using System.Security;

namespace Nullspace
{
    public partial class Properties
    {
        private static Regex RegexVector = new Regex("-?\\d+\\.\\d+");

        public static SecurityElement ConvertPropertiesToXML(Properties prop)
        {
            SecurityElement xml = new SecurityElement(prop.mNamespace);
            prop.WriteProperties(xml);
            return xml;
        }

        public static Properties CreateFromXml(SecurityElement root, string urlString = null)
        {
            if (root == null)
            {
                return null;
            }
            Properties properties = new Properties(root);
            return properties;
        }

        public static Properties CreateFromContent(string content, string urlString = null)
        {
            if (string.IsNullOrEmpty(content))
            {
                Debug.Assert(false, "Attempting to create a Properties object from an empty URL!");
                return null;
            }
            // Calculate the file and full namespace path from the specified url.
            string fileString = null;
            List<string> namespacePath = new List<string>();
            if (urlString != null)
            {
                CalculateNamespacePath(ref urlString, ref fileString, namespacePath);
            }
            using (NullMemoryStream stream = NullMemoryStream.ReadTextFromString(content))
            {
                Properties properties = new Properties(stream);
                properties.ResolveInheritance();
                // Get the specified properties object.
                Properties p = GetPropertiesFromNamespacePath(properties, namespacePath);
                if (p == null)
                {
                    Debug.Assert(false, string.Format("Failed to load properties from url {0}.", urlString));
                    return null;
                }
                if (p != properties)
                {
                    p = p.Clone();
                }
                if (fileString != null)
                {
                    p.SetDirectoryPath(Path.GetDirectoryName(fileString));
                }
                p.Rewind();
                return p;
            }
        }

        public static Properties Create(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                Debug.Assert(false, "Attempting to create a Properties object from an empty URL!");
                return null;
            }
            // Calculate the file and full namespace path from the specified url.
            string urlString = url;
            string fileString = null;
            List<string> namespacePath = new List<string>();
            CalculateNamespacePath(ref urlString, ref fileString, namespacePath);
            return CreateFromContent(File.ReadAllText(fileString), url);
        }

        public static void CalculateNamespacePath(ref string urlString, ref string fileString, List<string> namespacePath)
        {
            int loc = urlString.IndexOf("#");
            if (loc != -1)
            {
                fileString = urlString.Substring(0, loc);
                string namespacePathString = urlString.Substring(loc + 1);
                while ((loc = namespacePathString.IndexOf("/")) != -1)
                {
                    namespacePath.Add(namespacePathString.Substring(0, loc));
                    namespacePathString = namespacePathString.Substring(loc + 1);
                }
                namespacePath.Add(namespacePathString);
            }
            else
            {
                fileString = urlString;
            }
        }

        public static Properties GetPropertiesFromNamespacePath(Properties properties, List<string> namespacePath)
        {
            if (namespacePath.Count > 0)
            {
                int size = namespacePath.Count;
                properties.Rewind();
                Properties iter = properties.GetNextNamespace();
                for (int i = 0; i < size;)
                {
                    while (true)
                    {
                        if (iter == null)
                        {
                            Debug.Assert(false, "Failed to load properties object from url.");
                            return null;
                        }
                        if (namespacePath[i].Equals(iter.GetId())) // id, not namespace
                        {
                            if (i != size - 1)
                            {
                                properties = iter.GetNextNamespace();
                                iter.Rewind();
                                iter = properties;
                            }
                            else
                            {
                                properties = iter;
                            }
                            i++;
                            break;
                        }
                        iter = properties.GetNextNamespace();
                    }
                }
                properties.Rewind();
                return properties;
            }
            return properties;
        }

        public static bool ParseVector2(string str, out Vector2 v)
        {
            v = Vector2.zero;
            MatchCollection collection = MatchVector(str);
            if (collection.Count == 2)
            {
                v.x = float.Parse(collection[0].Value);
                v.y = float.Parse(collection[1].Value);
                return true;
            }
            return false;
        }

        public static bool ParseVector3(string str, out Vector3 v)
        {
            v = Vector3.zero;
            MatchCollection collection = MatchVector(str);
            if (collection.Count == 3)
            {
                v.x = float.Parse(collection[0].Value);
                v.y = float.Parse(collection[1].Value);
                v.z = float.Parse(collection[2].Value);
                return true;
            }
            return false;
        }

        public static bool ParseVector4(string str, out Vector4 v)
        {
            v = Vector4.zero;
            MatchCollection collection = MatchVector(str);
            if (collection.Count == 4)
            {
                v.x = float.Parse(collection[0].Value);
                v.y = float.Parse(collection[1].Value);
                v.z = float.Parse(collection[2].Value);
                v.w = float.Parse(collection[3].Value);
                return true;
            }
            return false;
        }

        public static bool ParseAxisAngle(string str, out Quaternion v)
        {
            v = Quaternion.identity;
            float angle;
            if (float.TryParse(str, out angle))
            {
                v = Quaternion.AngleAxis(angle, Vector3.up);
                return true;
            }
            return false;
        }

        public static bool ParseColor(string str, out Color v)
        {
            v = Color.black;
            MatchCollection collection = MatchVector(str);
            if (collection.Count == 3)
            {
                v.r = float.Parse(collection[0].Value);
                v.g = float.Parse(collection[1].Value);
                v.b = float.Parse(collection[2].Value);
                v.a = 1;
                return true;
            }
            if (collection.Count == 4)
            {
                v.r = float.Parse(collection[0].Value);
                v.g = float.Parse(collection[1].Value);
                v.b = float.Parse(collection[2].Value);
                v.a = float.Parse(collection[3].Value);
                return true;
            }
            return false;
        }

        public static MatchCollection MatchVector(string inputString)
        {
            return RegexVector.Matches(inputString);
        }

        public static bool IsVariable(string str, ref string outName)
        {
            int len = str.Length;
            if (len > 3 && str[0] == '$' && str[1] == '{' && str[len - 1] == '}')
            {
                outName = str.Substring(2, len - 3);
                return true;
            }
            return false;
        }

        public static void SkipWhiteSpace(NullMemoryStream stream)
        {
            int c;
            do
            {
                c = stream.Peek();
                if (c == -1)
                {
                    break;
                }
                if (char.IsWhiteSpace((char)c))
                {
                    stream.Read();
                }
            } while (c != -1);
        }

        public static int ReadChar(NullMemoryStream stream)
        {
            if (stream.Eof())
            {
                return -1;
            }
            int ch = stream.Read();
            return ch;
        }

        public static string TrimWhiteSpace(string str)
        {
            if (str == null)
            {
                return null;
            }
            return str.Trim();
        }

        private static string Activestring = null;
        private static int Activeposition = 0;
        private static string StrTok(string stringtotokenize, string delimiters)
        {
            if (stringtotokenize != null)
            {
                Activestring = stringtotokenize;
                Activeposition = -1;
            }

            //the stringtotokenize was never set:
            if (Activestring == null)
            {
                return null;
            }

            //all tokens have already been extracted:
            if (Activeposition == Activestring.Length)
            {
                return null;
            }

            //bypass delimiters:
            Activeposition++;
            while (Activeposition < Activestring.Length && delimiters.IndexOf(Activestring[Activeposition]) > -1)
            {
                Activeposition++;
            }

            //only delimiters were left, so return null:
            if (Activeposition == Activestring.Length)
            {
                return null;
            }

            //get starting position of string to return:
            int startingposition = Activeposition;

            //read until next delimiter:
            do
            {
                Activeposition++;
            } while (Activeposition < Activestring.Length && delimiters.IndexOf(Activestring[Activeposition]) == -1);

            return Activestring.Substring(startingposition, Activeposition - startingposition);
        }
    }

    public partial class Properties
    {
        private Properties mParent;
        private string mNamespace;
        private string mId;
        private string mParentID;
        private List<Property> mProperties;
        private List<Properties> mNamespaces;
        private List<Property> mVariables;
        private string mDirPath;
        private bool mVisited;
        private int mPropertiesItr;
        private int mNamespacesItr;

        private class Property
        {
            public string Name;
            public string Value;
            public Property(string name, string value)
            {
                Name = name;
                Value = value;
            }

            public Property(Property other)
            {
                Name = other.Name;
                Value = other.Value;
            }
        }

        private Properties()
        {
            mVariables = null;
            mDirPath = null;
            mParent = null;
            mVisited = false;
            mProperties = new List<Property>();
            mNamespaces = new List<Properties>();
        }

        private Properties(NullMemoryStream stream) : this()
        {
            ReadProperties(stream);
            Rewind();
        }

        private Properties(Properties copy)
        {
            mNamespace = copy.mNamespace;
            mId = copy.mId;
            mParentID = copy.mParentID;
            mVisited = copy.mVisited;
            mParent = copy.mParent;
            mDirPath = null;
            mVariables = null;
            SetDirectoryPath(copy.mDirPath);
            mProperties = new List<Property>();
            mNamespaces = new List<Properties>();
            for (int it = 0; it < copy.mProperties.Count; ++it)
            {
                mProperties.Add(new Property(copy.mProperties[it]));
            }
            for (int it = 0; it < copy.mNamespaces.Count; ++it)
            {
                mNamespaces.Add(new Properties(copy.mNamespaces[it]));
            }
            Rewind();
        }

        private Properties(NullMemoryStream stream, string name, string id, string parentID, Properties parent) : this()
        {
            mNamespace = name;
            mVariables = null;
            mDirPath = null;
            mParent = parent;
            mVisited = false;
            if (id != null)
            {
                mId = id;
            }
            if (parentID != null)
            {
                mParentID = parentID;
            }
            ReadProperties(stream);
            Rewind();
        }
        private Properties (SecurityElement root) : this()
        {
            ReadProperties(root);
            Rewind();
        }

        // Clones the Properties object.
        private Properties Clone()
        {
            Properties p = new Properties();

            p.mNamespace = mNamespace;
            p.mId = mId;
            p.mParentID = mParentID;
            p.mProperties = mProperties;
            p.mPropertiesItr = p.mProperties.Count;
            p.SetDirectoryPath(mDirPath);

            for (int i = 0, count = mNamespaces.Count; i < count; i++)
            {
                Debug.Assert(mNamespaces[i] != null, "");
                Properties child = mNamespaces[i].Clone();
                p.mNamespaces.Add(child);
                child.mParent = p;
            }
            p.mNamespacesItr = p.mNamespaces.Count;

            return p;
        }

        public void PrintAll(StringBuilder sb, int ident = 0)
        {
            string space = "";
            for (int i = 0; i < ident; ++i)
            {
                space += "    ";
            }
            sb.Append(space).AppendFormat("{0} {1} {2} {3}", mNamespace, mId, mDirPath, mParentID).AppendLine();
            foreach (Property prop in mProperties)
            {
                sb.Append(space).Append("    ").AppendFormat("{0} = {1}", prop.Name, prop.Value).AppendLine();
            }
            if (mVariables != null)
            {
                foreach (Property variable in mVariables)
                {
                    sb.Append(space).Append("    ").AppendFormat("{0} = {1}", variable.Name, variable.Value).AppendLine();
                }
            }

            Properties p = null;
            while ((p = GetNextNamespace()) != null)
            {
                p.PrintAll(sb, ident + 1);
                sb.AppendLine();
            }
        }

        public string GetNextProperty()
        {
            if (mPropertiesItr == mProperties.Count)
            {
                // Restart from the beginning
                mPropertiesItr = 0;
            }
            else
            {
                // Move to the next property
                ++mPropertiesItr;
            }

            return mPropertiesItr == mProperties.Count ? null : mProperties[mPropertiesItr].Name;
        }

        public Properties GetNextNamespace()
        {
            if (mNamespacesItr == mNamespaces.Count)
            {
                // Restart from the beginning
                mNamespacesItr = 0;
            }
            else
            {
                ++mNamespacesItr;
            }
            if (mNamespacesItr != mNamespaces.Count)
            {
                Properties ns = mNamespaces[mNamespacesItr];
                return ns;
            }
            return null;
        }

        public void Rewind()
        {
            mPropertiesItr = mProperties.Count;
            mNamespacesItr = mNamespaces.Count;
        }

        public Properties GetNamespace(string id, bool searchNames = false, bool recurse = true)
        {
            Debug.Assert(id != null, "");

            for (int i = 0; i < mNamespaces.Count; ++i)
            {
                Properties p = mNamespaces[i];
                if (id.Equals(searchNames ? p.mNamespace : p.mId))
                {
                    return p;
                }

                if (recurse)
                {
                    // Search recursively.
                    p = p.GetNamespace(id, searchNames, true);
                    if (p != null)
                    {
                        return p;
                    }
                }
            }
            return null;
        }

        public string GetNamespace()
        {
            return mNamespace;
        }

        public string GetId()
        {
            return mId;
        }

        public bool Exists(string name)
        {
            if (name == null)
            {
                return false;
            }

            for (int itr = 0; itr < mProperties.Count; ++itr)
            {
                if (mProperties[itr].Name == name)
                {
                    return true;
                }
            }
            return false;
        }

        public string GetString(string name = null, string defaultValue = null)
        {
            string variable = null;
            string value = null;
            if (name != null)
            {
                // If 'name' is a variable, return the variable value
                if (IsVariable(name, ref variable))
                {
                    return GetVariable(variable, defaultValue);
                }
                for (int i = 0; i < mProperties.Count; ++i)
                {
                    if (mProperties[i].Name == name)
                    {
                        value = mProperties[i].Value;
                        break;
                    }
                }
            }
            else
            {
                // No name provided - get the value at the current iterator position
                if (mPropertiesItr != mProperties.Count)
                {
                    value = mProperties[mPropertiesItr].Value;
                }
            }

            if (value != null)
            {
                // If the value references a variable, return the variable value
                if (IsVariable(value, ref variable))
                {
                    return GetVariable(variable, defaultValue);
                }
                return value;
            }
            return defaultValue;
        }

        public bool SetString(string name, string value)
        {
            if (name != null)
            {
                for (int itr = 0; itr < mProperties.Count; ++itr)
                {
                    if (mProperties[itr].Name == name)
                    {
                        // Update the first property that matches this name
                        mProperties[itr].Value = value != null ? value : "";
                        return true;
                    }
                }
                // There is no property with this name, so add one
                mProperties.Add(new Property(name, value != null ? value : ""));
            }
            else
            {
                // If there's a current property, set its value
                if (mPropertiesItr == mProperties.Count)
                {
                    return false;
                }
                mProperties[mPropertiesItr].Value = value != null ? value : "";
            }

            return true;
        }

        public bool GetBool(string name = null, bool defaultValue = false)
        {
            string valueString = GetString(name);
            if (valueString != null)
            {
                return valueString == "true";
            }
            return defaultValue;
        }

        public int GetInt(string name = null)
        {
            string valueString = GetString(name);
            if (valueString != null)
            {
                int value;
                if (int.TryParse(valueString, out value))
                {
                    return value;
                }
                Debug.Assert(false, string.Format("Error attempting to parse property {0} as an integer.", name));
            }

            return 0;
        }

        public float GetFloat(string name = null)
        {
            string valueString = GetString(name);
            if (valueString != null)
            {
                float value;
                if (float.TryParse(valueString, out value))
                {
                    return value;
                }
                Debug.Assert(false, string.Format("Error attempting to parse property {0} as an integer.", name));
            }

            return 0;
        }

        public long GetLong(string name = null)
        {
            string valueString = GetString(name);
            if (valueString != null)
            {
                long value;
                if (long.TryParse(valueString, out value))
                {
                    return value;
                }
                Debug.Assert(false, string.Format("Error attempting to parse property {0} as an integer.", name));
            }

            return 0;
        }

        public bool GetMatrix(string name, out Matrix4x4 v)
        {
            string valueString = GetString(name);
            v = Matrix4x4.zero;
            if (valueString != null)
            {
                MatchCollection mc = MatchVector(valueString);
                if (mc.Count == 16)
                {
                    v.m00 = float.Parse(mc[0].Value);
                    v.m01 = float.Parse(mc[1].Value);
                    v.m02 = float.Parse(mc[2].Value);
                    v.m03 = float.Parse(mc[3].Value);
                    v.m10 = float.Parse(mc[4].Value);
                    v.m11 = float.Parse(mc[5].Value);
                    v.m12 = float.Parse(mc[6].Value);
                    v.m13 = float.Parse(mc[7].Value);
                    v.m20 = float.Parse(mc[8].Value);
                    v.m21 = float.Parse(mc[9].Value);
                    v.m22 = float.Parse(mc[10].Value);
                    v.m23 = float.Parse(mc[11].Value);
                    v.m30 = float.Parse(mc[12].Value);
                    v.m31 = float.Parse(mc[13].Value);
                    v.m32 = float.Parse(mc[14].Value);
                    v.m33 = float.Parse(mc[15].Value);
                    return true;
                }
                Debug.Assert(false, string.Format("Error attempting to parse property {0} as an Matrix4x4.", name));
            }
            return false;
        }

        public bool GetVector2(string name, out Vector2 v)
        {
            return ParseVector2(GetString(name), out v);
        }

        public bool GetVector3(string name, out Vector3 v)
        {
            return ParseVector3(GetString(name), out v);
        }

        public bool GetVector4(string name, out Vector4 v)
        {
            return ParseVector4(GetString(name), out v);
        }

        public bool GetQuaternionFromAxisAngle(string name, out Quaternion v)
        {
            return ParseAxisAngle(GetString(name), out v);
        }

        public bool GetColor(string name, out Color v)
        {
            return ParseColor(GetString(name), out v);
        }

        public bool GetPath(string name, string path)
        {
            Debug.Assert(name != null && path != null, "");

            string valueString = GetString(name);
            if (valueString != null)
            {
                bool found = false;
                if (File.Exists(valueString))
                {
                    path = valueString;
                    found = true;
                }
                else
                {
                    Properties prop = this;
                    while (prop != null)
                    {
                        // Search for the file path relative to the bundle file
                        string dirPath = prop.mDirPath;
                        if (!string.IsNullOrEmpty(dirPath))
                        {
                            string relativePath = dirPath;
                            relativePath += valueString;
                            if (File.Exists(relativePath))
                            {
                                path = relativePath;
                                found = true;
                                break;
                            }
                        }
                        prop = prop.mParent;
                    }
                }
                return found;
            }
            return false;
        }

        public string GetVariable(string name, string defaultValue = null)
        {
            if (name == null)
            {
                return defaultValue;
            }
            // Search for variable in this Properties object
            if (mVariables != null)
            {
                for (int i = 0, count = mVariables.Count; i < count; ++i)
                {
                    Property prop = mVariables[i];
                    if (prop.Name == name)
                    {
                        return prop.Value;
                    }
                }
            }
            // Search for variable in parent Properties
            return mParent != null ? mParent.GetVariable(name, defaultValue) : defaultValue;
        }

        public void SetVariable(string name, string value)
        {
            Debug.Assert(name != null, "");

            Property prop = null;

            // Search for variable in this Properties object and parents
            Properties current = this;
            while (current != null)
            {
                if (current.mVariables != null)
                {
                    for (int i = 0, count = current.mVariables.Count; i < count; ++i)
                    {
                        Property p = current.mVariables[i];
                        if (p.Name == name)
                        {
                            prop = p;
                            break;
                        }
                    }
                }
                current = current.mParent;
            }
            if (prop != null)
            {
                // Found an existing property, set it
                prop.Value = value != null ? value : "";
            }
            else
            {
                // Add a new variable with this name
                if (mVariables == null)
                {
                    mVariables = new List<Property>();
                }
                mVariables.Add(new Property(name, value != null ? value : ""));
            }
        }
        private void WriteProperties(SecurityElement xml)
        {
            WriteAttributes(xml);
            for (int i = 0; i < mNamespaces.Count; ++i)
            {
                Properties child =  mNamespaces[i];
                SecurityElement xmlChild = new SecurityElement(child.mNamespace);
                child.WriteProperties(xmlChild);
                xml.AddChild(xmlChild);
            }
        }

        private void WriteAttributes(SecurityElement node)
        {
            if (mProperties != null)
            {
                foreach (Property prop in mProperties)
                {
                    node.AddAttribute(prop.Name, prop.Value);
                }
            }
        }

        private void ReadProperties(SecurityElement xml)
        {
            mNamespace = xml.Tag;
            mId = xml.Tag;
            int count = xml.Children.Count;
            ReadAttributes(xml);
            ReadTextValue(xml);
            for (int i = 0; i < count; ++i)
            {
                SecurityElement child = xml.Children[i] as SecurityElement;
                if (child.Children == null && child.Attributes == null)
                {
                    // property
                    ReadTextValue(child);
                }
                else
                {
                    // namespace
                    Properties childProp = new Properties(child);
                    mNamespaces.Add(childProp);
                }
            }
        }

        private void ReadTextValue(SecurityElement node)
        {
            if (!string.IsNullOrEmpty(node.Text))
            {
                string value = node.Text.Trim();
                mProperties.Add(new Property(node.Tag, value));
            }
        }

        private void ReadAttributes(SecurityElement node)
        {
            if (node.Attributes != null)
            {
                foreach (string key in node.Attributes.Keys)
                {
                    string value = node.Attributes[key].ToString();
                    mProperties.Add(new Property(key, value));
                }
            }
        }

        /// <summary>
        /// '{' 和 '}' 一定要 单独一行
        /// </summary>
        /// <param name="stream"></param>
        private void ReadProperties(NullMemoryStream stream)
        {
            Debug.Assert(stream != null, "");
            string namesp = null;
            string id = null;
            string parentID = null;
            bool comment = false;
            while (true)
            {
                // Stop when we have reached the end of the file.
                if (stream.Eof())  // 最后如果以  property 结束，需要加上最后一行的数据
                {
                    if (namesp != null)
                    {
                        mProperties.Add(new Property(namesp, id == null ? "" : id));
                    }
                    break;
                }

                // Read the next line.
                string line = stream.ReadLine();
                line = line.Trim();
                // Empty row
                if (string.IsNullOrEmpty(line))
                {
                    continue;
                }
                // Ignore comments
                if (comment)
                {
                    // Check for end of multi-line comment at either start or end of line
                    if (line.StartsWith("*/"))
                    {
                        comment = false;
                    }
                    else
                    {
                        if (line.EndsWith("*/"))
                        {
                            comment = false;
                        }
                    }
                }
                else
                {
                    if (line.StartsWith("/*"))
                    {
                        // Start of multi-line comment (must be at start of line)
                        comment = true;
                        if (line.EndsWith("*/")) // same line
                        {
                            comment = false;
                        }
                    }
                    else
                    {
                        int cc = line.IndexOf("//");// get data before first '//'
                        if (cc != -1)
                        {
                            line = line.Substring(0, cc);
                        }
                        if (!string.IsNullOrEmpty(line))
                        {
                            // If an '=' appears on this line, parse it as a name/value pair.
                            int rc = line.IndexOf("=");
                            if (rc != -1)
                            {
                                // First token should be the property name.

                                string[] strs = line.Split('=');
                                string name = strs[0];
                                // Remove white-space from name.
                                name = name.Trim();
                                // Scan for next token, the property's value.
                                string value = strs[1];
                                // Remove white-space from value.
                                value = value.Trim();
                                // Is this a variable assignment?
                                string variable = "";
                                if (IsVariable(name, ref variable))
                                {
                                    SetVariable(variable, value);
                                }
                                else
                                {
                                    // Normal name/value pair
                                    mProperties.Add(new Property(name, value));
                                }
                            }
                            else
                            {
                                // This line might begin or end a namespace,
                                // or it might be a key/value pair without '='.

                                // Check for '{' on same line.
                                rc = line.IndexOf("{");
                                if (rc != -1)
                                {
                                    if (line.Length > 1)
                                    {
                                        Debug.Assert(false, "Error parsing this line should be only '{' : " + line);
                                        return;
                                    }
                                }

                                // Check for '}' on same line.
                                int rcc = line.IndexOf("}");
                                if (rcc != -1)
                                {
                                    if (line.Length > 1)
                                    {
                                        Debug.Assert(false, "Error parsing this line should be only '}' : " + line);
                                        return;
                                    }
                                    // End of namespace.
                                    return;
                                }

                                if (rc != -1)
                                {
                                    if (string.IsNullOrEmpty(namesp))
                                    {
                                        Debug.Assert(false, "ReadProperties Error parsing  namespace");
                                        return;
                                    }
                                    // New namespace without an ID.
                                    Properties space = new Properties(stream, namesp, id, parentID, this);
                                    mNamespaces.Add(space);
                                    // 重置
                                    namesp = null;
                                    id = null;
                                    parentID = null;
                                }
                                else
                                {
                                    if (namesp != null)
                                    {
                                        mProperties.Add(new Property(namesp, id == null ? "" : id));
                                    }
                                    // 记录上一行的信息
                                    namesp = null;
                                    id = null;
                                    parentID = null;

                                    string[] strs = line.Split(new char[] { ':' });
                                    string namevalue = strs[0].Trim();
                                    if (strs.Length == 2)
                                    {
                                        parentID = strs[1].Trim();
                                    }
                                    namesp = StrTok(namevalue, " ");
                                    if (namesp != null)
                                    {
                                        namesp = namesp.Trim();
                                    }
                                    if (string.IsNullOrEmpty(namesp))
                                    {
                                        namesp = null;
                                        Debug.Assert(false, "Error parsing this line should be not null : " + line);
                                        return;
                                    }
                                    id = StrTok(null, "");
                                    if (id != null)
                                    {
                                        id = id.Trim();
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        // Called after create(); copies info from parents into derived namespaces.
        private void ResolveInheritance(string id = null)
        {
            // Namespaces can be defined like so:
            // "name id : parentID "
            // This method merges data from the parent namespace into the child.

            // Get a top-level namespace.
            Properties derived;
            if (id != null)
            {
                derived = GetNamespace(id);
            }
            else
            {
                derived = GetNextNamespace();
            }
            while (derived != null)
            {
                // If the namespace has a parent ID, find the parent.
                if (!string.IsNullOrEmpty(derived.mParentID))
                {
                    derived.mVisited = true;
                    Properties parent = GetNamespace(derived.mParentID);
                    if (parent != null)
                    {
                        Debug.Assert(!parent.mVisited, "");
                        ResolveInheritance(parent.GetId());

                        // Copy the child.
                        Properties overrides = new Properties(derived);

                        // Delete the child's data.
                        for (int i = 0, count = derived.mNamespaces.Count; i < count; i++)
                        {
                            // derived.mNamespaces[i]
                        }

                        // Copy data from the parent into the child.
                        derived.mProperties = parent.mProperties;
                        derived.mNamespaces = new List<Properties>();
                        for (int itt = 0; itt < parent.mNamespaces.Count; ++itt)
                        {
                            Debug.Assert(parent.mNamespaces[itt] != null, "");
                            derived.mNamespaces.Add(new Properties(parent.mNamespaces[itt]));
                        }
                        derived.Rewind();

                        // Take the original copy of the child and override the data copied from the parent.
                        derived.MergeWith(overrides);
                    }
                    derived.mVisited = false;
                }

                // Resolve inheritance within this namespace.
                derived.ResolveInheritance();

                // Get the next top-level namespace and check again.
                if (id != null)
                {
                    derived = GetNextNamespace();
                }
                else
                {
                    derived = null;
                }
            }
        }

        // Called by resolveInheritance().
        private void MergeWith(Properties overrides)
        {
            Debug.Assert(overrides != null, "");

            // Overwrite or add each property found in child.
            overrides.Rewind();
            string name = overrides.GetNextProperty();
            while (name != null)
            {
                this.SetString(name, overrides.GetString());
                name = overrides.GetNextProperty();
            }
            mPropertiesItr = mProperties.Count;

            // Merge all common nested namespaces, add new ones.
            Properties overridesNamespace = overrides.GetNextNamespace();
            while (overridesNamespace != null)
            {
                bool merged = false;

                Rewind();
                Properties derivedNamespace = GetNextNamespace();
                while (derivedNamespace != null)
                {
                    if (derivedNamespace.GetNamespace() == overridesNamespace.GetNamespace() &&
                        derivedNamespace.GetId() == overridesNamespace.GetId())
                    {
                        derivedNamespace.MergeWith(overridesNamespace);
                        merged = true;
                    }

                    derivedNamespace = GetNextNamespace();
                }

                if (!merged)
                {
                    // Add this new namespace.
                    Properties newNamespace = new Properties(overridesNamespace);

                    mNamespaces.Add(newNamespace);
                    mNamespacesItr = this.mNamespaces.Count;
                }

                overridesNamespace = overrides.GetNextNamespace();
            }
        }

        private void SetDirectoryPath(string path)
        {
            mDirPath = path;
        }
    }
}
