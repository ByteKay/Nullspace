
using System;
using System.Threading;

namespace Nullspace
{
    public class MainEntry
    {
        public static void Main(string[] argvs)
        {
            DebugUtils.SetLogAction(LogAction);
            TestSingleSequence();
            while (true)
            {
                SequenceManager.Instance.Tick();
                Thread.Sleep(16);
            }
        }

        private static void TestParallelSequence()
        {
            //SequenceParallel parallel = SequenceManager.CreateParallel();
            //parallel.PrependInterval(2);
        }

        private static void TestSingleSequence()
        {
            SequenceSingle sequence = SequenceManager.CreateSingle();
            sequence.Append(new UpdateCallback(CallbackUtils.Acquire(Test0)), 0);
            sequence.PrependInterval(1);
            sequence.Append(new UpdateCallback(CallbackUtils.Acquire(Test1, "kay")), 0);
            sequence.PrependInterval(1);
            sequence.Append(new UpdateCallback(CallbackUtils.Acquire(Test2, 1, 3)), 0);
            sequence.PrependInterval(1);
            sequence.Append(new UpdateCallback(CallbackUtils.Acquire(Test3, "yang", 2, false)), 0);
            sequence.PrependInterval(1);
            sequence.Append(new UpdateCallback(CallbackUtils.Acquire(Test4, "kayyang", 1, true, 2.0f)), 0);
            sequence.PrependInterval(1);
            sequence.OnCompletion(CallbackUtils.Acquire(OnCompletion));
        }

        public static void Test0()
        {
            DebugUtils.Log(InfoType.Info, "Test0");
        }

        public static void Test1(string a)
        {
            DebugUtils.Log(InfoType.Info, string.Format("Test1 {0} ", a));
        }

        public static void Test2(int a, int b)
        {
            DebugUtils.Log(InfoType.Info, string.Format("Test2 {0} {1}", a, b));
        }

        public static void Test3(string a, int b, bool c)
        {
            DebugUtils.Log(InfoType.Info, string.Format("Test3 {0} {1} {2}", a, b, c));
        }

        public static void Test4(string a, int b, bool c, float d)
        {
            DebugUtils.Log(InfoType.Info, string.Format("Test4 {0} {1} {2} {3}", a, b, c, d));
        }

        public static void OnCompletion()
        {
            DebugUtils.Log(InfoType.Info, string.Format("OnCompletion"));
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
            Console.WriteLine(DateTimeUtils.GetDateTimeStringHMS() +  info);
        }
    }
}
