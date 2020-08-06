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
            DateTime dt = DateTimeUtils.GetTime("2015/02/14 14:1:50");
            string strDt = DateTimeUtils.FormatTimeHMS(dt);
            Console.WriteLine(strDt);
            Console.ReadLine();
        }
    }
}
