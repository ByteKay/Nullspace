using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Nullspace
{
    public class SequenceBehaviourTest : MonoBehaviour
    {
        private SequenceBehaviour Seq;
        private void Start()
        {
            Seq = SequenceBehaviour.Create();
            Seq.PrependInterval(3.0f);
            Seq.Insert(0.2f, transform.LocalScaleTo(new Vector3(4, 5, 5)), 4.0f);
            Seq.Append(transform.RotateTo(Quaternion.FromToRotation(Vector3.back, Vector3.left)), 4.0f);
            Seq.InsertCallback(3.0f, InsertCallback, 2);
            Seq.OnComplete(OnComplete, 4);
        }

        private void OnDestroy()
        {
            if (Seq != null && Seq.IsPlaying)
            {
                Seq.Kill();
            }
        }

        public void Update()
        {
            if (Seq != null)
            {
                DebugUtils.Info("SequenceBehaviourTest", "Update TimeLine ", Seq.TimeLine);
            }
        }

        private void OnComplete(int id)
        {
            DebugUtils.Info("SequenceBehaviourTest", "OnComplete ", id);
        }

        private void InsertCallback(int id)
        {
            DebugUtils.Info("SequenceBehaviourTest", "InsertCallback ", id);
        }
    }
}
