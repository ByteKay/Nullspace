using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nullspace;
using System.Text;
using System.IO;
using NullMesh;

public class PropertiesTest : MonoBehaviour
{

	// Use this for initialization
	void Start () {
        //Properties prop = Properties.Create(Application.dataPath + "/PropertiesDemo/racer.material");
        //StringBuilder sb = new StringBuilder();
        //prop.PrintAll(sb);
        //File.WriteAllText(Application.dataPath + "/PropertiesDemo/racer.txt", sb.ToString());

        NullMemoryStream stream = NullMemoryStream.ReadTextFromFile(Application.dataPath + "/PropertiesDemo/racer.material");
        string line = stream.ReadLine();
        DebugUtils.Info("Start", line);

        line = stream.ReadLine();
        DebugUtils.Info("Start", line);
    }

    // Update is called once per frame
    void Update ()
    {
	    	
	}
}
