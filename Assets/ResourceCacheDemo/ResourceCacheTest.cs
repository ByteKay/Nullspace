﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Nullspace
{
    public enum EffectConfigName
    {
        THIRD_PERSON = 0,
    }

    // 空继承，区分 xml 而已
    [XmlData("ResourceCache")]
    public class EffectConfig : ResourceConfig<EffectConfig>
    {

    }

    public class ResourceCacheTest : MonoBehaviour
    {
        private bool IsInitialized = false;
        private ResourceCachePools EffectPools;
        private void Start()
        {
            EffectPools = new ResourceCachePools();
        }

        private void OnGUI()
        {
            if (GUILayout.Button("Save"))
            {
                Save();
            }
            if (!IsInitialized && GUILayout.Button("Initialize"))
            {
                XmlDataLoader.Instance.InitAndLoad("Nullspace", ResourceCachePools.SUFFIX_FLAG);
                DebugUtils.Info("EffectConfig", "DataPath " + Application.dataPath + " Count: " + EffectConfig.DataMap.Count);
                EffectPools.Initialize(ResourceCacheMask.Testing, EffectConfig.DataMap);
                IsInitialized = true;
            }
            if (IsInitialized)
            {
                if (GUILayout.Button("Play"))
                {
                    int poolId = EnumUtils.EnumToInt(EffectConfigName.THIRD_PERSON);
                    ResourceCacheBehaviourParam param = new ResourceCacheBehaviourParam();
                    EffectPools.Play(poolId, 5000, ResourceCacheBindParent.WorldEffectBind, param);
                }
            }
        }
        private void Save()
        {
            int id = 0;
            List<EffectConfig> configs = new List<EffectConfig>();

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
            configs.Add(config);

            EffectConfig.CheckDuplicatedDatas("ResourceConfig", configs);
            SortById<EffectConfig> inst = new SortById<EffectConfig>();
            configs.Sort(inst);
            XmlFileUtils.SaveXML(Application.dataPath + "/XmlData/ResourceCache"+ ResourceCachePools.SUFFIX_FLAG, configs);
        }
    }
}
