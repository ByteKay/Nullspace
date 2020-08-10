using Nullspace;
using System;
using System.Threading;

namespace Nullspace
{
    public class TestMain
    {
        public static void Main(string[] argvs)
        {
            DebugUtils.SetLogAction(LogAction);

            Properties cfg = Properties.Create("config.txt");
            string logConfig = cfg.GetString("server_config_file", "null");
            if (logConfig != null)
            {    
                Properties config = Properties.Create(logConfig);
                NetworkClient client = new NetworkClient(config);
                NetworkEventHandler.Initialize();

                while (true)
                {
                    TimerTaskQueue.Instance.Tick();
                    NetworkEventHandler.Update();
                    Thread.Sleep(100);
                }
            }
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
