
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
            TestParallelSequence();
            while (true)
            {
                SequenceManager.Instance.Tick();
                Thread.Sleep(16);
            }
        }


        private static void TestParallelSequence()
        {
            SequenceParallel sequence = SequenceManager.CreateParallel();
            sequence.OnCompletion(CallbackUtils.Acquire(OnCompletion));
            // 整体延迟 2 秒
            sequence.PrependInterval(2);
            // 只有开始和结束，持续1秒
            sequence.Append(CallbackUtils.Acquire(TestBegin), CallbackUtils.Acquire(TestEnd), 0);
            // 每帧调用且持续一秒
            sequence.Append(CallbackUtils.Acquire(Test1Process, "kay"), 1);
            // 执行开始和结束, 每帧调用且持续一秒
            sequence.Append(CallbackUtils.Acquire(Test2Begin, 1, 3), CallbackUtils.Acquire(Test2Process, 1, 3), CallbackUtils.Acquire(Test2End, 1, 3), 1);
            // 持续6秒，每两秒执行一次 Process
            sequence.AppendFrame(CallbackUtils.Acquire(Test3, "yang", 2, false), 6, 2.0f, true);
            // 持续6秒，执行10次 Process
            float endTime = sequence.AppendFrame(CallbackUtils.Acquire(Test4Begin), CallbackUtils.Acquire(Test4), CallbackUtils.Acquire(Test4End), 6, 10, true);
            // Sequence 结束后 回调处理
            sequence.InsertFrame(0, CallbackUtils.Acquire(InsertTestBegin), CallbackUtils.Acquire(InsertTest), CallbackUtils.Acquire(InsertTestEnd), endTime, 20, true);
        }

        private static void TestSingleSequence()
        {
            SequenceLinkedList sequence = SequenceManager.CreateSingle();
            // 整体延迟 4 秒
            sequence.AppendInterval(4);
            // 只有开始和结束，持续1秒
            sequence.Append(CallbackUtils.Acquire(TestBegin), CallbackUtils.Acquire(TestEnd), 0);
            // 每帧调用且持续一秒
            sequence.Append(CallbackUtils.Acquire(Test1Process, "kay"), 1);
            // 执行开始和结束, 每帧调用且持续一秒
            sequence.Append(CallbackUtils.Acquire(Test2Begin, 1, 3), CallbackUtils.Acquire(Test2Process, 1, 3), CallbackUtils.Acquire(Test2End, 1, 3), 1);
            // 持续6秒，每两秒执行一次 Process
            sequence.AppendFrame(CallbackUtils.Acquire(Test3, "yang", 2, false), 6, 2.0f, true);
            // 持续6秒，执行10次 Process
            sequence.AppendFrame(CallbackUtils.Acquire(Test4Begin), CallbackUtils.Acquire(Test4), CallbackUtils.Acquire(Test4End), 6, 10, true);
            // Sequence 结束后 回调处理
            sequence.OnCompletion(CallbackUtils.Acquire(OnCompletion));
        }

        private static int mCounter = 0;
        public static void InsertTestBegin()
        {
            DebugUtils.Log(InfoType.Info, "InsertTestBegin");
        }

        public static void InsertTest()
        {
            mCounter++;
            DebugUtils.Log(InfoType.Info, "InsertTest : " + mCounter);
        }

        public static void InsertTestEnd()
        {
            DebugUtils.Log(InfoType.Info, "InsertTestEnd");
        }

        public static void TestBegin()
        {
            DebugUtils.Log(InfoType.Info, "TestBegin");
        }
        public static void TestEnd()
        {
            DebugUtils.Log(InfoType.Info, "TestEnd");
        }
        public static void Test1Process(string a)
        {
            DebugUtils.Log(InfoType.Info, string.Format("Test1Process {0} ", a));
        }

        public static void Test2Begin(int a, int b)
        {
            DebugUtils.Log(InfoType.Info, string.Format("Test2Begin {0} {1}", a, b));
        }

        public static void Test2Process(int a, int b)
        {
            DebugUtils.Log(InfoType.Info, string.Format("Test2Process {0} {1}", a, b));
        }

        public static void Test2End(int a, int b)
        {
            DebugUtils.Log(InfoType.Info, string.Format("Test2End {0} {1}", a, b));
        }

        public static void Test3(string a, int b, bool c)
        {
            DebugUtils.Log(InfoType.Info, string.Format("Test3 {0} {1} {2}", a, b, c));
        }
        public static void Test4Begin()
        {
            DebugUtils.Log(InfoType.Info, string.Format("Test4Begin "));
        }
        public static void Test4()
        {
            DebugUtils.Log(InfoType.Info, string.Format("Test4 "));
        }

        public static void Test4End()
        {
            DebugUtils.Log(InfoType.Info, string.Format("Test4End "));
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
