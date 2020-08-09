
namespace Nullspace
{
    /// <summary>
    /// 基于次数
    /// </summary>
    /// <typeparam name="TargetType"></typeparam>
    public class CountTimeProcess<CommandTimerType> : ProcessTimer<CommandTimerType, int>
    {
        protected override bool IsFinished()
        {
            return CurValue >= TotalValue;
        }

        protected override void PostFire()
        {
            CurValue += 1;
        }

    }
}
