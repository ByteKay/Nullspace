using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Nullspace
{
    public class ResourceCacheTest : MonoBehaviour
    {
        private bool IsInitialized = false;
        private void Start()
        {
            
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
                DebugUtils.Info("ResourceConfig", "Count: " + ResourceConfig.DataMap.Count);
                IsInitialized = true;
            }
            if (IsInitialized)
            {
                if (GUILayout.Button("Play"))
                {

                }
            }
        }
        private void Save()
        {
            int id = 0;
            List<ResourceConfig> configs = new List<ResourceConfig>();

            ResourceConfig config = new ResourceConfig();
            config.Id = id++;
            config.Directory = "ResourceCacheDemo/Prefabs";
            config.Names = new List<string>() { "1_test0.prefab", "1_test1.prefab", "1_test2.prefab" };
            config.Delay = false;
            config.StrategyType = StrategyType.FixedForce;
            config.MaxSize = 4;
            config.MinSize = 1;
            config.LifeTime = 2000;
            config.GoName = "Test";
            config.Reset = true;
            config.BehaviourName = typeof(FlyBehaviour).FullName;
            config.Mask = EnumUtils.EnumToInt(ResourceCacheMask.Testing);
            config.Level = DeviceLevel.High;
            config.IsTimerOn = true;
            configs.Add(config);

            config = new ResourceConfig();
            config.Id = id++;
            config.Directory = "ResourceCacheDemo/Prefabs";
            config.Names = new List<string>() { "2_test0.prefab", "2_test1.prefab", "2_test2.prefab" };
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

            ResourceConfig.CheckDuplicatedDatas("ResourceConfig", configs);
            configs.Sort(ResourceConfig.SortInstance);
            XmlFileUtils.SaveXML(Application.dataPath + "/XmlData/ResourceCache"+ ResourceCachePools.SUFFIX_FLAG, configs);
        }
    }
}
