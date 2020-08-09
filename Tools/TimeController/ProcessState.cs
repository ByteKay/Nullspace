
namespace Nullspace
{
    /// <summary>
    /// 总设计：
    /// 1. ProcessState 对发射流程的状态定位。
    /// </summary>
    public enum ProcessState
    {
        DELAY,
        AUTO,
        COOLDOWN,
        INTERVAL,
        PROCESS,
        ///////////////////////////////////////////////////////
        START,
        PAUSE,
        RESUME,
        STOP,
    }

}
