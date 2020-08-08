
using GameData;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Nullspace
{
    public class MainEntry
    {
        public static Properties Config;
        public static void Main(string[] argvs)
        {
            Config = Properties.Create("config.txt");
            GameDataManager.SetDir(Config.GetString("xml_dir", "."), false, true);
            DebugUtils.SetLogAction(LogAction);

            LogAction(InfoType.Error, "Check Start ...");
            GameDataManager.InitAllData();
            GameDataManager.ClearAllData();
            LogAction(InfoType.Error, "Check End ...");
            Console.ReadLine();
        }

        private static void LogAction(InfoType infoType, string info)
        {
            switch (infoType)
            {
                case InfoType.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case InfoType.Info:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case InfoType.Warning:
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
            }
            Console.WriteLine(info);
        }
    }
}
