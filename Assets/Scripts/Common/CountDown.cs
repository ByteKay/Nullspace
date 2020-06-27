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
        private int mDays;
        private int mHours;
        private int mMinutes;
        private int mSeconds;
        private string mCountingText;
        private string mStopText;
        private string mEndText;
        private Text mText;
        private Action mEndAction;
        private TimeStringType mTimeType;
        private string mYearMonthSplitSign = "/";
        private string mMonthDaySplitSign = "/";
        private string mDayHourSplitSign = " ";
        private string mHourMinuteSplitSign = ":";
        private string mMinuteSecondSplitSign = ":";
        private string mSecondSign = "";
        private bool IsStillCountingDown = true;
        private bool IsRelease = false;
        /// <summary>
        /// CD构造函数
        /// </summary>
        /// <param name="theLabel"></param>
        /// <param name="theEndAction">CD结束时回调</param>
        public AutoCountDown(Text theLabel, Action theEndAction = null)
        {
            mText = theLabel;
            mCountingText = "";
            mStopText = "";
            mEndText = "";
            mDays = 0;
            mHours = 0;
            mMinutes = 0;
            mSeconds = 0;
            IsStillCountingDown = true;
            mEndAction = theEndAction;
            mTimeType = TimeStringType.UpToMinutes;
            mText.text = FormatTime();
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
            mText = theLabel;
            mCountingText = "";
            mStopText = "";
            mEndText = "";
            mDays = 0;
            mHours = theHours;
            mMinutes = theMinutes;
            mSeconds = theSeconds;
            IsStillCountingDown = true;
            mEndAction = theEndAction;
            mTimeType = TimeStringType.UpToMinutes;
            mText.text = FormatTime();
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
            mText = theLabel;
            mCountingText = theCountingText;
            mStopText = theStopText;
            mEndText = theEndText;
            mDays = 0;
            mHours = theHours;
            mMinutes = theMinutes;
            mSeconds = theSeconds;
            IsStillCountingDown = true;
            mEndAction = theEndAction;
            mTimeType = TimeStringType.UpToMinutes;
            mText.text = FormatTime();
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
            mText = theLabel;
            mCountingText = theCountingText;
            mStopText = theStopText;
            mEndText = theEndText;
            mDays = 0;
            mHours = theHours;
            mMinutes = theMinutes;
            mSeconds = theSeconds;
            IsStillCountingDown = true;
            mEndAction = theEndAction;
            mTimeType = theTimeStringType;
            mText.text = FormatTime();
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
            mText = theLabel;
            mCountingText = theCountingText;
            mStopText = theStopText;
            mEndText = theEndText;
            mDays = theDays;
            mHours = theHours;
            mMinutes = theMinutes;
            mSeconds = theSeconds;
            IsStillCountingDown = true;
            mEndAction = theEndAction;
            mTimeType = theTimeStringType;
            mText.text = FormatTime();
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
            mText = theLabel;
            mCountingText = theCountingText;
            mStopText = theStopText;
            mEndText = theEndText;
            mDays = secondsNum / SECONDS_DAY;
            mHours = secondsNum % SECONDS_DAY / SECONDS_HOUR;
            mMinutes = secondsNum % SECONDS_HOUR / SECONDS_MINUTE;
            mSeconds = secondsNum % SECONDS_MINUTE;
            IsStillCountingDown = true;
            mEndAction = theEndAction;
            mTimeType = theTimeStringType;
            mText.text = FormatTime();
            EnumEventDispatcher.AddEventListener(EnumEventType.SecondPast, CountDown);
        }

        /// <summary>
        /// CD析构函数
        /// </summary>
        ~AutoCountDown()
        {
            if (!IsRelease)
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
            IsRelease = true;
        }

        /// <summary>
        /// 倒计时
        /// </summary>
        public void CountDown()
        {
            if (IsStillCountingDown)
            {
                mSeconds--;
                if (mSeconds < 0)
                {
                    mSeconds = 59;
                    mMinutes--;
                    if (mMinutes < 0)
                    {
                        mMinutes = 59;
                        mHours--;
                        if (mHours < 0)
                        {
                            mDays--;
                            if (mDays < 0)
                            {
                                IsStillCountingDown = false;
                                EnumEventDispatcher.RemoveEventListener(EnumEventType.SecondPast, CountDown);
                                if (mText != null)
                                {
                                    mText.text = mEndText;
                                }
                                if (mEndAction != null)
                                {
                                    mEndAction();
                                }
                                return;
                            }
                            else
                            {
                                mHours = 23;
                            }
                        }
                    }
                }
            }
            if (mText != null)
            {
                mText.text = FormatTime();
            } 
        }

        /// <summary>
        /// 停止倒计时
        /// </summary>
        public void StopCountDown()
        {
            IsStillCountingDown = false;
            if (mText != null)
            {
                mText.text = mStopText;
            }
        }

        /// <summary>
        /// 倒计时结束
        /// </summary>
        public void EndCountDown()
        {
            IsStillCountingDown = false;
            EnumEventDispatcher.RemoveEventListener(EnumEventType.SecondPast, CountDown);
            if (mText != null)
            {
                mText.text = mEndText;
            }
        }

        /// <summary>
        /// 获取倒计时剩余秒数
        /// </summary>
        /// <returns></returns>
        public int GetLastSeconds()
        {
            int lastSeconds = 0;
            if (mDays > 0)
            {
                lastSeconds += mDays * SECONDS_DAY;
            }
            if (mHours > 0)
            {
                lastSeconds += mHours * SECONDS_HOUR;
            }
            if (mMinutes > 0)
            {
                lastSeconds += mMinutes * SECONDS_MINUTE;
            }
            if (mSeconds > 0)
            {
                lastSeconds += mSeconds;
            }
            return lastSeconds;
        }

        public void SetSplitSign(string splitSign)
        {
            mYearMonthSplitSign = splitSign;
            mMonthDaySplitSign = splitSign;
            mDayHourSplitSign = splitSign;

            mHourMinuteSplitSign = splitSign;
            mMinuteSecondSplitSign = splitSign;
        }

        public void SetSplitSign(string theHourMinuteSplitSign, string theMinuteSecondSplitSign)
        {
            mHourMinuteSplitSign = theHourMinuteSplitSign;
            mMinuteSecondSplitSign = theMinuteSecondSplitSign;
        }

        public void SetSplitSign(string theYearMonthSplitSign, string theMonthDaySplitSign, string theDayHourSplitSign, string theHourMinuteSplitSign, string theMinuteSecondSplitSign, string theSecondSign)
        {
            mYearMonthSplitSign = theYearMonthSplitSign;
            mMonthDaySplitSign = theMonthDaySplitSign;
            mDayHourSplitSign = theDayHourSplitSign;

            mHourMinuteSplitSign = theHourMinuteSplitSign;
            mMinuteSecondSplitSign = theMinuteSecondSplitSign;
            mSecondSign = theSecondSign;
        }

        public void UpdateCountingText(string newCountingText, bool isFlushNow = false)
        {
            mCountingText = newCountingText;
            if (isFlushNow)
            {
                FormatTime();
            }
        }

        public void UpdateStopText(string newStopText, bool isFlushNow = false)
        {
            mStopText = newStopText;
            if (isFlushNow)
            {
                StopCountDown();
            }
        }

        public void UpdateEndText(string newEndText, bool isFlushNow = false)
        {
            mEndText = newEndText;
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
            if (mHours < 10)
            {
                hoursString = string.Concat("0", mHours.ToString());
            }
            else
            {
                hoursString = mHours.ToString();
            }

            if (mMinutes < 10)
            {
                minutesString = string.Concat("0", mMinutes.ToString());
            }
            else
            {
                minutesString = mMinutes.ToString();
            }

            if (mSeconds < 10)
            {
                secondsString = string.Concat("0", mSeconds.ToString());
            }
            else
            {
                secondsString = mSeconds.ToString();
            }
            StrBuilder.Length = 0;
            switch (mTimeType)
            {
                case TimeStringType.Full:
                    return string.Empty;

                case TimeStringType.UpToMonth:
                    return string.Empty;

                case TimeStringType.UpToDay:
                    if (mDays > 0)
                    {
                        StrBuilder.Append(mCountingText).Append(mDays).Append(mDayHourSplitSign).Append(hoursString).Append(mHourMinuteSplitSign).Append(minutesString).Append(mMinuteSecondSplitSign).Append(secondsString).Append(mSecondSign);
                    }
                    else if (mHours > 0)
                    {
                        StrBuilder.Append(mCountingText).Append(hoursString).Append(mHourMinuteSplitSign).Append(minutesString).Append(mMinuteSecondSplitSign).Append(secondsString).Append(mSecondSign);
                    }
                    else if (mMinutes > 0)
                    {
                        StrBuilder.Append(mCountingText).Append(minutesString).Append(mMinuteSecondSplitSign).Append(secondsString).Append(mSecondSign);
                    }
                    else if (mSeconds > 0)
                    {
                        StrBuilder.Append(mCountingText).Append(minutesString).Append(mMinuteSecondSplitSign).Append(secondsString).Append(mSecondSign);
                    }
                    return StrBuilder.ToString();

                case TimeStringType.Date:
                    return string.Empty;

                case TimeStringType.UpToHour:
                    return StrBuilder.Append(mCountingText).Append(hoursString).Append(mHourMinuteSplitSign).Append(minutesString).Append(mMinuteSecondSplitSign).Append(secondsString).ToString();

                case TimeStringType.UpToMinutes:
                    return StrBuilder.Append(mCountingText).Append(minutesString).Append(mMinuteSecondSplitSign).Append(secondsString).ToString();

                case TimeStringType.UpToSecond:
                    return StrBuilder.Append(mSeconds).ToString();

                case TimeStringType.UpToDayHour:
                    {
                        string formatTime = mCountingText;
                        if (mDays > 0)
                        {
                            formatTime += string.Format("剩余{0}天{1}小时", mDays, mHours + 1);
                            return formatTime;
                        }
                        else
                        {
                            formatTime += string.Format("剩余{0}小时", mHours + 1);
                            return formatTime;
                        }
                    }

                default:
                    return string.Empty;
            }
        }
    }
}
