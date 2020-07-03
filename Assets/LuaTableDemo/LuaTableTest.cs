using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nullspace
{
    // 还存在一些问题，后期修复
    public class LuaTableTest : MonoBehaviour
    {

        private LuaTable Lua;

        // Use this for initialization
        void Start()
        {
            EffectConfig config = new EffectConfig();
            config.Id = EnumUtils.EnumToInt(EffectConfigName.THIRD_PERSON);
            config.Directory = "CameraFollowDemo/ThirdPersonCharacter/Prefabs";
            config.Names = new List<string>() { "ThirdPersonController.prefab" };
            config.Delay = false;
            config.StrategyType = StrategyType.FixedForce;
            config.MaxSize = 4;
            config.MinSize = 1;
            config.LifeTime = 2000;
            config.GoName = "Test";
            config.Reset = true;
            config.BehaviourName = typeof(EffectBehaviour).FullName;
            config.Mask = EnumUtils.EnumToInt(ResourceCacheMask.Testing);
            config.Level = DeviceLevel.High;
            config.IsTimerOn = true;

            bool test = LuaTable.PackLuaTable(config, out Lua);
            string v1 = Lua.ToString();
            DebugUtils.Info("LuaTableTest", v1);

            //Lua.Clear();
            //LuaTable.ParseLuaTable(v1, out Lua);
            //string v2 = Lua.ToString();
            //DebugUtils.Info("LuaTableTest", v2);
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}


