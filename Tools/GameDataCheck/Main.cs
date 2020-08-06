using GameData;
using System;
using System.Collections.Generic;

namespace Nullspace
{
    public class MainEntry
    {
        public static Properties Config;
        public static void Main(string[] argvs)
        {
            //Config = Properties.Create("config.txt");
            //GameDataManager.SetDir(Config.GetString("xml_dir", "."), false);
            //List<MonsterGroup> monsterGroupData = MonsterGroup.Data;
            //// Dictionary<int, Dictionary<int, MonsterProperty>> monsterPropertyDatas = MonsterProperty.Data;

            //GameDataManager.ClearAllData();

            //Console.WriteLine(GameDataManager.TypeCount());

            List<List<string>> tests = new List<List<string>>();
            for (int i = 0; i < 3; ++i)
            {
                tests.Add(new List<string>() { "1;2;3",  "45,64;43"});
            }
            string str = GameDataUtils.ToString(tests);
            Console.WriteLine(str);
            Console.ReadLine();
        }
    }
}
