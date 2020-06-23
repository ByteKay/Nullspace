

namespace Nullspace
{
    public class NullspaceRoot : Singleton<NullspaceRoot>
    {
        private void Awake()
        {
            // 1. XmlDataConfig
            // 2. LoggerConfig
            // 3. NetworkConfig
            // 4. TimerTaskQueue
            // 5. ObjectPools
        }

        protected override void OnDestroy()
        {
            
        }
    }
}


