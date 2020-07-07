using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using UnityEngine;

namespace Nullspace
{
    public class AlgorithmFileUtils
    {

        public static void Save(byte[,] skeletonResult, string fileName, bool change = true)
        {
            int w = skeletonResult.GetLength(0);
            int h = skeletonResult.GetLength(1);
            System.IO.StreamWriter write = new System.IO.StreamWriter(fileName, false, Encoding.UTF8);
            for (int i = 0; i < w; ++i)
            {
                string tmpStr = "";
                for (int j = 0; j < h; ++j)
                {
                    int tmp = skeletonResult[i, j];
                    if (change)
                    {
                        tmp = tmp > 10 ? 1 : 0;
                    }
                    tmpStr = string.Format("{0} {1}", tmpStr, tmp);
                }
                write.WriteLine(tmpStr);
            }
            write.Close();
        }

        public static void ParseBytes(byte[] all, out List<Vector3> vertList, out List<int> faceindices)
        {
            vertList = new List<Vector3>();
            faceindices = new List<int>();
            if (all != null)
            {
                int idx = 0;
                int verNum = BitConverter.ToInt32(all, idx);
                idx += 4;
                for (int i = 0; i < verNum; ++i)
                {
                    float v1 = BitConverter.ToSingle(all, idx);
                    idx += 4;
                    float v2 = BitConverter.ToSingle(all, idx);
                    idx += 4;
                    float v3 = BitConverter.ToSingle(all, idx);
                    idx += 4;
                    vertList.Add(new Vector3(v1, v2, v3));
                }
                int faceNum = BitConverter.ToInt32(all, idx);
                idx += 4;
                for (int i = 0; i < faceNum; ++i)
                {
                    faceindices.Add(BitConverter.ToInt32(all, idx));
                    idx += 4;
                }
            }
        }

        public static void LoadFromFile(string filePath, out List<Vector3> vertList, out List<int> faceindices)
        {
            FileStream stream = new FileStream(filePath, FileMode.Open);
            byte[] all = null;
            if (stream.CanRead)
            {
                int len = (int)stream.Length;
                all = new byte[len];
                stream.Read(all, 0, len);
            }
            stream.Close();
            vertList = new List<Vector3>();
            faceindices = new List<int>();
            if (all != null)
            {
                int idx = 0;
                int verNum = BitConverter.ToInt32(all, idx);
                idx += 4;
                for (int i = 0; i < verNum; ++i)
                {
                    float v1 = BitConverter.ToSingle(all, idx);
                    idx += 4;
                    float v2 = BitConverter.ToSingle(all, idx);
                    idx += 4;
                    float v3 = BitConverter.ToSingle(all, idx);
                    idx += 4;
                    vertList.Add(new Vector3(v1, v2, v3));
                }
                int faceNum = BitConverter.ToInt32(all, idx);
                idx += 4;
                for (int i = 0; i < faceNum; ++i)
                {
                    faceindices.Add(BitConverter.ToInt32(all, idx));
                    idx += 4;
                }
            }
        }
    }
}
