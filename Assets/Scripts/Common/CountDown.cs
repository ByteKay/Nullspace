using System;
using System.Text;
using UnityEngine.UI;

namespace Nullspace
{ 
    public class AutoCountDown
    {
        public readonly static int SECONDS_DAY = 86400;
        public readonly static int SECONDS_HOUR = 3600;
        public readonly static int SECONDS_MINUTE = 60;
        private static StringBuilder StrBuilder = new StringBuilder();
        public enum TimeStringType
        {
            Full,
            UpToMonth,
            UpToDay,
            Date,
            UpToHour, // 时：分：秒
            UpToMinutes, // 分：秒
            UpToSecond, // 秒
            UpToDayHour,// 大于1天显示为【剩余xx天xx小时】，少于1天则显示为【剩余xx小时】
        }
        /// <summary>
        /// days:天数；hours:小时数；minutes:分钟数；seconds:秒数
        /// </summary>
        private int Days;
        private int Hours;
        private int Minutes;
        private int Seconds;
        private string CountingText;
        private string StopText;
        private string EndText;
        private bool IsStillCountingDown = true;
        private TimeStringType TimeType;
        private string YearMonthSplitSign = "/";
        private string MonthDaySplitSign = "/";
        private string DayHourSplitSign = " ";
        private string HourMinuteSplitSign = ":";
        private string MinuteSecondSplitSign = ":";
        private string SecondSign = "";
        private bool HasRelease = false;
        private Text Text;
        private Action EndAction;

        /// <summary>
        /// CD构造函数
        /// </summary>
        /// <param name="theLabel"></param>
        /// <param name="theEndAction">CD结束时回调</param>
        public AutoCountDown(Text theLabel, Action theEndAction = null)
        {
            Text = theLabel;
            CountingText = "";
            StopText = "";
            EndText = "";
            Days = 0;
            Hours = 0;
            Minutes = 0;
            Seconds = 0;
            IsStillCountingDown = true;
            EndAction = theEndAction;
            TimeType = TimeStringType.UpToMinutes;
            Text.text = FormatTime();
            EnumEventDispatcher.AddEventListener(EnumEventType.SecondPast, CountDown);
        }

        /// <summary>
        /// CD构造函数
        /// </summary>
        /// <param name="theLabel"></param>
        /// <param name="theHours"></param>
        /// <param name="theMinutes"></param>
        /// <param name="theSeconds"></param>
        /// <param name="theEndAction">CD结束时回调</param>
        public AutoCountDown(Text theLabel, int theHours, int theMinutes, int theSeconds, Action theEndAction = null)
        {
            Text = theLabel;
            CountingText = "";
            StopText = "";
            EndText = "";
            Days = 0;
            Hours = theHours;
            Minutes = theMinutes;
            Seconds = theSeconds;
            IsStillCountingDown = true;
            EndAction = theEndAction;
            TimeType = TimeStringType.UpToMinutes;
            Text.text = FormatTime();
            EnumEventDispatcher.AddEventListener(EnumEventType.SecondPast, CountDown);
        }

        /// <summary>
        /// CD构造函数
        /// </summary>
        /// <param name="theLabel"></param>
        /// <param name="theHours"></param>
        /// <param name="theMinutes"></param>
        /// <param name="theSeconds"></param>
        /// <param name="theCountingText"></param>
        /// <param name="theStopText"></param>
        /// <param name="theEndText"></param>
        /// <param name="theEndAction">CD结束时回调</param>
        public AutoCountDown(Text theLabel, int theHours, int theMinutes, int theSeconds, string theCountingText, string theStopText, string theEndText, Action theEndAction = null)
        {
            Text = theLabel;
            CountingText = theCountingText;
            StopText = theStopText;
            EndText = theEndText;
            Days = 0;
            Hours = theHours;
            Minutes = theMinutes;
            Seconds = theSeconds;
            IsStillCountingDown = true;
            EndAction = theEndAction;
            TimeType = TimeStringType.UpToMinutes;
            Text.text = FormatTime();
            EnumEventDispatcher.AddEventListener(EnumEventType.SecondPast, CountDown);
        }

        /// <summary>
        /// CD构造函数
        /// </summary>
        /// <param name="theLabel"></param>
        /// <param name="theHours">小时数</param>
        /// <param name="theMinutes">分钟数</param>
        /// <param name="theSeconds">秒数</param>
        /// <param name="theCountingText"></param>
        /// <param name="theStopText"></param>
        /// <param name="theEndText"></param>
        /// <param name="theTimeStringType"></param>
        /// <param name="theEndAction">CD结束时回调</param>
        public AutoCountDown(Text theLabel, int theHours, int theMinutes, int theSeconds, string theCountingText, string theStopText, string theEndText, TimeStringType theTimeStringType, Action theEndAction = null)
        {
            Text = theLabel;
            CountingText = theCountingText;
            StopText = theStopText;
            EndText = theEndText;
            Days = 0;
            Hours = theHours;
            Minutes = theMinutes;
            Seconds = theSeconds;
            IsStillCountingDown = true;
            EndAction = theEndAction;
            TimeType = theTimeStringType;
            Text.text = FormatTime();
            EnumEventDispatcher.AddEventListener(EnumEventType.SecondPast, CountDown);
        }

        /// <summary>
        /// CD构造函数
        /// </summary>
        /// <param name="theLabel"></param>
        /// <param name="theDays">天数</param>
        /// <param name="theHours">小时数</param>
        /// <param name="theMinutes">分钟数</param>
        /// <param name="theSeconds">描述</param>
        /// <param name="theCountingText"></param>
        /// <param name="theStopText"></param>
        /// <param name="theEndText"></param>
        /// <param name="theTimeStringType">显示类型</param>
        /// <param name="theEndAction">CD结束时回调</param>
        public AutoCountDown(Text theLabel, int theDays, int theHours, int theMinutes, int theSeconds, string theCountingText, string theStopText, string theEndText, TimeStringType theTimeStringType, Action theEndAction = null)
        {
            Text = theLabel;
            CountingText = theCountingText;
            StopText = theStopText;
            EndText = theEndText;
            Days = theDays;
            Hours = theHours;
            Minutes = theMinutes;
            Seconds = theSeconds;
            IsStillCountingDown = true;
            EndAction = theEndAction;
            TimeType = theTimeStringType;
            Text.text = FormatTime();
            EnumEventDispatcher.AddEventListener(EnumEventType.SecondPast, CountDown);
        }

        /// <summary>
        /// CD构造函数
        /// </summary>
        /// <param name="theLabel"></param>
        /// <param name="secondsNum">CD秒数</param>
        /// <param name="theCountingText"></param>
        /// <param name="theStopText"></param>
        /// <param name="theEndText"></param>
        /// <param name="theTimeStringType"></param>
        /// <param name="theEndAction">CD结束时回调</param>
        public AutoCountDown(Text theLabel, int secondsNum, string theCountingText, string theStopText, string theEndText, TimeStringType theTimeStringType, Action theEndAction = null)
        {
            Text = theLabel;
            CountingText = theCountingText;
            StopText = theStopText;
            EndText = theEndText;
            Days = secondsNum / SECONDS_DAY;
            Hours = secondsNum % SECONDS_DAY / SECONDS_HOUR;
            Minutes = secondsNum % SECONDS_HOUR / SECONDS_MINUTE;
            Seconds = secondsNum % SECONDS_MINUTE;
            IsStillCountingDown = true;
            EndAction = theEndAction;
            TimeType = theTimeStringType;
            Text.text = FormatTime();
            EnumEventDispatcher.AddEventListener(EnumEventType.SecondPast, CountDown);
        }

        /// <summary>
        /// CD析构函数
        /// </summary>
        ~AutoCountDown()
        {
            if (!HasRelease)
            {
                Release();
            }
        }


        public void Release()
        {
            if (IsStillCountingDown)
            {
                EnumEventDispatcher.RemoveEventListener(EnumEventType.SecondPast, CountDown);
            }
            HasRelease = true;
        }

        /// <summary>
        /// 倒计时
        /// </summary>
        public void CountDown()
        {
            if (IsStillCountingDown)
            {
                Seconds--;
                if (Seconds < 0)
                {
                    Seconds = 59;
                    Minutes--;
                    if (Minutes < 0)
                    {
                        Minutes = 59;
                        Hours--;
                        if (Hours < 0)
                        {
                            Days--;
                            if (Days < 0)
                            {
                                IsStillCountingDown = false;
                                EnumEventDispatcher.RemoveEventListener(EnumEventType.SecondPast, CountDown);
                                if (Text != null)
                                {
                                    Text.text = EndText;
                                }
                                if (EndAction != null)
                                {
                                    EndAction();
                                }
                                return;
                            }
                            else
                            {
                                Hours = 23;
                            }
                        }
                    }
                }
            }
            if (Text != null)
            {
                Text.text = FormatTime();
            } 
        }

        /// <summary>
        /// 停止倒计时
        /// </summary>
        public void StopCountDown()
        {
            IsStillCountingDown = false;
            if (Text != null)
            {
                Text.text = StopText;
            }
        }

        /// <summary>
        /// 倒计时结束
        /// </summary>
        public void EndCountDown()
        {
            IsStillCountingDown = false;
            EnumEventDispatcher.RemoveEventListener(EnumEventType.SecondPast, CountDown);
            if (Text != null)
            {
                Text.text = EndText;
            }
        }

        /// <summary>
        /// 获取倒计时剩余秒数
        /// </summary>
        /// <returns></returns>
        public int GetLastSeconds()
        {
            int lastSeconds = 0;
            if (Days > 0)
            {
                lastSeconds += Days * SECONDS_DAY;
            }
            if (Hours > 0)
            {
                lastSeconds += Hours * SECONDS_HOUR;
            }
            if (Minutes > 0)
            {
                lastSeconds += Minutes * SECONDS_MINUTE;
            }
            if (Seconds > 0)
            {
                lastSeconds += Seconds;
            }
            return lastSeconds;
        }

        public void SetSplitSign(string splitSign)
        {
            YearMonthSplitSign = splitSign;
            MonthDaySplitSign = splitSign;
            DayHourSplitSign = splitSign;

            HourMinuteSplitSign = splitSign;
            MinuteSecondSplitSign = splitSign;
        }

        public void SetSplitSign(string theHourMinuteSplitSign, string theMinuteSecondSplitSign)
        {
            HourMinuteSplitSign = theHourMinuteSplitSign;
            MinuteSecondSplitSign = theMinuteSecondSplitSign;
        }

        public void SetSplitSign(string theYearMonthSplitSign, string theMonthDaySplitSign, string theDayHourSplitSign, string theHourMinuteSplitSign, string theMinuteSecondSplitSign, string theSecondSign)
        {
            YearMonthSplitSign = theYearMonthSplitSign;
            MonthDaySplitSign = theMonthDaySplitSign;
            DayHourSplitSign = theDayHourSplitSign;

            HourMinuteSplitSign = theHourMinuteSplitSign;
            MinuteSecondSplitSign = theMinuteSecondSplitSign;
            SecondSign = theSecondSign;
        }

        public void UpdateCountingText(string newCountingText, bool isFlushNow = false)
        {
            CountingText = newCountingText;
            if (isFlushNow)
            {
                FormatTime();
            }
        }

        public void UpdateStopText(string newStopText, bool isFlushNow = false)
        {
            StopText = newStopText;
            if (isFlushNow)
            {
                StopCountDown();
            }
        }

        public void UpdateEndText(string newEndText, bool isFlushNow = false)
        {
            EndText = newEndText;
            if (isFlushNow)
            {
                EndCountDown();
            }
        }

        private string FormatTime()
        {
            string hoursString = string.Empty;
            string minutesString = string.Empty;
            string secondsString = string.Empty;
            if (Hours < 10)
            {
                hoursString = string.Concat("0", Hours.ToString());
            }
            else
            {
                hoursString = Hours.ToString();
            }

            if (Minutes < 10)
            {
                minutesString = string.Concat("0", Minutes.ToString());
            }
            else
            {
                minutesString = Minutes.ToString();
            }

            if (Seconds < 10)
            {
                secondsString = string.Concat("0", Seconds.ToString());
            }
            else
            {
                secondsString = Seconds.ToString();
            }
            StrBuilder.Length = 0;
            switch (TimeType)
            {
                case TimeStringType.Full:
                    return string.Empty;

                case TimeStringType.UpToMonth:
                    return string.Empty;

                case TimeStringType.UpToDay:
                    if (Days > 0)
                    {
                        StrBuilder.Append(CountingText).Append(Days).Append(DayHourSplitSign).Append(hoursString).Append(HourMinuteSplitSign).Append(minutesString).Append(MinuteSecondSplitSign).Append(secondsString).Append(SecondSign);
                    }
                    else if (Hours > 0)
                    {
                        StrBuilder.Append(CountingText).Append(hoursString).Append(HourMinuteSplitSign).Append(minutesString).Append(MinuteSecondSplitSign).Append(secondsString).Append(SecondSign);
                    }
                    else if (Minutes > 0)
                    {
                        StrBuilder.Append(CountingText).Append(minutesString).Append(MinuteSecondSplitSign).Append(secondsString).Append(SecondSign);
                    }
                    else if (Seconds > 0)
                    {
                        StrBuilder.Append(CountingText).Append(minutesString).Append(MinuteSecondSplitSign).Append(secondsString).Append(SecondSign);
                    }
                    return StrBuilder.ToString();

                case TimeStringType.Date:
                    return string.Empty;

                case TimeStringType.UpToHour:
                    return StrBuilder.Append(CountingText).Append(hoursString).Append(HourMinuteSplitSign).Append(minutesString).Append(MinuteSecondSplitSign).Append(secondsString).ToString();

                case TimeStringType.UpToMinutes:
                    return StrBuilder.Append(CountingText).Append(minutesString).Append(MinuteSecondSplitSign).Append(secondsString).ToString();

                case TimeStringType.UpToSecond:
                    return StrBuilder.Append(Seconds).ToString();

                case TimeStringType.UpToDayHour:
                    {
                        string formatTime = CountingText;
                        if (Days > 0)
                        {
                            formatTime += string.Format("剩余{0}天{1}小时", Days, Hours + 1);
                            return formatTime;
                        }
                        else
                        {
                            formatTime += string.Format("剩余{0}小时", Hours + 1);
                            return formatTime;
                        }
                    }

                default:
                    return string.Empty;
            }
        }
    }
}
