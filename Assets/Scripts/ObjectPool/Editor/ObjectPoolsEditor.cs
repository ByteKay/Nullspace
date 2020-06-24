using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Nullspace
{
    [CustomEditor(typeof(ObjectPools))]
    public class ObjectPoolsEditor : Editor
    {
        private ObjectPools ObjectPools;

        private void OnEnable()
        {
            ObjectPools = (ObjectPools)target;
        }

        public override void OnInspectorGUI()
        {
            ObjectPools.InspectorShow();
        }
    }
}


