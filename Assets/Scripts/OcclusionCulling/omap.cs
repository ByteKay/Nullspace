using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nullspace
{
    public class omap
    {
        private int map_xres;
        private int map_yres;
        private int map_xresb;
        private int map_yresb;
        private int[] et;
        private int[,] border;
        private int ymin;
        private int ymax;
        public List<byte> blocks;
        public List<uint> map;

        public omap()
        {
            border = new int[2000,2];
            et = new int[2000];
        }

        public void Clear()
        {

        }
        public void DrawOPolygon(List<Vector2i> vs, int vp)
        {

        }

        public int QueryOPolygon(List<Vector2i> vs, int vp)
        {
            return 1;
        }
        public void SetResolution(int x, int y)
        {

        }
        public void SetDirtyRectangle(int x1, int y1, int x2, int y2)
        {

        }
        public void UpdateBlock(List<byte> bptr, List<uint> mptr)
        {

        }

        private void DrawSpan(List<byte> adr, int min, int max)
        {

        }
        private void DrawEdge(List<Vector2i> vs, int i, int j)
        {

        }

        private void ConvertEdge(List<Vector2i> vs, int i, int j)
        {

        }

        private int TestPolygon()
        {
            return 0;
        }

    }
}
