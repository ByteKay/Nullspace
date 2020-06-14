using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fishing
{
    /// <summary>
    /// Buff 对象接口
    /// </summary>
    public interface IBuffTarget
    {
        /// <summary>
        /// 更改速度加成
        /// </summary>
        /// <param name="times">加成百分比，带正负（正为加速；负为减速；负值很大时，可表现为暂停）</param>
        void SpeedTimes(float times);

        /// <summary>
        /// 控制是否能移动
        /// </summary>
        /// <param name="canMove">true为能移动；false为不能移动</param>
        void EnableMove(bool canMove);

        /// <summary>
        /// 冰冻开关
        /// </summary>
        /// <param name="toggle">true为开启冰冻；false为关闭</param>
        void Freeze(bool toggle);
    }
}
