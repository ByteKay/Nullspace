using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nullspace
{
    /// <summary>
    /// 流定时器
    /// </summary>
    /// <typeparam name="T">发射信息</typeparam>
    /// <typeparam name="U">整数或浮点</typeparam>
    public abstract partial class ProcessTimer<TargetType, ValueType>
    {
        /// <summary>
        /// 默认回调函数
        /// </summary>
        /// <param name="target">定时器对象</param>
        /// <param name="msg"></param>
        /// <param name="total"></param>
        /// <param name="current"></param>
        /// <returns></returns>
        public virtual int OneProcess(ProcessTimer<TargetType, ValueType> target, TargetType msg, ValueType total, ValueType current)
        {
            target.Produce(ProcessState.INTERVAL, false, 3);
            return 0;
        }

        /// <summary>
        /// 默认结束处理
        /// </summary>
        /// <param name="target"></param>
        /// <param name="total"></param>
        /// <param name="current"></param>
        public virtual void FinishedProcess(ProcessTimer<TargetType, ValueType> target, ValueType total, ValueType current)
        {
            // todo
        }

    }

    /// <summary>
    /// 流定时器
    /// </summary>
    /// <typeparam name="T">发射信息</typeparam>
    /// <typeparam name="U">整数或浮点</typeparam>
    public abstract partial class ProcessTimer<TargetType, ValueType>
    {
        public bool IsState(ProcessState state)
        {
            return mCurrentProcessParam != null && mCurrentProcessParam.processState == state;
        }

        public bool IsInInterval()
        {
            return IsState(ProcessState.INTERVAL);
        }

        public bool IsNotInInterval()
        {
            return !IsState(ProcessState.INTERVAL);
        }

        public bool IsInAuto()
        {
            return IsState(ProcessState.AUTO);
        }

        public bool IsNotInAuto()
        {
            return !IsState(ProcessState.AUTO);
        }

        public bool IsInDelay()
        {
            return IsState(ProcessState.DELAY);
        }

        public bool IsNotInDelay()
        {
            return !IsState(ProcessState.DELAY);
        }

        public bool IsInCD()
        {
            return IsState(ProcessState.COOLDOWN);
        }

        public bool IsNotInCD()
        {
            return !IsState(ProcessState.COOLDOWN);
        }

        public bool IsPlaying()
        {
            return isActive;
        }

        public bool IsPause()
        {
            return IsState(ProcessState.PAUSE);
        }
    }

    /// <summary>
    /// 流定时器
    /// </summary>
    /// <typeparam name="T">发射信息</typeparam>
    /// <typeparam name="U">整数或浮点</typeparam>
    public abstract partial class ProcessTimer<TargetType, ValueType>
    {
        /// <summary>
        /// 发射成功后，处理。
        /// 针对时间，加上 dt
        /// 针对数量，加上 1
        /// </summary>
        protected abstract void PostFire();

        /// <summary>
        /// 结束判断。由于int和float 泛化问题，这里需子类实现
        /// </summary>
        /// <returns></returns>
        protected abstract bool IsFinished();

        /// <summary>
        /// 添加发射信息
        /// </summary>
        /// <param name="msg"></param>
        public virtual void AddTarget(TargetType msg)
        {
            // 如果生产时，处于cd期间并且控制为忽略，则不添加处理
            if (ignoreWhenCD && IsInCD())
            {
                DebugUtils.Log( InfoType.Info, "AutoCountTimer refuse ");
                return;
            }
            mTargetMessages.AddLast(msg);
        }
    }

    /// <summary>
    /// 流定时器
    /// </summary>
    /// <typeparam name="T">发射信息</typeparam>
    /// <typeparam name="U">整数或浮点</typeparam>
    public abstract partial class ProcessTimer<TargetType, ValueType>
    {
        // 发射次数或时长
        public ValueType TotalValue { get; set; }
        // 当前运行时长或次数
        public ValueType CurValue { get; set; }
        // 有效性. Start 和 Stop
        public bool isActive { get; set; }
        // cd 期间是否能新增 发射请求
        public bool ignoreWhenCD { get; set; }
        // 发射一次回调
        public Func<ProcessTimer<TargetType, ValueType>, TargetType, ValueType, ValueType, int> OneProcessAction { get; set; }
        // 发射结束回调
        public Action<ProcessTimer<TargetType, ValueType>, ValueType, ValueType> FinishedProcessAction { get; set; }
        // 发射控制参数消息
        protected LinkedList<ProcessParam> mStateMessages;
        // 发射物体消息
        protected LinkedList<TargetType> mTargetMessages;
        // 当前控制状态
        protected ProcessParam mCurrentProcessParam;

        /// <summary>
        /// 初始化数据
        /// </summary>
        public virtual void Awake()
        {
            mTargetMessages = new LinkedList<TargetType>();
            mStateMessages = new LinkedList<ProcessParam>();
            mCurrentProcessParam = null;
            isActive = false;
            ignoreWhenCD = true;
            SetAction(OneProcess, FinishedProcess);
        }


        public virtual void SetCount(ValueType total, ValueType current)
        {
            TotalValue = total;
            CurValue = current;
        }

        protected virtual void SetAction(Func<ProcessTimer<TargetType, ValueType>, TargetType, ValueType, ValueType, int> oneProcess, Action<ProcessTimer<TargetType, ValueType>, ValueType, ValueType> finishProcess)
        {
            OneProcessAction = oneProcess;
            FinishedProcessAction = finishProcess;
        }

        /// <summary>
        /// 生产数据
        /// </summary>
        /// <param name="state"></param>
        /// <param name="time"></param>
        /// <param name="bLast"></param>
        /// <param name="immediateSequence"></param>
        public virtual void Produce(ProcessState state, bool isNature = true, float time = 1, bool bLast = true, bool immediateSequence = false)
        {
            Produce(new ProcessParam() { processState = state, timeValue = time, bNature = isNature, timeStartPoint = -1, timePausePoint = -1 }, bLast, immediateSequence);
        }

        /// <summary>
        /// 生产数据
        /// </summary>
        /// <param name="param">处理参数</param>
        /// <param name="bLast">追加队尾还是队首</param>
        /// <param name="immediateSequence">立即消费执行</param>
        public virtual void Produce(ProcessParam param, bool bLast = true, bool immediateSequence = false)
        {
            if (bLast)
            {
                mStateMessages.AddLast(param);
            }
            else
            {
                mStateMessages.AddFirst(param);
            }

            if (immediateSequence)
            {
                Consume();
            }
        }

        /// <summary>
        /// 全局控制 开始，暂停，恢复和停止
        /// </summary>
        /// <param name="state"></param>
        /// <param name="time">默认值给1，表明需要执行</param>
        public virtual void Control(ProcessState state)
        {
            ProcessParam stateParam = new ProcessParam() { processState = state, timeValue = 1 };
            bool bWrongState = true;
            switch (state)
            {
                case ProcessState.START:
                    StartTimer();
                    bWrongState = false;
                    break;
                case ProcessState.PAUSE:
                    Pause(stateParam);
                    bWrongState = false;
                    break;
                case ProcessState.RESUME:
                    Resume();
                    bWrongState = false;
                    break;
                case ProcessState.STOP:
                    Stop();
                    bWrongState = false;
                    break;
            }

            if (bWrongState)
            {
                DebugUtils.Log(InfoType.Error, "wrong state " + state);
            }
        }

        /// <summary>
        /// 开始执行流程
        /// 必须是当前为开始过，否则不重复执行
        /// </summary>
        protected virtual void StartTimer()
        {
            if (!isActive)
            {
                isActive = true;
                Consume();
            }
        }

        /// <summary>
        /// 暂停。这个暂停会给一个时长
        /// 如果是永久，时长传个很大值即可
        /// 如果是暂停一会儿，传具体的时长即可
        /// </summary>
        /// <param name="pause"></param>
        protected virtual void Pause(ProcessParam pause)
        {
            // 将现在的处理重新放回到队列
            if (mCurrentProcessParam != null)
            {
                // 记录当前暂停时间戳
                mCurrentProcessParam.timePausePoint = Time.realtimeSinceStartup;
                // 将当前进一步执行
                Produce(mCurrentProcessParam, false, false);
            }
            // 关闭，并保存 pause 为当前状态。
            // 注意  isActive 并不重置
            HandleInvoke(false, pause);
        }

        /// <summary>
        /// 恢复执行流程。条件：当前状态必须为 PAUSE
        /// </summary>
        protected virtual void Resume()
        {
            if (IsPause())
            {
                // 清理当前 pause 状态
                HandleInvoke(false, null);
                // 重新消费
                Consume();
            }
        }

        /// <summary>
        /// 停止，清空数据和状态重置
        /// </summary>
        public virtual void Stop()
        {
            HandleInvoke(false, null);
            isActive = false;
            mStateMessages.Clear();
            mTargetMessages.Clear();
        }

        /// <summary>
        /// 消费
        /// 如果开启，则处理；否则不处理
        /// 若当前状态不为null，或者 无状态数据需要执行，则不处理
        /// 否则，拿出下一个需要执行的状态执行。
        /// 如果拿出的状态无效，则继续拿下一个消费
        /// </summary>
        protected virtual void Consume()
        {
            if (isActive)
            {
                while (mCurrentProcessParam == null && mStateMessages.Count > 0)
                {
                    ProcessParam first = mStateMessages.First.Value;
                    mStateMessages.RemoveFirst();
                    if (first.UpdateTime())
                    {
                        HandleStateOn(first);
                    }
                }
            }
        }


        /// <summary>
        /// 这里将时间定义为 ： 延迟，冷却，周期
        /// 将具体的操作定义为：Process
        /// </summary>
        protected virtual void HandleStateOn(ProcessParam first)
        {
            if (first != null)
            {
                switch (first.processState)
                {
                    case ProcessState.COOLDOWN:
                    case ProcessState.DELAY:
                    case ProcessState.INTERVAL:
                    case ProcessState.AUTO:
                        HandleInvoke(true, first);
                        break;
                    case ProcessState.PROCESS:
                        Process();
                        break;
                }
            }
        }

        /// <summary>
        /// 如果当前状态为指定类型，则取消当前，执行下一个
        /// 操控的是 mStateMessages
        /// </summary>
        protected virtual void ForceNext(ProcessState curState)
        {
            if (mCurrentProcessParam == null || IsState(curState))
            {
                HandleInvoke(false, null);
                Consume();
            }
        }

        /// <summary>
        /// 一次Fire后，回调进行 添加控制逻辑
        /// </summary>
        protected virtual void Process()
        {
            int retCode = 0;
            if (mTargetMessages.Count > 0)
            {
                TargetType msg = mTargetMessages.First.Value;
                mTargetMessages.RemoveFirst();
                if (msg != null && OneProcessAction != null)
                {
                    // 返回值
                    retCode = OneProcessAction(this, msg, TotalValue, CurValue);
                }
            }
            if (retCode != 0)
            {
                DebugUtils.Log( InfoType.Info, string.Format("Process (TotalValue, CurValue) = ({1}, {2})", TotalValue, CurValue));
                return;
            }
            PostFire();
            CheckFinished();
        }

        /// <summary>
        /// 检测是否结束，并执行结束回调
        /// </summary>
        protected void CheckFinished()
        {
            if (IsFinished())
            {
                // 先停止处理
                Stop();
                if (FinishedProcessAction != null)
                {
                    FinishedProcessAction(this, TotalValue, CurValue);
                }
                else
                {
                    DebugUtils.Log( InfoType.Info, string.Format("CheckFinished Finished: (TotalValue, CurValue) = ({1}, {2})", TotalValue, CurValue));
                }
            }
        }


        /// <summary>
        /// 进入一个状态时，通过设置时长
        /// </summary>
        /// <param name="on"></param>
        /// <param name="time"></param>
        protected virtual void HandleInvoke(bool on, ProcessParam process)
        {
            StopTimerId();
            mCurrentProcessParam = process;
            if (on)
            {
                TimerId = TimerTaskQueue.Instance.AddTimer((int)process.timeValue * 1000, 0, HandleStateOff);
            }
        }

        /// <summary>
        /// 一定时长执行结束回调
        /// </summary>
        protected virtual void HandleStateOff()
        {
            mCurrentProcessParam = null;
            // 执行下一个
            Consume();
        }

        protected void StopTimerId()
        {
            if (TimerId > 0)
            {
                TimerTaskQueue.Instance.DelTimer(TimerId);
                TimerId = 0;
            }
        }

        protected int TimerId { get; set; }
    }
}
