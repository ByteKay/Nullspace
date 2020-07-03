using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nullspace
{

    public class MannulDrawManager :  Singleton<MannulDrawManager>
    {
        private Dictionary<int, MannulDraw> DrawObjects;
        protected virtual void Awake()
        {
            DrawObjects = new Dictionary<int, MannulDraw>();
        }

        public void AddObject(MannulDraw obj)
        {
            if (!DrawObjects.ContainsKey(obj.GetInstanceID()))
            {
                DrawObjects.Add(obj.GetInstanceID(), obj);
            }
        }

        public void RemoveObject(MannulDraw obj)
        {
            if (DrawObjects.ContainsKey(obj.GetInstanceID()))
            {
                DrawObjects.Remove(obj.GetInstanceID());
            }
        }

        public void LateUpdate()
        {
            foreach (MannulDraw draw in DrawObjects.Values)
            {
                draw.DrawMesh();
            }
        }

        protected override void OnDestroy()
        {
            DrawObjects.Clear();
        }
    }
}


