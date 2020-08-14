
namespace Nullspace
{
    /// <summary>
    /// 每Update回调处理，直到时间用完。
    /// 不严格要求数量
    /// </summary>
    public class UpdateCallback : BehaviourCallback
    {
        internal UpdateCallback(float startTime, float duration, Callback begin = null, Callback process = null, Callback end = null) : base(startTime, duration, begin, process, end)
        {

        }
        internal UpdateCallback(Callback begin = null, Callback process = null, Callback end = null) : base( begin, process, end)
        {

        }
    }
}
