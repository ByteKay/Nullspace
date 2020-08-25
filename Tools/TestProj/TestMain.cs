using System;

namespace Nullspace
{
    public class TestMain
    {
        private static void InitializeLogger(Properties cfg)
        {
            string logConfig = cfg.GetString("log_config_file", "null");
            if (logConfig != null)
            {
                Properties config = Properties.Create(logConfig);
                Logger.Instance.Initialize(config);
            }
        }

        private static void InitializeClient(Properties cfg)
        {
            string serverConfig = cfg.GetString("server_config_file", "null");
            if (serverConfig != null)
            {
                Properties config = Properties.Create(serverConfig);
                NetworkClient.Instance.Initialize(config);
            }
        }

        private static void Initialize()
        {
            Properties cfg = Properties.Create("config.txt");
            InitializeLogger(cfg);
            InitializeClient(cfg);
            TimerTaskQueue.Instance.Reset();
            FrameTimerTaskHeap.Instance.Reset();
            SequenceManager.Instance.Clear();
            IntEventDispatcher.Cleanup();
            NetworkEventHandler.Initialize();
        }

        private static void Destroy()
        {
            NetworkClient.Instance.Stop();
            NetworkEventHandler.Clear();
            SequenceManager.Instance.Clear();
            TimerTaskQueue.Instance.Reset();
            FrameTimerTaskHeap.Instance.Reset();
            IntEventDispatcher.Cleanup();
            Logger.Instance.Stop();
        }

        private static void Update()
        {
            // 协议
            NetworkEventHandler.Update();
            TimerTaskQueue.Instance.Tick();
            FrameTimerTaskHeap.Instance.Tick();
            SequenceManager.Instance.Tick();
        }

        public static void Main(string[] argvs)
        {
            DebugUtils.SetLogAction(LogAction);
            TestSequence.Test();
            //TestLogger.Test();
            //TestClient.Test();
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
            Console.WriteLine(DateTimeUtils.GetDateTimeStringHMS() + info);
        }
    }
}
