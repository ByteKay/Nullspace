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
            Culler.SafeDistance(Mathf.Sqrt(0.32f * 0.32f + 0.24f * 0.24f + 0.30f * 0.30f));
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
                // 如果相机存在变化,需要打开
                // Culler.UpdateCameraMatrix();
                // Culler.DrawFrustumPlanes();
                foreach (OOObject obj in DrawObjects.Values)
                {
                    obj.UpdateTransform();
                }
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
            base.OnDestroy();
            DrawObjects.Clear();
            Culler = null;
        }
    }
}


