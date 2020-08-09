
using System.Collections.Generic;
using UnityEngine;

namespace Nullspace
{

    public class PathTriggerData : GameDataMap<int, PathTriggerData>
    {
        public static readonly string FileUrl = "NavPathData#PathTriggerDatas";
        public static readonly bool IsDelayInitialized = true;
        public static readonly List<string> KeyNameList = new List<string>() { "ID" };

        public PathTriggerData()
        {
            Params = new Dictionary<int, string>();
        }
        public int ID { get; set; }
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

    public class NavPathData : GameDataMap<int, NavPathData>
    {
        public static readonly string FileUrl = "NavPathData#NavPathDatas";
        public static readonly bool IsDelayInitialized = true;
        public static readonly List<string> KeyNameList = new List<string>() { "ID" };

        public NavPathData()
        {
            OriginWayPoints = new List<Vector3>();
            WayPoints = new List<Vector3>();
            RangeLengths = new List<float>();
            SimulatePoints = new List<Vector3>();
            Triggers = new List<int>();
        }

        public int ID { get; set; }
        public float PathLength { get; set; }
        public List<Vector3> OriginWayPoints { get; set; }
        public List<Vector3> WayPoints { get; set; }
        public List<Vector3> SimulatePoints { get; set; }
        public List<float> RangeLengths { get; set; }
        public List<int> Triggers { get; set; }

    }
}
