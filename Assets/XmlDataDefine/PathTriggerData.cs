
using System.Collections.Generic;

namespace Nullspace
{
    [XmlData(XmlFileNameDefine.TriggerData)]
    public class PathTriggerData : XmlData<PathTriggerData>
    {
        public PathTriggerData()
        {
            Params = new Dictionary<int, string>();
        }
        public int Type { get; set; }
        public float Length { get; set; }
        public float Duration { get; set; }
        public float Accelerate { get; set; }
        public Dictionary<int, string> Params { get; set; }
        public string GetParam(int fieldKey)
        {
            return Params[fieldKey];
        }
    }
}
