
using System.Collections.Generic;

namespace Nullspace
{
    public class PathTriggerData
    {
        public static Dictionary<int, PathTriggerData> DataMap = new Dictionary<int, PathTriggerData>();

        public PathTriggerData()
        {
            Params = new Dictionary<int, string>();
        }

        public int id { get; set; }
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
