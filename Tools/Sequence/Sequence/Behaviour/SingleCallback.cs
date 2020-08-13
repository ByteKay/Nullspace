
namespace Nullspace
{
    /// <summary>
    /// 每帧回调
    /// </summary>
    public class SingleCallback : BehaviourCallback
    {
        internal SingleCallback(float startTime, float duration, AbstractCallback begin = null, AbstractCallback process = null, AbstractCallback end = null) : base(startTime, duration, begin, process, end)
        {

        }
        internal SingleCallback(AbstractCallback begin = null, AbstractCallback process = null, AbstractCallback end = null) : base( begin, process, end)
        {

        }

        // 行为所属 sequence
        internal SequenceSingle Single { get; set; }
        // 行为执行完毕后，紧接着执行下一个行为
        internal override void End()
        {
            base.End();
            if (Single != null)
            {
                // 执行下一个行为
                Single.Next();
            }
        }
    }
}
