using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nullspace
{

    public class FixedPathMoableTest : MonoBehaviour, IPathTrigger
    {
        private FixedPathController PathCtl;
        private void Awake()
        {
            PathCtl = new FixedPathController();
            PathCtl.mCtlPosition = transform;
            PathCtl.mCtlRotate = transform;
            PathCtl.mTriggerHandler = this;
            PathCtl.EnableMove(false);
            StartMove();
        }
        public void StartMove()
        {
            // 初始化数据
            string pointsStr = "-40.8,0.0;0.0,-13.0;20.26,-0.24;0.0,13.0;-20.56,-0.49;30.46,14.27;-24.65,-14.44;-25.59,16.77;40.00,-15.00";
            List<Vector3> wayPoints = new List<Vector3>();
            NavPathUtils.ParseVector3Array(pointsStr, ref wayPoints);
            AbstractNavPath navPath = NavPathUtils.Create(NavPathType.CurvePosCurveDir, Vector3.zero, false, this, wayPoints, 5);
            PathCtl.StartMove(navPath, 20, Vector3.zero, false);
            PathCtl.EnableMove(true);

            Callback<int> testCallback = ObjectPools.Instance.Acquire<Callback<int>>();
            testCallback.Handler = (Action<int>)TriggerCallback;
            testCallback.Arg1 = 20;
            PathCtl.RegisterTrigger(4.0f, testCallback);
        }

        public void Update()
        {
            UpdateAll(Time.deltaTime);
        }

        public void UpdateAll(float time)
        {
            // 先移动位置
            PathCtl.Update(time);
        }

        public void OnDrawGizmosSelected()
        {
            if (PathCtl != null)
            {
                PathCtl.OnDrawGizmosSelected();
            }
        }

        public void OnPathStart()
        {
            DebugUtils.Info("FixedPathMoableTest", "OnPathStart");
        }

        public void OnPathEnd()
        {
            DebugUtils.Info("FixedPathMoableTest", "OnPathEnd");
        }

        public void OnPathTrigger(int triggerId)
        {
            DebugUtils.Info("FixedPathMoableTest", "OnPathTrigger ", triggerId);
        }

        public void TriggerCallback(int test)
        {
            DebugUtils.Info("FixedPathMoableTest", "TriggerCallback ", test);
        }

        public void OnPathTrigger(AbstractCallback callback)
        {
            if (callback != null)
            {
                callback.Run();
                ObjectPools.Instance.Release(callback);
            }
            DebugUtils.Info("FixedPathMoableTest", "OnPathTrigger AbstractCallback");
        }
    }

}
