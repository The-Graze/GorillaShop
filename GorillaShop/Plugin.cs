using BepInEx;
using GorillaNetworking;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR;
namespace GorillaShop
{
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    [BepInDependency("com.sinai.unityexplorer")]
    public class Plugin : BaseUnityPlugin
    {
        public static GameObject temp;
        public Plugin()
        {
            HarmonyPatches.ApplyHarmonyPatches();
            Stream str = Assembly.GetExecutingAssembly().GetManifestResourceStream("GorillaShop.Assets.shop");
            AssetBundle bundle = AssetBundle.LoadFromStream(str);
            temp = bundle.LoadAsset<GameObject>("shopanch");
            str.Close();
        }

        void Start()
        {
            GorillaTagger.OnPlayerSpawned(delegate { new GameObject("GorillaShop Manager").AddComponent<ShopManager>(); });
        }
    }
}
