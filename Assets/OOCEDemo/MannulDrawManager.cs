using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nullspace
{

    public class MannulDrawManager : Singleton<MannulDrawManager>
    {
        private Dictionary<int, OOObject> DrawObjects;
        private OOCE Culler;

        private void Awake()
        {
            DrawObjects = new Dictionary<int, OOObject>();
            InitializeCuller();
        }

        private void InitializeCuller()
        {
            Culler = new OOCE();
            Vector3 min = new Vector3(-150, -150, -150);
            Vector3 max = new Vector3(256 * 60 * 5 + 150, 10000, 256 * 60 * 5 + 150);
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

        //protected override void OnDestroy()
        //{
        //    DrawObjects.Clear();
        //    Culler = null;
        //}
    }
}


