using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Nullspace
{
    internal static class SequenceUtils
    {
        public static BehaviourCallback FillAmount(this Image img, float startV, float endV)
        {
            Callback4<Image, float, float, BehaviourCallback> callback = new Callback4<Image, float, float, BehaviourCallback>();
            callback.Arg1 = img;
            callback.Arg2 = startV;
            callback.Arg3 = endV;
            callback.Handler = (Action<Image, float, float, BehaviourCallback>)FillAmount;
            BehaviourCallback beh = new BehaviourCallback(callback, callback, callback);
            callback.Arg4 = beh;
            return beh;
        }

        private static void FillAmount(Image img, float start, float end, BehaviourCallback bc)
        {
            img.fillAmount = MathUtils.Interpolation(start, end, bc.Percent);
        }

        public static BehaviourCallback MoveTo(this Transform trans, Vector3 end)
        {
            Callback4<Transform, Vector3, Vector3, BehaviourCallback> callback = new Callback4<Transform, Vector3, Vector3, BehaviourCallback>();
            callback.Arg1 = trans;
            callback.Arg2 = trans.position;
            callback.Arg3 = end;
            callback.Handler = (Action<Transform, Vector3, Vector3, BehaviourCallback>)MoveTo;
            BehaviourCallback beh = new BehaviourCallback(callback, callback, callback);
            callback.Arg4 = beh;
            return beh;
        }

        public static BehaviourCallback LocalMoveTo(this Transform trans, Vector3 end)
        {
            Callback4<Transform, Vector3, Vector3, BehaviourCallback> callback = new Callback4<Transform, Vector3, Vector3, BehaviourCallback>();
            callback.Arg1 = trans;
            callback.Arg2 = trans.localPosition;
            callback.Arg3 = end;
            callback.Handler = (Action<Transform, Vector3, Vector3, BehaviourCallback>)LocalMoveTo;
            BehaviourCallback beh = new BehaviourCallback(callback, callback, callback);
            callback.Arg4 = beh;
            return beh;
        }

        public static BehaviourCallback LocalScaleTo(this Transform trans, Vector3 end)
        {
            Callback4<Transform, Vector3, Vector3, BehaviourCallback> callback = new Callback4<Transform, Vector3, Vector3, BehaviourCallback>();
            callback.Arg1 = trans;
            callback.Arg2 = trans.localScale;
            callback.Arg3 = end;
            callback.Handler = (Action<Transform, Vector3, Vector3, BehaviourCallback>)LocalScaleTo;
            BehaviourCallback beh = new BehaviourCallback(callback, callback, callback);
            callback.Arg4 = beh;
            return beh;
        }

        public static BehaviourCallback LocalEulerTo(this Transform trans, Vector3 end)
        {
            Callback4<Transform, Vector3, Vector3, BehaviourCallback> callback = new Callback4<Transform, Vector3, Vector3, BehaviourCallback>();
            callback.Arg1 = trans;
            callback.Arg2 = trans.localEulerAngles;
            callback.Arg3 = end;
            callback.Handler = (Action<Transform, Vector3, Vector3, BehaviourCallback>)LocalEulerTo;
            BehaviourCallback beh = new BehaviourCallback(callback, callback, callback);
            callback.Arg4 = beh;
            return beh;
        }

        public static BehaviourCallback EulerTo(this Transform trans, Vector3 end)
        {
            Callback4<Transform, Vector3, Vector3, BehaviourCallback> callback = new Callback4<Transform, Vector3, Vector3, BehaviourCallback>();
            callback.Arg1 = trans;
            callback.Arg2 = trans.eulerAngles;
            callback.Arg3 = end;
            callback.Handler = (Action<Transform, Vector3, Vector3, BehaviourCallback>)EulerTo;
            BehaviourCallback beh = new BehaviourCallback(callback, callback, callback);
            callback.Arg4 = beh;
            return beh;
        }

        public static BehaviourCallback LocalRotateTo(this Transform trans, Quaternion end)
        {
            Callback4<Transform, Quaternion, Quaternion, BehaviourCallback> callback = new Callback4<Transform, Quaternion, Quaternion, BehaviourCallback>();
            callback.Arg1 = trans;
            callback.Arg2 = trans.localRotation;
            callback.Arg3 = end;
            callback.Handler = (Action<Transform, Quaternion, Quaternion, BehaviourCallback>)LocalRotateTo;
            BehaviourCallback beh = new BehaviourCallback(callback, callback, callback);
            callback.Arg4 = beh;
            return beh;
        }

        public static BehaviourCallback RotateTo(this Transform trans, Quaternion end)
        {
            Callback4<Transform, Quaternion, Quaternion, BehaviourCallback> callback = new Callback4<Transform, Quaternion, Quaternion, BehaviourCallback>();
            callback.Arg1 = trans;
            callback.Arg2 = trans.rotation;
            callback.Arg3 = end;
            callback.Handler = (Action<Transform, Quaternion, Quaternion, BehaviourCallback>)RotateTo;
            BehaviourCallback beh = new BehaviourCallback(callback, callback, callback);
            callback.Arg4 = beh;
            return beh;
        }
        
        //public static BehaviourCallback PathTo(this Transform trans, float duration, List<Vector3> waypoints, NavPathType pathType = NavPathType.LinePosLineDir, int subdivisions = 5)
        //{
        //    FixedPathController pathCtrl = new FixedPathController();
        //    AbstractNavPath navPath = NavPathUtils.Create(pathType, Vector3.zero, NavPathFlipType.None, null, waypoints, subdivisions);
        //    pathCtrl.StartMove(navPath, navPath.PathLength / duration, Vector3.zero, false);
        //    pathCtrl.mCtlPosition = trans;
        //    pathCtrl.mCtlRotate = trans;

        //    Callback<FixedPathController, BehaviourCallback> callback = new Callback<FixedPathController, BehaviourCallback>();
        //    callback.Arg1 = pathCtrl;
        //    callback.Handler = (Action<FixedPathController, BehaviourCallback>)PathTo;
        //    BehaviourCallback beh = new BehaviourCallback(callback, callback, callback);
        //    callback.Arg2 = beh;
        //    return beh;
        //}

        public static BehaviourCallback MoveXTo(this Transform trans, float x)
        {
            Vector3 pos = trans.position;
            pos.x = x;
            return trans.MoveTo(pos);
        }
        public static BehaviourCallback MoveYTo(this Transform trans, float y)
        {
            Vector3 pos = trans.position;
            pos.y = y;
            return trans.MoveTo(pos);
        }
        public static BehaviourCallback MoveZTo(this Transform trans, float z)
        {
            Vector3 pos = trans.position;
            pos.z = z;
            return trans.MoveTo(pos);
        }

        public static BehaviourCallback LocalMoveXTo(this Transform trans, float x)
        {
            Vector3 pos = trans.localPosition;
            pos.x = x;
            return trans.LocalMoveTo(pos);
        }
        public static BehaviourCallback LocalMoveYTo(this Transform trans, float y)
        {
            Vector3 pos = trans.localPosition;
            pos.y = y;
            return trans.LocalMoveTo(pos);
        }
        public static BehaviourCallback LocalMoveZTo(this Transform trans, float z)
        {
            Vector3 pos = trans.localPosition;
            pos.z = z;
            return trans.LocalMoveTo(pos);
        }


        public static BehaviourCallback LocalScaleXTo(this Transform trans, float x)
        {
            Vector3 scale = trans.localScale;
            scale.x = x;
            return trans.LocalScaleTo(scale);
        }

        public static BehaviourCallback LocalScaleYTo(this Transform trans, float y)
        {
            Vector3 scale = trans.localScale;
            scale.y = y;
            return trans.LocalScaleTo(scale);
        }

        public static BehaviourCallback LocalScaleZTo(this Transform trans, float z)
        {
            Vector3 scale = trans.localScale;
            scale.z = z;
            return trans.LocalScaleTo(scale);
        }



        public static BehaviourCallback LocalEulerXTo(this Transform trans, float x)
        {
            Vector3 localEulers = trans.localEulerAngles;
            localEulers.x = x;
            return trans.LocalEulerTo(localEulers);
        }

        public static BehaviourCallback LocalEulerYTo(this Transform trans, float y)
        {
            Vector3 localEulers = trans.localEulerAngles;
            localEulers.y = y;
            return trans.LocalEulerTo(localEulers);
        }

        public static BehaviourCallback LocalEulerZTo(this Transform trans, float z)
        {
            Vector3 localEulers = trans.localEulerAngles;
            localEulers.z = z;
            return trans.LocalEulerTo(localEulers);
        }


        public static BehaviourCallback EulerXTo(this Transform trans, float x)
        {
            Vector3 eulers = trans.eulerAngles;
            eulers.x = x;
            return trans.EulerTo(eulers);
        }

        public static BehaviourCallback EulerYTo(this Transform trans, float y)
        {
            Vector3 eulers = trans.eulerAngles;
            eulers.y = y;
            return trans.EulerTo(eulers);
        }

        public static BehaviourCallback EulerZTo(this Transform trans, float z)
        {
            Vector3 eulers = trans.eulerAngles;
            eulers.z = z;
            return trans.EulerTo(eulers);
        }


        private static void PathTo(NavPathController pathCtl, BehaviourCallback beh)
        {
            pathCtl.Update(Time.deltaTime);
        }

        private static void MoveTo(Transform trans, Vector3 start, Vector3 end, BehaviourCallback beh)
        {
            if (trans != null)
            {
                trans.position = MathUtils.Interpolation(start, end, beh.Percent);
            }
        }

        private static void LocalMoveTo(Transform trans, Vector3 start, Vector3 end, BehaviourCallback beh)
        {
            if (trans != null)
            {
                trans.localPosition = MathUtils.Interpolation(start, end, beh.Percent);
            }
        }

        private static void LocalScaleTo(Transform trans, Vector3 start, Vector3 end, BehaviourCallback beh)
        {
            if (trans != null)
            {
                trans.localScale = MathUtils.Interpolation(start, end, beh.Percent);
            }
        }

        private static void EulerTo(Transform trans, Vector3 start, Vector3 end, BehaviourCallback beh)
        {
            if (trans != null)
            {
                trans.eulerAngles = MathUtils.Interpolation(start, end, beh.Percent);
            }
        }
        private static void LocalEulerTo(Transform trans, Vector3 start, Vector3 end, BehaviourCallback beh)
        {
            if (trans != null)
            {
                trans.localEulerAngles = MathUtils.Interpolation(start, end, beh.Percent);
            }
        }

        private static void RotateTo(Transform trans, Quaternion start, Quaternion end, BehaviourCallback beh)
        {
            if (trans != null)
            {
                // 采用线性插值
                trans.rotation = Quaternion.Lerp(start, end, beh.Percent);
            }
        }
        private static void LocalRotateTo(Transform trans, Quaternion start, Quaternion end, BehaviourCallback beh)
        {
            if (trans != null)
            {
                // 采用线性插值
                trans.localRotation = Quaternion.Lerp(start, end, beh.Percent);
            }
        }

    }
}
