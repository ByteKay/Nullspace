using System;
using System.Collections.Generic;
using System.Text;
using Nullspace;
using UnityEngine;
namespace GameData
{
    public class MonsterProperty : GameDataTwoMap<MonsterProperty>
    {
        protected static new string FileUrl = "Test#MonsterProperty";
        protected static new bool IsDelayInitialized = true;
        protected static new List<string> KeyNameList = new List<string>(){ "Index", "ModelId" };
        public uint Index { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public uint ModelId { get; set; }
        public uint Zoom { get; set; }
        public uint Chartlet { get; set; }
        public uint Head { get; set; }
        public uint Color { get; set; }
        public uint Radius { get; set; }
        public byte Race { get; set; }
        public byte Sex { get; set; }
        public byte Type { get; set; }
        public uint Level { get; set; }
        public byte OfficeLevel { get; set; }
        public uint Exp { get; set; }
        public byte IsDynamic { get; set; }
        public int MaxHp { get; set; }
        public int InitAngerValue { get; set; }
        public int MaxAngerValue { get; set; }
        public int AngerGet { get; set; }

    }

    public class MonsterGroup : GameDataList<MonsterGroup>
    {
        protected static new string FileUrl = "Test#MonsterGroup";
        protected static new bool IsDelayInitialized = true;
        protected static new List<string> KeyNameList = null;
        public uint Index { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public uint ModelId { get; set; }
        public uint Zoom { get; set; }
        public uint Chartlet { get; set; }
        public uint Color { get; set; }
        public byte MoveType { get; set; }
        public float MoveSpeed { get; set; }
        public float RunSpeed { get; set; }
        public uint TurnSpeed { get; set; }
        public uint Height { get; set; }
        public uint FollowRadius { get; set; }
        public uint AttackRadius { get; set; }
        public uint SeeRadius { get; set; }
        public uint GroupType { get; set; }
        public uint GroupLevel { get; set; }
        public uint RenownDrop { get; set; }
        public uint QuestDrop { get; set; }
        public uint TCDrop { get; set; }

    }

}
