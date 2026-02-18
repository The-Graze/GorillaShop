using BepInEx;
using GorillaNetworking;
using System.IO;
using System.Reflection;
using BepInEx.Logging;
using UnityEngine;
namespace GorillaShop
{
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        private static readonly Stream Str = Assembly.GetExecutingAssembly().GetManifestResourceStream("GorillaShop.Assets.shop");
        private static readonly AssetBundle Bundle = AssetBundle.LoadFromStream(Str);
        public static readonly GameObject Temp = Bundle.LoadAsset<GameObject>("shopanch");
        public static readonly Material Mat = Bundle.LoadAsset<Material>("button");

        private Plugin() => HarmonyPatches.ApplyHarmonyPatches();
        public static ManualLogSource PluginLogs;
        private void Start()
        {
            GorillaTagger.OnPlayerSpawned(delegate
            {
                PluginLogs = Logger;
                var o = new GameObject("GorillaShop Manager", typeof(ShopManager));
                Str.Close();
                Bundle.UnloadAsync(false);
                DontDestroyOnLoad(new GameObject("KeyboardListener", typeof(KeyboardListner)));
            });
        }
    }
}
