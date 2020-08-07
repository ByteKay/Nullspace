/****************************************
* The Class Is Generated Automatically By GameDataTool, 
* Don't Modify It Manually.
* DateTime: 0001-01-02 20:21:21.
****************************************/

using System;
using System.Collections.Generic;
using System.Text;
using Nullspace;
using UnityEngine;
namespace GameData
{
    public class MonsterProperty : GameDataTwoMap<uint, uint, MonsterProperty>
    {
        public static readonly string FileUrl = "Test#MonsterPropertys";
        public static readonly bool IsDelayInitialized = true;
        public static readonly List<string> KeyNameList = new List<string>(){ "Index", "ModelId" };
        public uint Index { get; private set; }
        public string Name { get; private set; }
        public string Title { get; private set; }
        public uint ModelId { get; private set; }
        public uint Zoom1 { get; private set; }
        public uint Chartlet { get; private set; }
        public uint Head { get; private set; }
        public uint Color { get; private set; }
        public uint Radius { get; private set; }
        public byte Race { get; private set; }
        public byte Sex { get; private set; }
        public byte Type { get; private set; }
        public uint Level { get; private set; }
        public byte OfficeLevel { get; private set; }
        public uint Exp { get; private set; }
        public byte IsDynamic { get; private set; }
        public int MaxHp { get; private set; }
        public int InitAngerValue { get; private set; }
        public int MaxAngerValue { get; private set; }
        public int AngerGet { get; private set; }

    }

    public class MonsterGroup : GameDataList<MonsterGroup>
    {
        public static readonly string FileUrl = "Test#MonsterGroups";
        public static readonly bool IsDelayInitialized = true;
        public static readonly List<string> KeyNameList = null;
        public uint Index { get; private set; }
        public string Name { get; private set; }
        public string Title { get; private set; }
        public uint ModelId { get; private set; }
        public uint Zoom { get; private set; }
        public uint Chartlet { get; private set; }
        public uint Color { get; private set; }
        public byte MoveType { get; private set; }
        public float MoveSpeed { get; private set; }
        public float RunSpeed { get; private set; }
        public uint TurnSpeed { get; private set; }
        public uint Height { get; private set; }
        public uint FollowRadius { get; private set; }
        public uint AttackRadius { get; private set; }
        public uint SeeRadius { get; private set; }
        public uint GroupType { get; private set; }
        public uint GroupLevel { get; private set; }
        public uint RenownDrop { get; private set; }
        public uint QuestDrop { get; private set; }
        public uint TCDrop { get; private set; }

    }

}
