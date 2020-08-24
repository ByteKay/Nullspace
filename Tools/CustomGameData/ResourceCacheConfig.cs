
using Nullspace;
using System.Collections.Generic;

namespace GameData
{
    public class ResourceConfig<T> : GameDataMap<int, T>, IResourceConfig where T : GameDataMap<int, T>, new()
    {
        public static readonly string FileUrl = "ResourceConfig#ResourceConfigs";
        public static readonly bool IsDelayInitialized = true;
        public static readonly List<string> KeyNameList = new List<string>() { "ID" };

        private int mID;
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

        public ResourceConfig()
        {
            mNames = new List<string>();
        }

        public int ID { get { return mID; } set { mID = value; } }
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
