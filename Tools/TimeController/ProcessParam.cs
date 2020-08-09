
using UnityEngine;

namespace Nullspace
{
    /// <summary>
    /// 定时器的状态参数：这里以时长处理，暂不支持 帧数
    /// 帧数的扩展需要将时长计算转化为帧数
    /// </summary>
    public class ProcessParam
    {
        public ProcessState processState;  // 状态类别
        public float timeValue;      // 总时长 秒为单位
        public bool bNature;      // 是否为自然时长运行
        public float timeStartPoint = -1; // 开始时间戳
        public float timePausePoint = -1; // 暂停时间戳

        /// <summary>
        /// 更新数据
        /// 如果没被暂停过，则UpdateTime只会执行一次
        /// 如果被暂停过，timePausePoint不会小于0
        /// </summary>
        /// <returns></returns>
        public bool UpdateTime()
        {
            // 表示已执行
            if (timeStartPoint > 0)
            {
                // 计算已执行时长
                float timeEllappsed = 0;
                if (bNature)
                {
                    // 自然时长处理
                    // 当前时长减去执行时长，得已执行时长
                    timeEllappsed = Time.realtimeSinceStartup - timeStartPoint;
                }
                else
                {
                    // 非自然时长，即暂停的时长不考虑
                    if (timePausePoint < 0)
                    {
                        timePausePoint = Time.realtimeSinceStartup;
                        // UpdateTime 被多次执行，说明被暂停过，不应该执行到这里
                        DebugUtils.Assert(false, "wrong");
                    }
                    // 暂停的时间点减去开启的时间点，得到实际执行时长
                    timeEllappsed = timePausePoint - timeStartPoint;
                }
                // 更新剩余时长
                timeValue = timeValue - timeEllappsed;
            }
            // 更新当前执行时间起点
            timeStartPoint = DateTimeUtils.GetTimeStampSeconds();
            return timeValue > 0;
        }

    }

}
