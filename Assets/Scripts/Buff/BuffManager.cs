
using System.Collections.Generic;

namespace Nullspace
{
    public class BuffManager
    {
        private static List<int> CACHE_MOVE = new List<int>();

        private Dictionary<int, Buff> mBuffMap;
        private IBuffTarget mBuffTarget;
        private int mState;
        private bool bLocked;
        private int BuffCount { get { return mBuffMap.Count; } }
        
        public BuffManager(IBuffTarget target)
        {
            mBuffTarget = target;
            mBuffMap = new Dictionary<int, Buff>();
            mState = BuffType.NONE;
            bLocked = false;
        }

        public bool AddTimeBuff(int buffType, float duration, bool replaced = true, float accelerate = 0)
        {
            Buff buff = null;
            switch (buffType)
            {
                case BuffType.PAUSE_MOVE:
                case BuffType.ACCELERATA_MOVE:
                case BuffType.DECELERATA_MOVE:
                case BuffType.UNLOCK_MOVE:
                    buff = new BuffAccelerate(buffType, mBuffTarget, duration, accelerate);// 执行一次
                    break;
                case BuffType.FREEZE_MOVE:
                    buff = new BuffFreeze(buffType, mBuffTarget, duration);
                    break;
                default:
                    break;
            }
            if (buff != null)
            {
                return AddBuff(buff, replaced);
            }
            return false;
        }

        public void Update(float delta)
        {
            bLocked = true;
            // 更新Buff
            CACHE_MOVE.Clear();
            foreach (Buff buff in mBuffMap.Values)
            {
                buff.Update(delta);
                if (buff.IsEnd())
                {
                    CACHE_MOVE.Add(buff.Type);
                }
            }
            // 删除Buff
            foreach (int type in CACHE_MOVE)
            {
                if (mBuffMap.ContainsKey(type))
                {
                    mState &= ~type;
                    mBuffMap.Remove(type);
                }
            }
            bLocked = false;
        }

        /// <summary>
        /// Update 执行过程中 不要嵌套调用 ClearBuff
        /// </summary>
        public void ClearBuff()
        {
            if (bLocked)
            {
                UnityEngine.Debug.LogError("ClearBuff locked");
            }
            mState = BuffType.NONE;
            mBuffMap.Clear();
        }

        /// <summary>
        /// Update 执行过程中 不要嵌套调用 AddBuff
        /// </summary>
        public bool AddBuff(Buff buff, bool replaced)
        {
            if (bLocked)
            {
                UnityEngine.Debug.LogError("AddBuff locked");
            }
            if (mBuffMap.ContainsKey(buff.Type)) // 已经存在
            {
                if (replaced || mBuffMap[buff.Type].IsEnd()) // 替换
                {
                    if (mBuffMap[buff.Type].IsActive)
                    {
                        mBuffMap[buff.Type].End(); // 先还原
                    }
                    mBuffMap[buff.Type] = buff;
                }
                else
                {
                    mBuffMap[buff.Type].Merge(buff); // 合并
                }
                return true;
            }
            else
            {
                if (CanAdd(buff))//能添加
                {
                    BreakBuffs(buff);//打断互斥
                    mState |= buff.Type;//加状态位
                    mBuffMap.Add(buff.Type, buff);//加buff
                    return true;
                }
            }
            return false;
        }

        private bool CanAdd(Buff buff)
        {
            int state = mState;//拷贝
            while (state > 0)
            {
                int n = state & (state - 1);// 消去最后一个1
                int r = state ^ n;// 异或处理：同则为0，不同为1.取最后为1的位
                if (BuffType.BLACK_TYPE.ContainsKey(r)) // 检查黑名单
                {
                    if ((BuffType.BLACK_TYPE[r] & buff.Type) != 0) // 在黑名单中
                    {
                        return false;// 不能添加
                    }
                }
                state = n;// 继续检查
            }
            return true;
        }

        /// <summary>
        /// Update 执行过程中 不要嵌套调用 BreakBuffs
        /// </summary>
        private void BreakBuffs(Buff buff)
        {
            if (BuffType.BLACK_TYPE.ContainsKey(buff.Type))
            {
                int breakable = BuffType.BLACK_TYPE[buff.Type];
                int state = mState;//拷贝
                while ((breakable & state) != 0)
                {
                    int n = state & (state - 1);// 消去最后一个1
                    int r = state ^ n;// 异或处理：同则为0，不同为1.取最后为1的位   
                    if ((breakable & r) != 0)// 当前状态存在被打断的Buff
                    {
                        if (mBuffMap.ContainsKey(r))// 校验状态
                        {
                            if (bLocked) // 在Update中执行，不能出现这样子的情况
                            {
                                UnityEngine.Debug.LogError("BreakBuffs locked");
                            }
                            if (mBuffMap[r].IsActive)
                            {
                                mBuffMap[r].End(); // 还原
                            }
                            mState &= ~r;// 移除状态
                            mBuffMap.Remove(r); // 删除Buff
                        }
                        else
                        {
                            mState &= ~r;
                            UnityEngine.Debug.LogError("something wrong mState: " + r);
                        }
                    }
                    state = n;// 继续检查
                }
            }
        }

    }
}
