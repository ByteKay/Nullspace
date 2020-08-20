
using System.Threading;

namespace Nullspace
{
    public class TestClient
    {
        public static void Test()
        {
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

    }
}
