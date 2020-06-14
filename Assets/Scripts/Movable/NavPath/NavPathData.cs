
using System.Collections.Generic;

using UnityEngine;

namespace Nullspace
{
    public class NavPathData
    {
        public static Dictionary<int, NavPathData> DataMap = new Dictionary<int, NavPathData>();

        public NavPathData()
        {
            OriginWayPoints = new List<Vector3>();
            WayPoints = new List<Vector3>();
            RangeLengths = new List<float>();
            SimulatePoints = new List<Vector3>();
            Triggers = new List<int>();
        }
        public int id { get; set; }
        public float pathLength { get; set; }
        public List<Vector3> OriginWayPoints { get; set; }
        public List<Vector3> WayPoints { get; set; }
        public List<Vector3> SimulatePoints { get; set; }
        public List<float> RangeLengths { get; set; }
        public List<int> Triggers { get; set; }

    }
}
