using Nullspace;
using System.Threading;

namespace Nullspace
{
    public class TestMain
    {
        public static void Main(string[] argvs)
        {
            Properties cfg = Properties.Create("config.txt");
            string logConfig = cfg.GetString("server_config_file", "null");
            if (logConfig != null)
            {
                Properties config = Properties.Create(logConfig);
                NetworkClient client = new NetworkClient(config);
                while (true)
                {
                    NetworkEventHandler.Update();
                    Thread.Sleep(100);
                }
            }
        }
    }


}
