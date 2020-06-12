
using UnityEngine;

namespace Nullspace
{
    public enum PolygonDirection
    {
        CCW, // counter clock wise  逆时针
        CW // clock-wise 顺时针
    }

    public class GeoPolygon2
    {
        public GeoPointsArray2 mPolygon;
        public float[] mXArray;
        public float[] mYArray;
        public float mArea;
        public PolygonDirection mDirection;

        public GeoPolygon2(GeoPointsArray2 poly)
        {
            mPolygon = poly;
            Initialize();
        }

        private void InitializeArray()
        {
            int count = mPolygon.mPointArray.Count;
            mXArray = new float[count];
            mYArray = new float[count];
            int i = 0;
            foreach (Vector2 point in mPolygon.mPointArray)
            {
                mXArray[i] = point[0];
                mXArray[i++] = point[1];
            }
        }

        public void Reverse()
        {
            mPolygon.Reverse();
            InitializeArray();
        }

        public void Initialize()
        {
            float area = 0.0f;
            mDirection = GeoPolygonUtils.CalculatePolygonArea(mPolygon, ref area);
            if (mDirection == PolygonDirection.CW)
            {
                Reverse();
                mDirection = PolygonDirection.CCW;
            }
            else
            {
                area = -area;
                InitializeArray();
            }
        }
    }

    public class GeoPolygonConvex2 : GeoPolygon2
    {
        public GeoPolygonConvex2(GeoPointsArray2 poly) :
            base(poly)
        {
        }
    }

    public class GeoPolygon3
    {
        public GeoPolygon2 mPolygon;
        public GeoPlane mPlane;
    }

}
