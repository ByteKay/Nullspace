
namespace Nullspace
{
    /// <summary>
    /// 按秒调用
    /// </summary>
    public class BehaviourSecondTimeCallback : BehaviourTimeCallback
    {
        private int CurrentSeconds = 0;
        public BehaviourSecondTimeCallback(AbstractCallback process = null, AbstractCallback begin = null, AbstractCallback end = null) : base(begin, process, end)
        {

        }

        public override void Reset()
        {
            base.Reset();
            CurrentSeconds = 0;
        }

        public override void Process()
        {
            int elappsedSeconds = (int)TimeElappsed - (int)StartTime;
            if (elappsedSeconds > CurrentSeconds)
            {
                CurrentSeconds = elappsedSeconds;
                base.Process();
            }
        }
    }
}
