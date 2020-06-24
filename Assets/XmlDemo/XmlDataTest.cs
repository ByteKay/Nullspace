using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Nullspace
{

    [XmlData(XmlDataTest.FileName, "test")]
    public class TestXmlData : XmlData<TestXmlData>
    {
        public int Age { get; set; }
        public string Name { get; set; }
    }

    public class XmlDataTest : MonoBehaviour
    {
        public const string FileName = "test_person";

        private void OnGUI()
        {
            if (GUILayout.Button("Save XML"))
            {
                SaveXMl();
            }

            if (GUILayout.Button("Load XML"))
            {
                XmlDataLoader.Instance.InitAndLoad("Nullspace", ResourceCachePools.SUFFIX_FLAG);
                DebugUtils.Info("XmlDataTest", TestXmlData.DataMap.Count);
            }
        }


        private void SaveXMl()
        {
            List<TestXmlData> datas = new List<TestXmlData>();
            datas.Add(new TestXmlData() { Id = 1, Age = 10, Name = "kay1"});
            datas.Add(new TestXmlData() { Id = 2, Age = 9, Name = "kay2" });
            datas.Add(new TestXmlData() { Id = 3, Age = 10, Name = "kay3" });
            datas.Add(new TestXmlData() { Id = 4, Age = 12, Name = "kay4" });
            datas.Add(new TestXmlData() { Id = 5, Age = 13, Name = "kay5" });
            XmlFileUtils.SaveXML(Application.dataPath + "/XmlData/test_person" + ResourceCachePools.SUFFIX_FLAG, datas);
        }
    }
}
