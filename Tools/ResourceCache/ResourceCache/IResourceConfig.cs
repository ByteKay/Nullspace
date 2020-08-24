using System.Collections.Generic;

namespace Nullspace
{
    public interface IResourceConfig
    {
        int ID { get; set; }
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
}
