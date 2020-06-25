using System;

using UnityEngine;
using UnityEngine.Events;

namespace Nullspace
{
    public class GameObjectPathTween : MonoBehaviour, IPathTrigger
    {
        private AbstractNavPath NavPath;
        private NavPathType PathType;
        private float Speed;
        private GameObjectPathTween(NavPathType pathType, AbstractNavPath navPath)
        {
            PathType = pathType;
            NavPath = navPath;
            Speed = 1.0f;
        }
        
        public void SetDuration(float duration)
        {
            Speed = NavPath.PathLength / duration;
        }

        public void SetSpeed(float speed)
        {
            Speed = speed;
        }

        public void RegisterTrigger(float time, AbstractCallback callback)
        {
            float length = Speed * time;
            NavPath.InsertTriggerByLength(false, length, callback);
        }

        public void Play()
        {

        }

        public void OnPathStart()
        {
            throw new NotImplementedException();
        }

        public void OnPathEnd()
        {
            throw new NotImplementedException();
        }

        public void OnPathTrigger(int triggerId)
        {
            
        }

        public void OnPathTrigger(AbstractCallback callback)
        {
            
        }
    }
}
