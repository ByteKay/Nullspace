using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace Nullspace
{
    public class FileUtils
    {
        public static string GetFileNameWithoutExtention(string fileName, char separator = '/')
        {
            var name = GetFileName(fileName, separator);
            return GetFilePathWithoutExtention(name);
        }

        public static string GetFilePathWithoutExtention(string fileName)
        {
            return fileName.Substring(0, fileName.LastIndexOf('.'));
        }

        public static string GetFileExtention(string fileName)
        {
            return fileName.Substring(fileName.LastIndexOf('.') + 1);
        }

        public static string GetDirectoryName(string fileName)
        {
            int index = fileName.LastIndexOf('/');
            index = index == -1 ? fileName.LastIndexOf("\\") : index;
            if (index == -1)
            {
                index = fileName.LastIndexOf(':');
                if (index == -1)
                    return ".";
                else
                {
                    return fileName;
                }
            }
            return fileName.Substring(0, index);
        }

        public static string GetFileName(string path, char separator = '/')
        {
            return path.Substring(path.LastIndexOf(separator) + 1);
        }

        public static string PathNormalize(string str)
        {
            return str.Replace("\\", "/").ToLower();
        }

        /// <summary>
        /// 生成文件的md5
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static String BuildFileMd5(String filename)
        {
            String filemd5 = null;
            try
            {
                using (var fileStream = File.OpenRead(filename))
                {
                    var md5 = MD5.Create();
                    var fileMD5Bytes = md5.ComputeHash(fileStream);//计算指定Stream 对象的哈希值                            
                    //fileStream.Close();//流数据比较大，手动卸载 
                    //fileStream.Dispose();
                    //由以连字符分隔的十六进制对构成的String，其中每一对表示value 中对应的元素；例如“F-2C-4A”               
                    filemd5 = FormatMD5(fileMD5Bytes);
                }
            }
            catch (System.Exception ex)
            {
                DebugUtils.Error("", "", ex.Message);
            }
            return filemd5;
        }

        public static Byte[] CreateMD5(Byte[] data)
        {

            using (var md5 = MD5.Create())
            {
                return md5.ComputeHash(data);
            }
        }

        public static string FormatMD5(Byte[] data)
        {
            return System.BitConverter.ToString(data).Replace("-", "").ToLower();
        }

        public static List<String> GetFileNamesByDirectory(String path)
        {
            return Directory.GetFiles(path).ToList();
        }

        /// <summary>
        /// 读取文本文件。
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static String LoadText(String fileName)
        {
            fileName = fileName.Replace('\\', '/');
            return LoadTextFile(fileName);
        }

        /// <summary>
        /// 读取数据文件。
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static byte[] LoadBytes(String fileName)
        {
            fileName = fileName.Replace('\\', '/');
            return LoadByteFile(fileName);
        }

        public static bool IsFileExist(String fileName)
        {
            fileName = fileName.Replace('\\', '/');
            return File.Exists(fileName);
        }

        /// <summary>
        /// 读取数据文件。
        /// </summary>
        /// <param name="fileFullNames"></param>
        /// <returns></returns>
        public static List<KeyValuePair<string, byte[]>> LoadFiles(List<KeyValuePair<string, string>> fileFullNames)
        {
            return LoadLocalFiles(fileFullNames);
        }

        private static List<KeyValuePair<string, byte[]>> LoadLocalFiles(List<KeyValuePair<string, string>> fileFullNames)
        {
            var result = new List<KeyValuePair<string, byte[]>>();
            foreach (var item in fileFullNames)
            {
                var data = LoadByteFile(item.Value);
                result.Add(new KeyValuePair<string, byte[]>(item.Key, data));
            }
            return result;
        }

        public static String LoadTextFile(String fileName)
        {
            if (File.Exists(fileName))
            {
                using (StreamReader sr = File.OpenText(fileName))
                {
                    return sr.ReadToEnd();
                }
            }
            else
                return String.Empty;
        }

        public static byte[] LoadByteFile(String fileName)
        {
            if (File.Exists(fileName))
                return File.ReadAllBytes(fileName);
            else
                return null;
        }

        public static string GetStreamPath(string fileName)
        {
            var Path = String.Concat(Application.streamingAssetsPath, "/", fileName);
            if (Application.platform != RuntimePlatform.Android)
                Path = String.Concat("file://", Path);
            return Path;
        }

        public static byte[] LoadByteResource(String fileName)
        {
            TextAsset binAsset = Resources.Load(fileName, typeof(TextAsset)) as TextAsset;
            var result = binAsset.bytes;
            Resources.UnloadAsset(binAsset);
            return result;
        }
    }
}
