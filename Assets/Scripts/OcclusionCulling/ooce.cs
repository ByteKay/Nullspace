using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Nullspace
{
    public class ooce
    {
        private frustum fr;
        private ooce_object visible;
        private ooce_object tail;
        private ooce_object temp;
        private Matrix4x4 mtr, mv, pr;
        private Vector3 position;
        private PriorityQueue<float, object, float> q1;
        private PriorityQueue<float, object, float> q2;
        private Vector3 look, abslook;
        private clipper clip;
        private float safe_distance;
        private int max_items;
        private int max_level;

        public long[] stat;
        public omap map;
        public ooce_kdtree tree;

        public ooce()
        {
            stat = new long[10];
        }

        public void Init(ref Vector3 min, ref Vector3 max)
        {

        }
        public void Camera(ref Matrix4x4 modelview, ref Matrix4x4 projection, ref Vector3 pos)
        {

        }
        public void SetResolution(int x, int y)
        {

        }
        public void Delete()
        {

        }
        public void Add(ooce_object obj)
        {

        }
        public void Set(ooce_object obj, ref Matrix4x4 m)
        {

        }
        public void Remove(ooce_object obj)
        {

        }
        public void FindVisible(int mode)
        {

        }
        public void InitTree()
        {

        }
        public void SafeDistance(float dist)
        {

        }
        public void MaxItems(int n)
        {

        }
        public void MaxDepth(int n)
        {

        }
        public int GetObjectID()
        {
            return 0;
        }
        public int GetModelID()
        {
            return 0;
        }
        public int GetFirstObject()
        {
            return 0;
        }
        public int GetNextObject()
        {
            return 0;
        }
        public int GetObjectTransform(ref Matrix4x4 m)
        {
            return 0;
        }

        private void DeleteNodes(ooce_node nd)
        {

        }
        private void FrustumCull()
        {

        }
        private void OcclusionCull()
        {

        }
        private void OcclusionCullOld()
        {

        }
        private void FlushOccluders(float distance)
        {

        }
        private int IsVisible(int flush, bbox b, float dist)
        {
            return 0;
        }
        private int IsVisible2(bbox b)
        {
            return 0;
        }
        private void PushBox(object obj, bbox b)
        {

        }
        private void MinMax(bbox b, ref float min, ref float max)
        {

        }
        private void PushBox2(object obj, bbox b)
        {

        }
        private int QueryBox(bbox x)
        {
            return 0;
        }
        private int IsSafe(bbox a, bbox b)
        {
            return 0;
        }
        private void DrawOccluder(ooce_object obj)
        {

        }
        private int DrawSafe(ooce_node nd)
        {
            return 0;
        }
    }
}
