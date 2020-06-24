using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nullspace
{
    [XmlData("ResourceCache")]
    public class ResourceConfig : XmlData<ResourceConfig>
    {
        public string Directory { get; set; }
        public List<string> Names { get; set; }
        public bool Delay { get; set; }
        public StrategyType StrategyType { get; internal set; }
        public int MaxSize { get; internal set; }
        public int MinSize { get; internal set; }
        public int LifeTime { get; internal set; }
        public string GoName { get; internal set; }
        public bool Reset { get; internal set; }
        public string BehaviourName { get; internal set; }
        public int Mask { get; internal set; }
        public int Level { get; internal set; }
        public bool IsTimerOn { get; internal set; }
    }
}
