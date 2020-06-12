using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nullspace
{
    public static class TimeUtils
    {
        static DateTime ZERO = new DateTime(1970, 1, 1, 0, 0, 0, 0);
        /// <summary>
        /// 格式化日期格式。（yyyy-MM-dd HH:mm:ss）
        /// </summary>
        /// <param name="datetime">日期对象</param>
        /// <returns>日期字符串</returns>
        public static String FormatTimeHMS(DateTime datetime)
        {
            return datetime.ToString("yyyy-MM-dd HH:mm:ss");
        }
        /// <summary>
        /// 格式化日期格式。（yyyy-MM-dd）
        /// </summary>
        /// <param name="datetime">日期对象</param>
        /// <returns>日期字符串</returns>
        public static String FormatTime(DateTime datetime)
        {
            return datetime.ToString("yyyy-MM-dd");
        }

        /// <summary>
        /// 格式化日期格式。（yyyy-MM-dd HH:mm:ss）
        /// </summary>
        /// <param name="datetime">日期值</param>
        /// <returns>日期字符串</returns>
        public static String FormatTime(long datetime)
        {
            DateTime time = DateTime.FromBinary(datetime);
            return time.ToString("yyyy-MM-dd HH:mm:ss");
        }

        /// <summary>
        /// 时间戳转为C#格式时间。
        /// </summary>
        /// <param name="timeStamp">时间戳</param>
        /// <returns></returns>
        public static DateTime GetTime(Int64 timeStamp)
        {
            DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(ZERO);
            return startTime.AddSeconds(timeStamp);
        }

        // 0.1 um  100nm
        public static long Ticks()
        {
            return DateTime.Now.Ticks;
        }

        public static long GetTimeStampI()
        {
            TimeSpan ts = DateTime.UtcNow - ZERO;
            return Convert.ToInt64(ts.TotalMilliseconds);
        }

        public static string GetTimeStampS()
        {
            TimeSpan ts = DateTime.UtcNow - ZERO;
            return Convert.ToInt64(ts.TotalMilliseconds).ToString();
        }
    }
}

