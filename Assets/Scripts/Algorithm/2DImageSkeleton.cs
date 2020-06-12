
namespace Nullspace
{
    public class ImageSkeleton
    {
        // 2^8 = 256
        private static int[] TABLE_ARRAY = new int[256]{
         0,0,1,1,0,0,1,1,1,1,0,1,1,1,0,1,
         1,1,0,0,1,1,1,1,0,0,0,0,0,0,0,1,
         0,0,1,1,0,0,1,1,1,1,0,1,1,1,0,1,
         1,1,0,0,1,1,1,1,0,0,0,0,0,0,0,1,
         1,1,0,0,1,1,0,0,0,0,0,0,0,0,0,0,
         0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
         1,1,0,0,1,1,0,0,1,1,0,1,1,1,0,1,
         0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
         0,0,1,1,0,0,1,1,1,1,0,1,1,1,0,1,
         1,1,0,0,1,1,1,1,0,0,0,0,0,0,0,1,
         0,0,1,1,0,0,1,1,1,1,0,1,1,1,0,1,
         1,1,0,0,1,1,1,1,0,0,0,0,0,0,0,0,
         1,1,0,0,1,1,0,0,0,0,0,0,0,0,0,0,
         1,1,0,0,1,1,1,1,0,0,0,0,0,0,0,0,
         1,1,0,0,1,1,0,0,1,1,0,1,1,1,0,0,
         1,1,0,0,1,1,1,0,1,1,0,0,1,0,0,0};

        /// <summary>
        /// Be aware that the height and the width of the image should be equal.
        /// </summary>
        /// <param name="image">source data</param>
        /// <param name="num">count loop</param>
        /// <returns></returns>
        public static byte[,] Skeletonization(byte[,] image, int num = 10)
        {
            byte[,] data = CleanData(image);
            for (int i = 0; i < num; ++i)
            {
                VThin(data);
                HThin(data);
            }
            return data;
        }

        private static string ImageToString(byte[,] image)
        {
            int w = image.GetLength(0);
            int h = image.GetLength(1);
            string temp = "";
            for (int i = 0; i < w; ++i)
            {
                for (int j = 0; j < h; ++j)
                {
                    temp = string.Format("{0}_{1}", image[i, j], temp);
                }
            }
            return temp;
        }

        private static byte[,] CleanData(byte[,] image) // otsu can be applied
        {
            int w = image.GetLength(0);
            int h = image.GetLength(1);
            byte[,] result = new byte[w, h];
            for (int i = 0; i < w; ++i)
            {
                for(int j = 0; j < h; ++j)
                {
                    result[i, j] = image[i, j] > 10 ? (byte)255 : (byte)0;
                }    
            }
            return result;
        }

        private static void HThin(byte[,] image)
        {
            int h = image.GetLength(1);
            int w = image.GetLength(0);
            int next = 1;
            for (int j = 0; j < w; ++j)
            {
                for (int i = 0; i < h; ++i)
                {
                    if (next == 0)
                    {
                        next = 1;
                    }
                    else
                    {
                        int m = -1;
                        if (i > 0 && i < h - 1)
                        {
                            m = image[i - 1, j] + image[i, j] + image[i + 1, j];
                        }
                        else
                        {
                            m = 1;
                        }
                        if (image[i, j] == 0 && m != 0)
                        {
                            int[] a = new int[9] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                            for (int k = 0; k < 3; ++k)
                            {
                                for (int l = 0; l < 3; ++l)
                                {
                                    int i1 = i - 1 + k;
                                    int j1 = j - 1 + l;
                                    if (-1 < i1 && i1 < h && -1 < j1 && j1 < w && image[i1, j1] == 255)
                                    {
                                        a[k * 3 + l] = 1;
                                    }
                                }
                            }
                            int sum = a[0] * 1 + a[1] * 2 + a[2] * 4 + a[3] * 8 + a[5] * 16 + a[6] * 32 + a[7] * 64 + a[8] * 128;
                            image[i, j] = (byte)(TABLE_ARRAY[sum] * 255);
                            if (TABLE_ARRAY[sum] == 1)
                            {
                                next = 0;
                            }
                        }
                    }
                }
            }
        }

        private static void VThin(byte[,] image)
        {
            int h = image.GetLength(1);
            int w = image.GetLength(0);
            int next = 1;

            for (int i = 0; i < h; ++i)
            {
                for (int j = 0; j < w; ++j)
                {
                    if (next == 0)
                    {
                        next = 1;
                    }
                    else
                    {
                        int m = -1;
                        if (0 < j && j < w - 1)
                        {
                            m = image[i, j - 1] + image[i, j] + image[i, j + 1];
                        }
                        else
                        {
                            m = 1;
                        }
                        if (image[i, j] == 0 && m != 0)
                        {
                            int[] a = new int[9] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                            for (int k = 0; k < 3; ++k)
                            {
                                for (int l = 0; l < 3; ++l)
                                {
                                    int i1 = i - 1 + k;
                                    int j1 = j - 1 + l;
                                    if (-1 < i1 && i1 < h && -1 < j1 && j1 < w && image[i1, j1] == 255)
                                    {
                                        a[k * 3 + l] = 1;
                                    }
                                }
                            }
                            int sum = a[0] * 1 + a[1] * 2 + a[2] * 4 + a[3] * 8 + a[5] * 16 + a[6] * 32 + a[7] * 64 + a[8] * 128;
                            image[i, j] = (byte)(TABLE_ARRAY[sum] * 255);
                            if (TABLE_ARRAY[sum] == 1)
                            {
                                next = 0;
                            }
                        }
                    }
                }
            }
        }
    }
}
