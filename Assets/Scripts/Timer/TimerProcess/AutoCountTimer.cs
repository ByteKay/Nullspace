using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nullspace
{
    public enum CommandTimerType
    {
        Default
    }

    /// <summary>
    /// 自动发射规则 BulletType.Default
    /// </summary>
    public class AutoCountTimer : TimerCount<CommandTimerType>
    {

        public override int OneProcess(ProcessTimer<CommandTimerType, int> target, CommandTimerType msg, int total, int current)
        {
            DebugUtils.Info("AutoCountTimer", "AutoCountTimer OneProcess");
            return 0;
        }

        /// <summary>
        /// 默认结束处理
        /// </summary>
        /// <param name="target"></param>
        /// <param name="total"></param>
        /// <param name="current"></param>
        public override void FinishedProcess(ProcessTimer<CommandTimerType, int> target, int total, int current)
        {
            DebugUtils.Info("AutoCountTimer", "AutoCountTimer FinishedProcess");
            target.SetCount(1, 0);
            target.AddTarget(CommandTimerType.Default);
            target.Produce(ProcessState.COOLDOWN, true, 3);
            target.Produce(ProcessState.PROCESS);
            target.Control(ProcessState.START);
        }
    }
}
