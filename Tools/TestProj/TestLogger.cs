
using System.Threading;

namespace Nullspace
{
    public class TestLogger
    {
        static Logger logger = new Logger();

        public static void Test()
        {
            Properties cfg = Properties.Create("config.txt");
            string logConfig = cfg.GetString("log_config_file", "null");
            if (logConfig != null)
            {
                LoggerConfig config = LoggerConfig.Create(logConfig);
                logger.Initialize(config);
                while (true)
                {
                    logger.LogInfo("test");
                    Thread.Sleep(100);
                }
            }
        }
    }
}
