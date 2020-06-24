using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Nullspace
{
    public class XmlDataTest : MonoBehaviour
    {
        private void OnGUI()
        {
            if (GUILayout.Button("Save XML"))
            {
                SaveXMl();
            }

            if (GUILayout.Button("Load XML"))
            {
                XmlDataLoader.Instance.InitAndLoad(XmlFileNameDefine.NAMESPACE, XmlFileNameDefine.SUFFIX_FLAG);
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
            XmlFileUtils.SaveXML(string.Format("{0}/{1}/{2}{3}", Application.dataPath, XmlFileNameDefine.DIRECTORY, XmlFileNameDefine.TestPerson, XmlFileNameDefine.SUFFIX_FLAG), datas);
        }
    }
}
