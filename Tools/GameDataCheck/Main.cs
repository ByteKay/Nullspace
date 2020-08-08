
using System;

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

            LogAction("Check Start ...");
            GameDataManager.InitAllData();
            GameDataManager.ClearAllData();
            LogAction("Check End ...");
            Console.ReadLine();
        }

        private static void LogAction(string info)
        {
            Console.WriteLine(info);
        }
    }
}
