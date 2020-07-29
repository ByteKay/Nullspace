using NullMesh;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Assertions;

namespace Nullspace
{

    public enum PropertyType
    {
        NONE,
        STRING,
        NUMBER,
        VECTOR2,
        VECTOR3,
        VECTOR4,
        MATRIX
    }

    public partial class Properties
    {
        private static Regex RegexVector = new Regex("-?\\d+\\.\\d+");
        public static Properties Create(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                DebugUtils.Info("Create", "Attempting to create a Properties object from an empty URL!");
                return null;
            }
            // Calculate the file and full namespace path from the specified url.
            string urlString = url;
            string fileString = null;
            List<string> namespacePath = new List<string>();
            CalculateNamespacePath(ref urlString, ref fileString, namespacePath);
            using (NullMemoryStream stream = NullMemoryStream.ReadTextFromFile(fileString))
            {
                Properties properties = new Properties(stream);
                properties.ResolveInheritance();
                // Get the specified properties object.
                Properties p = GetPropertiesFromNamespacePath(properties, namespacePath);
                if (p == null)
                {
                    DebugUtils.Warning("Create", "Failed to load properties from url '%s'.", url);
                    return null;
                }
                // If the loaded properties object is not the root namespace,
                // then we have to clone it and delete the root namespace
                // so that we don't leak memory.
                if (p != properties)
                {
                    p = p.Clone();
                }
                p.SetDirectoryPath(Path.GetDirectoryName(fileString));
                return p;
            }
        }
        // Utility functions (shared with SceneLoader).
        public static void CalculateNamespacePath(ref string urlString, ref string fileString, List<string> namespacePath)
        {
            // If the url references a specific namespace within the file,
            // calculate the full namespace path to the final namespace.
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
            // If the url references a specific namespace within the file,
            // return the specified namespace or notify the user if it cannot be found.
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
                            DebugUtils.Warning("getPropertiesFromNamespacePath", "Failed to load properties object from url.");
                            return null;
                        }
                        if (namespacePath[i].Equals(iter.GetId()))
                        {
                            if (i != size - 1)
                            {
                                properties = iter.GetNextNamespace();
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

        //Reads the next character from the stream. Returns EOF if the end of the stream is reached.
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

        // Internal structure containing a single property.
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

        private Properties(NullMemoryStream stream, string name, string id, string parentID, Properties parent)
        {
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
                Assert.IsTrue(mNamespaces[i] != null, "");
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
            Assert.IsTrue(id != null, "");

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

        public PropertyType GetType(string name = null)
        {
            string value = GetString(name);
            if (value == null)
            {
                return PropertyType.NONE;
            }
            value = value.Replace(" ", "");
            // Parse the value to determine the format
            MatchCollection collection = MatchVector(value);

            switch (collection.Count)
            {
                case 1:
                    double d;
                    return double.TryParse(value, out d) ? PropertyType.NUMBER : PropertyType.STRING;
                case 2:
                    return PropertyType.VECTOR2;
                case 3:
                    return PropertyType.VECTOR3;
                case 4:
                    return PropertyType.VECTOR4;
                case 16:
                    return PropertyType.MATRIX;
                default:
                    return PropertyType.STRING;
            }
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
                DebugUtils.Error("GetInt", "Error attempting to parse property '%s' as an integer.", name);
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
                DebugUtils.Error("GetFloat", "Error attempting to parse property '%s' as an integer.", name);
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
                DebugUtils.Error("GetFloat", "Error attempting to parse property '%s' as an integer.", name);
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
                    v[0] = float.Parse(mc[0].Value);
                    v[1] = float.Parse(mc[1].Value);
                    v[2] = float.Parse(mc[2].Value);
                    v[3] = float.Parse(mc[3].Value);
                    v[4] = float.Parse(mc[4].Value);
                    v[5] = float.Parse(mc[5].Value);
                    v[6] = float.Parse(mc[6].Value);
                    v[7] = float.Parse(mc[7].Value);
                    v[8] = float.Parse(mc[8].Value);
                    v[9] = float.Parse(mc[9].Value);
                    v[10] = float.Parse(mc[10].Value);
                    v[11] = float.Parse(mc[11].Value);
                    v[12] = float.Parse(mc[12].Value);
                    v[13] = float.Parse(mc[13].Value);
                    v[14] = float.Parse(mc[14].Value);
                    v[15] = float.Parse(mc[15].Value);
                    return true;
                }
                DebugUtils.Error("GetFloat", "Error attempting to parse property '%s' as an integer.", name);
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
            Assert.IsTrue(name != null && path != null, "");

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
            Assert.IsTrue(name != null, "");

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

        private void ReadProperties(NullMemoryStream stream)
        {
            Assert.IsTrue(stream != null, "");

            string line = "";
            string variable = "";

            string name;
            string value;
            string parentID;
            int c;
            int rc;
            int rcc;
            int rccc;
            bool comment = false;
            // name value
            // name value {
            // name value : parentID
            // name value : parentID {
            // name value : parentID { }
            // {
            // }
            // \t
            // \n
            // \t\n
            // name = value
            // /*
            // */
            // /* */
            // //

            while (true)
            {
                // Stop when we have reached the end of the file.
                if (stream.Eof())
                {
                    break;
                }

                // Read the next line.
                line = stream.ReadLine();
                line = line.Trim();
                // empty row
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
                    }
                    else
                    {
                        if (!line.StartsWith("//"))
                        {
                            // If an '=' appears on this line, parse it as a name/value pair.
                            rc = line.IndexOf("=");
                            if (rc != -1)
                            {
                                // First token should be the property name.
                                name = StrTok(line, "=");
                                if (name == null)
                                {
                                    DebugUtils.Error("ReadProperties", "Error parsing properties file: attribute without name.");
                                    return;
                                }
                                // Remove white-space from name.
                                name = name.Trim();
                                // Scan for next token, the property's value.
                                value = StrTok(null, "");
                                if (value == null)
                                {
                                    DebugUtils.Error("ReadProperties", "Error parsing properties file: attribute with name ('%s') but no value.", name);
                                    return;
                                }
                                // Remove white-space from value.
                                value = value.Trim();

                                // Is this a variable assignment?
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


                                parentID = null;
                                // Get the last character on the line (ignoring whitespace).
                                int lineEnd = line.Length - 1;

                                // This line might begin or end a namespace,
                                // or it might be a key/value pair without '='.

                                // Check for '{' on same line.
                                rc = line.IndexOf("{");
                                // Check for inheritance: ':'
                                rcc = line.IndexOf(":");
                                // Check for '}' on same line.
                                rccc = line.IndexOf("}");
                                // Get the name of the namespace.
                                name = StrTok(line, " \t\n{");
                                name = name != null ? name.Trim() : null;
                                if (name == null)
                                {
                                    DebugUtils.Error("ReadProperties", "Error parsing properties file: failed to determine a valid token for line '%s'.", line);
                                    return;
                                }
                                else
                                {
                                    if (name[0] == '}')
                                    {
                                        // End of namespace.
                                        return;
                                    }
                                }
                                value = StrTok(null, ":{");
                                value = value != null ? value.Trim() : null;
                                // Get its parent ID if it has one.
                                if (rcc != -1)
                                {
                                    parentID = StrTok(null, "{");
                                    parentID = parentID != null ? parentID.Trim() : null;
                                }

                                if (value != null && value[0] == '{')
                                {
                                    // If the namespace ends on this line, seek back to right before the '}' character.
                                    if (rccc != -1 && rccc == lineEnd)
                                    {
                                        while (stream.Peek() != '}')
                                        {
                                            ReadChar(stream);
                                            if (stream.Seek(-2, SeekOrigin.Current) < 0)
                                            {
                                                DebugUtils.Error("ReadProperties", "Failed to seek back to before a '}' character in properties file.");
                                                return;
                                            }
                                        }
                                        if (stream.Seek(-1, SeekOrigin.Current) < 0)
                                        {
                                            DebugUtils.Error("ReadProperties", "Failed to seek back to before a '}' character in properties file.");
                                            return;
                                        }
                                    }

                                    // New namespace without an ID.
                                    Properties space = new Properties(stream, name, null, parentID, this);
                                    mNamespaces.Add(space);

                                    // If the namespace ends on this line, seek to right after the '}' character.
                                    if (rccc != -1 && rccc == lineEnd)
                                    {
                                        if (stream.Seek(1, SeekOrigin.Current) < 0)
                                        {
                                            DebugUtils.Error("ReadProperties", "Failed to seek to immediately after a '}' character in properties file.");
                                            return;
                                        }
                                    }
                                }
                                else
                                {
                                    // If '{' appears on the same line.
                                    if (rc != -1)
                                    {
                                        // If the namespace ends on this line, seek back to right before the '}' character.
                                        if (rccc != -1 && rccc == lineEnd)
                                        {
                                            if (stream.Seek(-1, SeekOrigin.Current) < 0)
                                            {
                                                DebugUtils.Error("ReadProperties", "Failed to seek back to before a '}' character in properties file.");
                                                return;
                                            }
                                            while (ReadChar(stream) != '}')
                                            {
                                                if (stream.Seek(-2, SeekOrigin.Current) < 0)
                                                {
                                                    DebugUtils.Error("ReadProperties", "Failed to seek back to before a '}' character in properties file.");
                                                    return;
                                                }
                                            }
                                            if (stream.Seek(-1, SeekOrigin.Current) < 0)
                                            {
                                                DebugUtils.Error("ReadProperties", "Failed to seek back to before a '}' character in properties file.");
                                                return;
                                            }
                                        }

                                        // Create new namespace.
                                        Properties space = new Properties(stream, name, value, parentID, this);
                                        mNamespaces.Add(space);

                                        // If the namespace ends on this line, seek to right after the '}' character.
                                        if (rccc != -1 && rccc == lineEnd)
                                        {
                                            if (stream.Seek(1, SeekOrigin.Current) < 0)
                                            {
                                                DebugUtils.Error("ReadProperties", "Failed to seek to immediately after a '}' character in properties file.");
                                                return;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        // Find out if the next line starts with "{"
                                        SkipWhiteSpace(stream);
                                        c = ReadChar(stream);
                                        if (c == '{')
                                        {
                                            // Create new namespace.
                                            Properties space = new Properties(stream, name, value, parentID, this);
                                            mNamespaces.Add(space);
                                        }
                                        else
                                        {
                                            // Back up from fgetc()
                                            if (stream.Seek(-1, SeekOrigin.Current) < 0)
                                            {
                                                DebugUtils.Error("ReadProperties", "Failed to seek backwards a single character after testing if the next line starts with '{'.");
                                            }

                                            // Store "name value" as a name/value pair, or even just "name".
                                            if (value != null)
                                            {
                                                mProperties.Add(new Property(name, value));
                                            }
                                            else
                                            {
                                                mProperties.Add(new Property(name, ""));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }


        private void ReadProperties1(NullMemoryStream stream)
        {
            Assert.IsTrue(stream != null, "");

            string line = "";
            string variable = "";

            string name;
            string value;
            string parentID;
            int c;
            int rc;
            int rcc;
            int rccc;
            bool comment = false;

            while (true)
            {
                // Stop when we have reached the end of the file.
                if (stream.Eof())
                {
                    break;
                }

                // Read the next line.
                line = stream.ReadLine();
                if (line == null)
                {
                    DebugUtils.Warning("ReadProperties", "Error reading line from file.");
                    return;
                }
                line = line.Trim();
                // Ignore comments
                if (comment)
                {
                    // Check for end of multi-line comment at either start or end of line
                    if (line.StartsWith("/*"))
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
                    }
                    else
                    {
                        if (!line.StartsWith("//"))
                        {
                            // If an '=' appears on this line, parse it as a name/value pair.
                            rc = line.IndexOf("=");
                            if (rc != -1)
                            {
                                // First token should be the property name.
                                name = line.Substring(0, rc);
                                if (name == null)
                                {
                                    DebugUtils.Error("ReadProperties", "Error parsing properties file: attribute without name.");
                                    return;
                                }
                                // Remove white-space from name.
                                name = name.Trim();
                                // Scan for next token, the property's value.
                                value = line.Substring(rc + 1);
                                if (value == null || string.IsNullOrEmpty(value.Trim()))
                                {
                                    DebugUtils.Error("ReadProperties", "Error parsing properties file: attribute with name ('%s') but no value.", name);
                                    return;
                                }
                                // Remove white-space from value.
                                value = value.Trim();

                                // Is this a variable assignment?
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
                                parentID = null;
                                // Get the last character on the line (ignoring whitespace).
                                int lineEnd = line.Length - 1;

                                // This line might begin or end a namespace,
                                // or it might be a key/value pair without '='.

                                // Check for '{' on same line.
                                rc = line.IndexOf("{");
                                // Check for inheritance: ':'
                                rcc = line.IndexOf(":");
                                // Check for '}' on same line.
                                rccc = line.IndexOf("}");
                                // Get the name of the namespace.
                                string[] strs = line.Split(new char[] { ' ', '\t', '\n', '{' });
                                if (strs.Length == 0)
                                {
                                    DebugUtils.Error("ReadProperties", "Error parsing properties file: failed to determine a valid token for line '%s'.", line);
                                    return;
                                }
                                else
                                {
                                    name = strs[0].Trim();
                                    if (name[0] == '}')
                                    {
                                        // End of namespace.
                                        return;
                                    }
                                }
                                line = line.Remove(0, strs[0].Length);
                                // Get its ID if it has one.
                                strs = line.Split(new char[] { ':', '{' });
                                value = (strs.Length > 0) ? strs[0].Trim() : null;
                                // Get its parent ID if it has one.
                                if (rcc != -1)
                                {
                                    line = line.Remove(0, strs[0].Length);
                                    strs = line.Split(new char[] { '{' });
                                    parentID = (strs.Length > 0) ? strs[0].Trim() : null;
                                }

                                if (value != null && value[0] == '{')
                                {
                                    // If the namespace ends on this line, seek back to right before the '}' character.
                                    if (rccc != -1 && rccc == lineEnd)
                                    {
                                        if (stream.Seek(-1, SeekOrigin.Current) < 0)
                                        {
                                            DebugUtils.Error("ReadProperties", "Failed to seek back to before a '}' character in properties file.");
                                            return;
                                        }
                                        while (ReadChar(stream) != '}')
                                        {
                                            if (stream.Seek(-2, SeekOrigin.Current) < 0)
                                            {
                                                DebugUtils.Error("ReadProperties", "Failed to seek back to before a '}' character in properties file.");
                                                return;
                                            }
                                        }
                                        if (stream.Seek(-1, SeekOrigin.Current) < 0)
                                        {
                                            DebugUtils.Error("ReadProperties", "Failed to seek back to before a '}' character in properties file.");
                                            return;
                                        }
                                    }

                                    // New namespace without an ID.
                                    Properties space = new Properties(stream, name, null, parentID, this);
                                    mNamespaces.Add(space);

                                    // If the namespace ends on this line, seek to right after the '}' character.
                                    if (rccc != -1 && rccc == lineEnd)
                                    {
                                        if (stream.Seek(1, SeekOrigin.Current) < 0)
                                        {
                                            DebugUtils.Error("ReadProperties", "Failed to seek to immediately after a '}' character in properties file.");
                                            return;
                                        }
                                    }
                                }
                                else
                                {
                                    // If '{' appears on the same line.
                                    if (rc != -1)
                                    {
                                        // If the namespace ends on this line, seek back to right before the '}' character.
                                        if (rccc != -1 && rccc == lineEnd)
                                        {
                                            if (stream.Seek(-1, SeekOrigin.Current) < 0)
                                            {
                                                DebugUtils.Error("ReadProperties", "Failed to seek back to before a '}' character in properties file.");
                                                return;
                                            }
                                            while (ReadChar(stream) != '}')
                                            {
                                                if (stream.Seek(-2, SeekOrigin.Current) < 0)
                                                {
                                                    DebugUtils.Error("ReadProperties", "Failed to seek back to before a '}' character in properties file.");
                                                    return;
                                                }
                                            }
                                            if (stream.Seek(-1, SeekOrigin.Current) < 0)
                                            {
                                                DebugUtils.Error("ReadProperties", "Failed to seek back to before a '}' character in properties file.");
                                                return;
                                            }
                                        }

                                        // Create new namespace.
                                        Properties space = new Properties(stream, name, value, parentID, this);
                                        mNamespaces.Add(space);

                                        // If the namespace ends on this line, seek to right after the '}' character.
                                        if (rccc != -1 && rccc == lineEnd)
                                        {
                                            if (stream.Seek(1, SeekOrigin.Current) < 0)
                                            {
                                                DebugUtils.Error("ReadProperties", "Failed to seek to immediately after a '}' character in properties file.");
                                                return;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        // Find out if the next line starts with "{"
                                        SkipWhiteSpace(stream);
                                        c = ReadChar(stream);
                                        if (c == '{')
                                        {
                                            // Create new namespace.
                                            Properties space = new Properties(stream, name, value, parentID, this);
                                            mNamespaces.Add(space);
                                        }
                                        else
                                        {
                                            // Back up from fgetc()
                                            if (stream.Seek(-1, SeekOrigin.Current) < 0)
                                            {
                                                DebugUtils.Error("ReadProperties", "Failed to seek backwards a single character after testing if the next line starts with '{'.");
                                            }

                                            // Store "name value" as a name/value pair, or even just "name".
                                            if (value != null)
                                            {
                                                mProperties.Add(new Property(name, value));
                                            }
                                            else
                                            {
                                                mProperties.Add(new Property(name, ""));
                                            }
                                        }
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
            // "name id : parentID { }"
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
                        Assert.IsTrue(!parent.mVisited, "");
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
                            Assert.IsTrue(parent.mNamespaces[itt] != null, "");
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
            Assert.IsTrue(overrides != null, "");

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
