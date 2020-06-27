using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nullspace
{
    public class ClientServerTime : Singleton<ClientServerTime>
    {

        private DateTime mDefaultFixServerTime;
        private DateTime mDateTime;
        private int mTimeZone;
        private int mTimerId;
        private int mEscapeTimerId;
        private int mDefalutSyncTime;
        private int mCurSyncTime;
        private ClientTimeSync mTimeSync;

        private void Awake()
        {
            Second = 0;
            EscapeSecond = 0;
            mTimerId = -1;
            mEscapeTimerId = -1;
            mDefalutSyncTime = 3;
            mCurSyncTime = 3;
            mTimeZone = 0;
            mDefaultFixServerTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            mTimeSync = new ClientTimeSync();
            InitTimeFromServer();
        }

        private void InitTimeFromServer()
        {
            GetTimeFromServer();
            GetTimeZoneFromServer();
        }

        protected override void OnDestroy()
        {
            TimerTaskQueue.Instance.DelTimer(mTimerId);
            TimerTaskQueue.Instance.DelTimer(mEscapeTimerId);
        }

        private void GetTimeFromServer()
        {
            Second++;
            EscapeSecond++;
            mCurSyncTime++;
            mDateTime = mDefaultFixServerTime.AddMinutes(mTimeZone).AddSeconds(Second);
            EnumEventDispatcher.TriggerEvent(EnumEventType.SecondPast);
            if (mCurSyncTime >= mDefalutSyncTime)
            {
                mCurSyncTime = 0;
                mTimeSync.GetServerTimeStampReq();
            }
            mTimerId = TimerTaskQueue.Instance.AddTimer(1000, 0, GetTimeFromServer);
        }

        public void GetTimeZoneFromServer()
        {
            mTimeSync.GetServerTimeZoneReq();
        }

        public void SetTimeZone(int theTimeZone)
        {
            mTimeZone = 0 - theTimeZone;
        }

        public int Second { get; set; }
        public int EscapeSecond { get; set; }

        public int GetTimeZone()
        {
            return mTimeZone;
        }

        public DateTime GetCurrentDateTime()
        {
            return mDefaultFixServerTime.AddMinutes(mTimeZone).AddSeconds(Second);
        }

        public DateTime GetCurrentEscapeDateTime()
        {
            return mDefaultFixServerTime.AddMinutes(mTimeZone).AddSeconds(EscapeSecond);
        }

        public DateTime GetDateTimeBySecond(int s)
        {
            return mDefaultFixServerTime.AddMinutes(mTimeZone).AddSeconds(s);
        }

        public int CalculateTimeSpanSecond(int endTime)
        {
            return endTime - Second;
        }

        public TimeSpan CalculateTimeSpanDateTime(int endTime)
        {
            return mDefaultFixServerTime.AddMinutes(mTimeZone).AddSeconds(endTime) - mDefaultFixServerTime.AddMinutes(mTimeZone).AddSeconds(Second);
        }

        public int CalculateEscapeTimeSpanSecond(int endTime)
        {
            return endTime - EscapeSecond;
        }

        public TimeSpan CalculateEscapeTimeSpanDateTime(int endTime)
        {
            return mDefaultFixServerTime.AddMinutes(mTimeZone).AddSeconds(endTime) - mDefaultFixServerTime.AddMinutes(mTimeZone).AddSeconds(EscapeSecond);
        }
    }

    public class ClientTimeSync
    {

        private List<float> mClientSendTimePoint;
        private List<uint> mServerReceiveTimePoint;
        private List<float> mClientReceiveTimePoint;
        private float mSendCount;
        private int mToolCnt;
        private int mKickCnt;
        //前端上次同步时间点，秒
        private int mClientPreTime;
        //后端上次同步时间点, 秒
        private int mServerPreTime;
        /*
            * 用RpcCall("GetServerTimeReq", (ushort)5);<=> GetServerTimeResp  预估前端时刻对应的服务器时刻
            * syncInfo[0] 这次req前端时刻  
            * syncInfo[1] 这次resp前端时刻  
            * syncInfo[2] 史上最精确req前端时刻  
            * syncInfo[3] 史上最精确resp前端时刻  
            * syncInfo[4] 史上最精确(resp-req)前端时刻  
            * syncInfo[5] 史上最精确前端时刻对应的服务器tick
        */
        public uint[] mClientServerSyncTimeInfo = { 0, 0, 0, 0, 99999999, 0 };

        public ClientTimeSync()
        {
            mClientSendTimePoint = new List<float>();
            mServerReceiveTimePoint = new List<uint>();
            mClientReceiveTimePoint = new List<float>();
            mSendCount = 5;
            mToolCnt = 0;
            mKickCnt = 0;
            mClientPreTime = 0;
            mServerPreTime = 0;
            AddListeners();
        }

        private void AddListeners()
        {
            // listeners
        }

        public void GetServerTimeStampReq()
        {
            GetServerTickReq(1);
        }

        public void GetServerTimeEscapeReq()
        {
            GetServerTickReq(2);
        }

        public void GetServerTimeZoneReq()
        {
            GetServerTickReq(3);
        }

        public void GetServerTickReq()
        {
            mClientServerSyncTimeInfo[0] = (uint)(UnityEngine.Time.realtimeSinceStartup * 1000);
            GetServerTickReq(4);
            if (mClientSendTimePoint.Count < mSendCount)
            {
                mClientSendTimePoint.Add(UnityEngine.Time.realtimeSinceStartup);
            }
            else
            {
                mClientSendTimePoint.RemoveAt(0);
                mClientSendTimePoint.Add(UnityEngine.Time.realtimeSinceStartup);
            }
        }

        public void GetServerTickReq(ushort handle)
        {
            // send
        }


        public void GetServerTimeResp(ushort handleCode, uint arg)
        {
            switch (handleCode)
            {
                case 1:
                    //CheckTime(arg);
                    int clientCurTime = (int)UnityEngine.Time.realtimeSinceStartup;
                    mClientPreTime = clientCurTime;
                    mServerPreTime = (int)arg;
                    ClientServerTime.Instance.Second = (int)arg;
                    break;
                case 2:
                    ClientServerTime.Instance.EscapeSecond = (int)arg;
                    break;
                case 3:
                    ClientServerTime.Instance.SetTimeZone((int)arg);
                    break;
                case 4:
                    mClientServerSyncTimeInfo[1] = (uint)(UnityEngine.Time.realtimeSinceStartup * 1000);
                    uint newDiaotaClientT = mClientServerSyncTimeInfo[1] - mClientServerSyncTimeInfo[0];
                    if (newDiaotaClientT < mClientServerSyncTimeInfo[4])
                    {
                        //始终取最小diaotaT的服务器时间为最准确服务器时间
                        mClientServerSyncTimeInfo[2] = mClientServerSyncTimeInfo[0];
                        mClientServerSyncTimeInfo[3] = mClientServerSyncTimeInfo[1] - newDiaotaClientT / 2;
                        mClientServerSyncTimeInfo[4] = newDiaotaClientT;
                        mClientServerSyncTimeInfo[5] = arg;
                    }
                    CheckTime(arg);
                    TimerTaskQueue.Instance.AddTimer(3000, 0, GetServerTickReq);
                    break;
            }
        }


        private void CheckTime(uint svrTime)
        {
            mClientReceiveTimePoint.Add(UnityEngine.Time.realtimeSinceStartup);
            mServerReceiveTimePoint.Add(svrTime);
            if (mServerReceiveTimePoint.Count < 2)
            {
                return;
            }
            float great = mClientReceiveTimePoint[1] - mClientSendTimePoint[0];
            float m = (mServerReceiveTimePoint[1] - mServerReceiveTimePoint[0]) * 0.001f;
            float less = mClientSendTimePoint[1] - mClientReceiveTimePoint[0];
            if (m > less)
            {
                mToolCnt = 0;
                mServerReceiveTimePoint.RemoveAt(0);
                mClientSendTimePoint.RemoveAt(0);
                mClientReceiveTimePoint.RemoveAt(0);
                return;
            }

            mToolCnt++;
            DebugUtils.Error("CheckTime", string.Format("may be used tool cnt: {0} c1 c2 {1}, {2} c1` c2, {3} c1` c2,{4} s1` s2`,{5},{6}", mToolCnt, mClientSendTimePoint[0], mClientSendTimePoint[1], mClientReceiveTimePoint[0], mClientReceiveTimePoint[1], mServerReceiveTimePoint[0], mServerReceiveTimePoint[1]));
            mServerReceiveTimePoint.RemoveAt(0);
            mClientSendTimePoint.RemoveAt(0);
            mClientReceiveTimePoint.RemoveAt(0);
            if (mToolCnt >= 2)
            {
                mToolCnt = 0;
                mServerReceiveTimePoint.Clear();
                mClientSendTimePoint.Clear();
                mClientReceiveTimePoint.Clear();
            }
        }

    }
}
