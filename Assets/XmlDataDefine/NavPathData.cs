
using System.Collections.Generic;

using UnityEngine;

namespace Nullspace
{



    [XmlData(XmlFileNameDefine.PathData)]
    public class NavPathData : XmlData<NavPathData>
    {
        public NavPathData()
        {
            OriginWayPoints = new List<Vector3>();
            WayPoints = new List<Vector3>();
            RangeLengths = new List<float>();
            SimulatePoints = new List<Vector3>();
            Triggers = new List<int>();
        }
        public float PathLength { get; set; }
        public List<Vector3> OriginWayPoints { get; set; }
        public List<Vector3> WayPoints { get; set; }
        public List<Vector3> SimulatePoints { get; set; }
        public List<float> RangeLengths { get; set; }
        public List<int> Triggers { get; set; }

    }
}
