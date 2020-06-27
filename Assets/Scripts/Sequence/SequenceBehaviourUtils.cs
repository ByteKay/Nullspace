using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Nullspace

{
    public static class SequenceBehaviourUtils
    {
        public static BehaviourTimeCallback MoveXTo(this Transform trans, float x)
        {
            Vector3 pos = trans.position;
            pos.x = x;
            return trans.MoveTo(pos);
        }
        public static BehaviourTimeCallback MoveYTo(this Transform trans, float y)
        {
            Vector3 pos = trans.position;
            pos.y = y;
            return trans.MoveTo(pos);
        }
        public static BehaviourTimeCallback MoveZTo(this Transform trans, float z)
        {
            Vector3 pos = trans.position;
            pos.z = z;
            return trans.MoveTo(pos);
        }

        public static BehaviourTimeCallback MoveTo(this Transform trans, Vector3 end)
        {
            Callback<Transform, Vector3, Vector3, BehaviourTimeCallback> callback = new Callback<Transform, Vector3, Vector3, BehaviourTimeCallback>();
            callback.Arg1 = trans;
            callback.Arg2 = trans.position;
            callback.Arg3 = end;
            callback.Handler = (Action<Transform, Vector3, Vector3, BehaviourTimeCallback>)MoveTo;
            BehaviourTimeCallback beh = new BehaviourTimeCallback(callback);
            callback.Arg4 = beh;
            return beh;
        }
        public static BehaviourTimeCallback LocalMoveXTo(this Transform trans, float x)
        {
            Vector3 pos = trans.localPosition;
            pos.x = x;
            return trans.LocalMoveTo(pos);
        }
        public static BehaviourTimeCallback LocalMoveYTo(this Transform trans, float y)
        {
            Vector3 pos = trans.localPosition;
            pos.y = y;
            return trans.LocalMoveTo(pos);
        }
        public static BehaviourTimeCallback LocalMoveZTo(this Transform trans, float z)
        {
            Vector3 pos = trans.localPosition;
            pos.z = z;
            return trans.LocalMoveTo(pos);
        }

        public static BehaviourTimeCallback LocalMoveTo(this Transform trans, Vector3 end)
        {
            Callback<Transform, Vector3, Vector3, BehaviourTimeCallback> callback = new Callback<Transform, Vector3, Vector3, BehaviourTimeCallback>();
            callback.Arg1 = trans;
            callback.Arg2 = trans.localPosition;
            callback.Arg3 = end;
            callback.Handler = (Action<Transform, Vector3, Vector3, BehaviourTimeCallback>)LocalMoveTo;
            BehaviourTimeCallback beh = new BehaviourTimeCallback(callback);
            callback.Arg4 = beh;
            return beh;
        }

        public static BehaviourTimeCallback LocalScaleXTo(this Transform trans, float x)
        {
            Vector3 scale = trans.localScale;
            scale.x = x;
            return trans.LocalScaleTo(scale);
        }

        public static BehaviourTimeCallback LocalScaleYTo(this Transform trans, float y)
        {
            Vector3 scale = trans.localScale;
            scale.y = y;
            return trans.LocalScaleTo(scale);
        }

        public static BehaviourTimeCallback LocalScaleZTo(this Transform trans, float z)
        {
            Vector3 scale = trans.localScale;
            scale.z = z;
            return trans.LocalScaleTo(scale);
        }

        public static BehaviourTimeCallback LocalScaleTo(this Transform trans, Vector3 end)
        {
            Callback<Transform, Vector3, Vector3, BehaviourTimeCallback> callback = new Callback<Transform, Vector3, Vector3, BehaviourTimeCallback>();
            callback.Arg1 = trans;
            callback.Arg2 = trans.localScale;
            callback.Arg3 = end;
            callback.Handler = (Action<Transform, Vector3, Vector3, BehaviourTimeCallback>)LocalScaleTo;
            BehaviourTimeCallback beh = new BehaviourTimeCallback(callback);
            callback.Arg4 = beh;
            return beh;
        }


        public static BehaviourTimeCallback LocalEulerXTo(this Transform trans, float x)
        {
            Vector3 localEulers = trans.localEulerAngles;
            localEulers.x = x;
            return trans.LocalEulerTo(localEulers);
        }

        public static BehaviourTimeCallback LocalEulerYTo(this Transform trans, float y)
        {
            Vector3 localEulers = trans.localEulerAngles;
            localEulers.y = y;
            return trans.LocalEulerTo(localEulers);
        }

        public static BehaviourTimeCallback LocalEulerZTo(this Transform trans, float z)
        {
            Vector3 localEulers = trans.localEulerAngles;
            localEulers.z = z;
            return trans.LocalEulerTo(localEulers);
        }

        public static BehaviourTimeCallback LocalEulerTo(this Transform trans, Vector3 end)
        {
            Callback<Transform, Vector3, Vector3, BehaviourTimeCallback> callback = new Callback<Transform, Vector3, Vector3, BehaviourTimeCallback>();
            callback.Arg1 = trans;
            callback.Arg2 = trans.localEulerAngles;
            callback.Arg3 = end;
            callback.Handler = (Action<Transform, Vector3, Vector3, BehaviourTimeCallback>)LocalEulerTo;
            BehaviourTimeCallback beh = new BehaviourTimeCallback(callback);
            callback.Arg4 = beh;
            return beh;
        }


        public static BehaviourTimeCallback EulerXTo(this Transform trans, float x)
        {
            Vector3 eulers = trans.eulerAngles;
            eulers.x = x;
            return trans.EulerTo(eulers);
        }

        public static BehaviourTimeCallback EulerYTo(this Transform trans, float y)
        {
            Vector3 eulers = trans.eulerAngles;
            eulers.y = y;
            return trans.EulerTo(eulers);
        }

        public static BehaviourTimeCallback EulerZTo(this Transform trans, float z)
        {
            Vector3 eulers = trans.eulerAngles;
            eulers.z = z;
            return trans.EulerTo(eulers);
        }

        public static BehaviourTimeCallback EulerTo(this Transform trans, Vector3 end)
        {
            Callback<Transform, Vector3, Vector3, BehaviourTimeCallback> callback = new Callback<Transform, Vector3, Vector3, BehaviourTimeCallback>();
            callback.Arg1 = trans;
            callback.Arg2 = trans.eulerAngles;
            callback.Arg3 = end;
            callback.Handler = (Action<Transform, Vector3, Vector3, BehaviourTimeCallback>)EulerTo;
            BehaviourTimeCallback beh = new BehaviourTimeCallback(callback);
            callback.Arg4 = beh;
            return beh;
        }

        public static BehaviourTimeCallback LocalRotateTo(this Transform trans, Quaternion end)
        {
            Callback<Transform, Quaternion, Quaternion, BehaviourTimeCallback> callback = new Callback<Transform, Quaternion, Quaternion, BehaviourTimeCallback>();
            callback.Arg1 = trans;
            callback.Arg2 = trans.localRotation;
            callback.Arg3 = end;
            callback.Handler = (Action<Transform, Quaternion, Quaternion, BehaviourTimeCallback>)LocalRotateTo;
            BehaviourTimeCallback beh = new BehaviourTimeCallback(callback);
            callback.Arg4 = beh;
            return beh;
        }

        public static BehaviourTimeCallback RotateTo(this Transform trans, Quaternion end)
        {
            Callback<Transform, Quaternion, Quaternion, BehaviourTimeCallback> callback = new Callback<Transform, Quaternion, Quaternion, BehaviourTimeCallback>();
            callback.Arg1 = trans;
            callback.Arg2 = trans.rotation;
            callback.Arg3 = end;
            callback.Handler = (Action<Transform, Quaternion, Quaternion, BehaviourTimeCallback>)RotateTo;
            BehaviourTimeCallback beh = new BehaviourTimeCallback(callback);
            callback.Arg4 = beh;
            return beh;
        }


        public static BehaviourTimeCallback PathTo(this Transform trans, float duration, List<Vector3> waypoints, NavPathType pathType = NavPathType.LinePosLineDir, int subdivisions = 5)
        {
            FixedPathController pathCtrl = new FixedPathController();
            AbstractNavPath navPath = NavPathUtils.Create(pathType, Vector3.zero, false, null, waypoints, subdivisions);
            pathCtrl.StartMove(navPath, navPath.PathLength / duration, Vector3.zero, false);
            pathCtrl.mCtlPosition = trans;
            pathCtrl.mCtlRotate = trans;

            Callback<FixedPathController, BehaviourTimeCallback> callback = new Callback<FixedPathController, BehaviourTimeCallback>();
            callback.Arg1 = pathCtrl;
            callback.Handler = (Action<FixedPathController, BehaviourTimeCallback>)PathTo;
            BehaviourTimeCallback beh = new BehaviourTimeCallback(callback);
            callback.Arg2 = beh;
            return beh;
        }

        private static void PathTo(FixedPathController pathCtl, BehaviourTimeCallback beh)
        {
            pathCtl.Update(Time.deltaTime);
        }

        private static void MoveTo(Transform trans, Vector3 start, Vector3 end, BehaviourTimeCallback beh)
        {
            if (trans != null)
            {
                trans.position = GeoUtils.Interpolation(start, end, beh.Percent);
            }
        }

        private static void LocalMoveTo(Transform trans, Vector3 start, Vector3 end, BehaviourTimeCallback beh)
        {
            if (trans != null)
            {
                trans.localPosition = GeoUtils.Interpolation(start, end, beh.Percent);
            }
        }

        private static void LocalScaleTo(Transform trans, Vector3 start, Vector3 end, BehaviourTimeCallback beh)
        {
            if (trans != null)
            {
                trans.localScale = GeoUtils.Interpolation(start, end, beh.Percent);
            }
        }

        private static void EulerTo(Transform trans, Vector3 start, Vector3 end, BehaviourTimeCallback beh)
        {
            if (trans != null)
            {
                trans.eulerAngles = GeoUtils.Interpolation(start, end, beh.Percent);
            }
        }
        private static void LocalEulerTo(Transform trans, Vector3 start, Vector3 end, BehaviourTimeCallback beh)
        {
            if (trans != null)
            {
                trans.localEulerAngles = GeoUtils.Interpolation(start, end, beh.Percent);
            }
        }

        private static void RotateTo(Transform trans, Quaternion start, Quaternion end, BehaviourTimeCallback beh)
        {
            if (trans != null)
            {
                // 采用线性插值
                trans.rotation = Quaternion.Lerp(start, end, beh.Percent);
            }
        }
        private static void LocalRotateTo(Transform trans, Quaternion start, Quaternion end, BehaviourTimeCallback beh)
        {
            if (trans != null)
            {
                // 采用线性插值
                trans.localRotation = Quaternion.Lerp(start, end, beh.Percent);
            }
        }

    }
}
