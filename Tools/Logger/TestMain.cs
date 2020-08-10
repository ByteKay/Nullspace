using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Nullspace
{
    public class TestMain
    {
        static Logger logger = new Logger();

        public static void Main(string[] argvs)
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
