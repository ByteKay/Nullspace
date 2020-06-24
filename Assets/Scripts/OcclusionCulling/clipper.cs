using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Nullspace
{
    public class clipper
    {
        private float scale_x;
        private float scale_y;
        private float delta_x;
        private float delta_y;
        private float map_xres;
        private float map_yres;
        private Vector4[] _vi;
        private Vector4[] _vo;
        private float[] dd;
        private int vp;
        public clipper()
        {
            _vi = new Vector4[64];
            _vo = new Vector4[64];
            dd = new float[64];
        }

        public void SetResolution(int x, int y)
        {

        }
        public int ClipAndProject(int n)
        {
            return 3;
        }

        private void Clip()
        {

        }
        private void Lerp(float t, ref Vector4 a, ref Vector4 b, ref Vector4 c)
        {

        }
    }
}
