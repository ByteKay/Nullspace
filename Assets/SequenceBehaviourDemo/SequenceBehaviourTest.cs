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
            Seq.Insert(0.2f, transform.LocalScaleTo(new Vector3(4, 5, 5)), 4.0f);
            Seq.Insert(0.2f, transform.RotateTo(Quaternion.FromToRotation(Vector3.back, Vector3.left)), 4.0f);
        }

        private void OnDestroy()
        {
            if (Seq != null && Seq.IsPlaying)
            {
                Seq.Kill();
            }
        }
    }
}
