
using System.Collections.Generic;
using UnityEngine;

namespace NullMesh
{

    public class NullTestData : INullStream
    {
        public int age;
        public string name;
        public bool isMale;
        public float money;

        public bool LoadFromStream(NullMemoryStream stream)
        {
            bool res = stream.ReadString(out name);
            res &= stream.ReadInt(out age);
            res &= stream.ReadBool(out isMale);
            res &= stream.ReadFloat(out money);
            return res;
        }

        public int SaveToStream(NullMemoryStream stream)
        {
            int size = stream.WriteString(name);
            size += stream.WriteInt(age);
            size += stream.WriteBool(isMale);
            size += stream.WriteFloat(money);
            return size;
        }

        public string GetKey()
        {
            return string.Format("{0}_{1}_{2}_{3}", name, age, isMale, money);
        }
    }

    public class NullMemoryStreamTest
    {

        // Use this for initialization
        public void Start()
        {
            string testPath = "test.bytes";
            using (NullMemoryStream stream = NullMemoryStream.WriteToFile(testPath))
            {
                List<Quaternion> test = new List<Quaternion>();
                Dictionary<int, Vector3> map = new Dictionary<int, Vector3>();
                for (int i = 0; i < 100; ++i)
                {
                    test.Add(Quaternion.identity);
                    map.Add(i, Vector3.zero);
                }
                Dictionary<int, NullTestData> stds = new Dictionary<int, NullTestData>();
                stds.Add(0, new NullTestData() { name = "test1", age = 12, isMale = false, money = 4.6f });
                stds.Add(1, new NullTestData() { name = "test2", age = 8, isMale = true, money = 48f });
                stream.WriteList(test, false);
                stream.WriteMap(map, false);
                stream.WriteMap(stds, false);
            }

            using (NullMemoryStream stream = NullMemoryStream.ReadFromFile(testPath))
            {
                List<Quaternion> test;
                Dictionary<int, Vector3> map;
                Dictionary<int, NullTestData> stds;
                stream.ReadList(out test);
                stream.ReadMap(out map);
                stream.ReadMap(out stds);
                Debug.Log("test: " + test.Count + " " + test[0] + " " + test[test.Count - 1]);
                Debug.Log("map: " + map.Count + " " + map[0] + " " + map[map.Count - 1]);
                Debug.Log("stds: " + stds.Count + " " + stds[0].GetKey() + " " + stds[1].GetKey());
            }
        }

    }
}
