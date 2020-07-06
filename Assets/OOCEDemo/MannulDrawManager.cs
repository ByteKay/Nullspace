using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nullspace
{

    public class MannulDrawManager :  Singleton<MannulDrawManager>
    {
        private Dictionary<int, OOObject> DrawObjects;

        private OOCE Culler;

        protected virtual void Awake()
        {
            DrawObjects = new Dictionary<int, OOObject>();
            Culler = new OOCE();
            InitializeCuller();
        }

        private void InitializeCuller()
        {
            Culler = new OOCE();
            Vector3 min = new Vector3();
            Vector3 max = new Vector3();
            min[0] = -150;
            min[1] = -150;
            min[2] = -150;
            max[0] = 256 * 60 * 5 + 150;
            max[1] = 10000;
            max[2] = 256 * 60 * 5 + 150;
            Culler.Init(ref min, ref max);
            Culler.SetResolution(Screen.width, Screen.height);
            Culler.MaxDepth(32);
            Culler.MaxItems(8);
            Culler.Camera(Camera.main);
        }

        public void AddObject(MannulDraw obj)
        {
            if (!DrawObjects.ContainsKey(obj.GetInstanceID()))
            {
                // object
                OOObject oobj = new OOObject(obj);
                oobj.SetObjectId(obj.GetInstanceID());
                // culler
                Culler.Add(oobj);
                DrawObjects.Add(oobj.GetObjectId(), oobj);
            }
        }

        public void RemoveObject(MannulDraw obj)
        {
            if (DrawObjects.ContainsKey(obj.GetInstanceID()))
            {
                Culler.Remove(DrawObjects[obj.GetInstanceID()]);
                DrawObjects.Remove(obj.GetInstanceID());
            }
        }

        public void LateUpdate()
        {
            if (Culler != null)
            {
                Culler.FindVisible(OOCE.OOCE_OCCLUSION_CULLING);
                int visible = Culler.GetFirstObject();
                while (visible == 1)
                {
                    int id = Culler.GetObjectID();
                    if (DrawObjects.ContainsKey(id))
                    {
                        DrawObjects[id].Draw();
                    }
                    visible = Culler.GetNextObject();
                }
            }
        }

        protected override void OnDestroy()
        {
            DrawObjects.Clear();
            Culler = null;
        }
    }
}


