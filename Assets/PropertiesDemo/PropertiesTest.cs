using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nullspace;
using System.Text;
using System.IO;

public class PropertiesTest : MonoBehaviour
{

	// Use this for initialization
	void Start () {
        // Properties prop = Properties.Create(Application.dataPath + "/PropertiesDemo/racer.material");
        // Properties prop = Properties.Create(Application.dataPath + "/PropertiesDemo/test.txt");
        Properties prop = Properties.CreateFromContent(File.ReadAllText(Application.dataPath + "/PropertiesDemo/test.txt"));
        StringBuilder sb = new StringBuilder();
        prop.PrintAll(sb);
        File.WriteAllText(Application.dataPath + "/PropertiesDemo/test_from_content.txt", sb.ToString());
    }

    // Update is called once per frame
    void Update ()
    {
	    	
	}
}
