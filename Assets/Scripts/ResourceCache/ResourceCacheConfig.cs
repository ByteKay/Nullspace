using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nullspace
{
    public interface IResourceConfig
    {
        int Id { get; set; }
        string Directory { get; set; }
        List<string> Names { get; set; }
        bool Delay { get; set; }
        StrategyType StrategyType { get; set; }
        int MaxSize { get; set; }
        int MinSize { get; set; }
        int LifeTime { get; set; }
        string GoName { get; set; }
        bool Reset { get; set; }
        string BehaviourName { get; set; }
        int Mask { get; set; }
        int Level { get; set; }
        bool IsTimerOn { get; set; }
    }

    public class ResourceConfig<T> : XmlData<ResourceConfig<T>>, IResourceConfig
    {
        private string mDirectory;
        private List<string> mNames;
        private bool mDelay;
        private StrategyType mStrategyType;
        private int mMaxSize;
        private int mMinSize;
        private int mLifeTime;
        private string mGoName;
        private bool mReset;
        private string mBehaviourName;
        private int mMask;
        private int mLevel;
        private bool mIsTimerOn;

        public string Directory { get { return mDirectory; } set { mDirectory = value; } }
        public List<string> Names { get { return mNames; } set { mNames = value; } }
        public bool Delay { get { return mDelay; } set { mDelay = value; } }
        public StrategyType StrategyType { get { return mStrategyType; } set { mStrategyType = value; } }
        public int MaxSize { get { return mMaxSize; } set { mMaxSize = value; } }
        public int MinSize { get { return mMinSize; } set { mMinSize = value; } }
        public int LifeTime { get { return mLifeTime; } set { mLifeTime = value; } }
        public string GoName { get { return mGoName; } set { mGoName = value; } }
        public bool Reset { get { return mReset; } set { mReset = value; } }
        public string BehaviourName { get { return mBehaviourName; } set { mBehaviourName = value; } }
        public int Mask { get { return mMask; } set { mMask = value; } }
        public int Level { get { return mLevel; } set { mLevel = value; } }
        public bool IsTimerOn { get { return mIsTimerOn; } set { mIsTimerOn = value; } }
    }
}
