using BepInEx;
using System.IO;
using System.Reflection;
using UnityEngine;
namespace GorillaShop
{
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        public static Stream str = Assembly.GetExecutingAssembly().GetManifestResourceStream("GorillaShop.Assets.shop");
        public static AssetBundle bundle = AssetBundle.LoadFromStream(str);
        public static GameObject temp = bundle.LoadAsset<GameObject>("shopanch");
        public static Material mat = bundle.LoadAsset<Material>("button");
        public static GorillaPressableButton ButtonBase;

        Plugin() => HarmonyPatches.ApplyHarmonyPatches();
        void Start()
        {
            GorillaTagger.OnPlayerSpawned(delegate
            {
                new GameObject("GorillaShop Manager", typeof(ShopManager));
                ButtonBase = FindObjectOfType<GorillaPressableButton>();
                str.Close();
                bundle.UnloadAsync(false);

                DontDestroyOnLoad(new GameObject("KeyboardListener", typeof(KeyboardListner)));
            });
        }
    }
}
