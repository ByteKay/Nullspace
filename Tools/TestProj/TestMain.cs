using System;

namespace Nullspace
{
    public class TestMain
    {
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
