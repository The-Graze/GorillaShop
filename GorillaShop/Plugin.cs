using BepInEx;
using GorillaNetworking;
using HarmonyLib;
using Photon.Pun;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utilla;
using System.Collections.Generic;
using UniverseLib.UI;
using UnityEngine.EventSystems;

namespace GorillaShop
{
    [BepInDependency("org.legoandmars.gorillatag.utilla", "1.5.0")]
    [BepInDependency("com.sinai.unityexplorer")]
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        Canvas canvas;
        public GameObject StuffContainer;
        public GameObject Item;
        public Text ammount;
        public Text totalall;
        public bool got = false;
        public List<CosmeticsController.CosmeticItem> Canbuy = new List<CosmeticsController.CosmeticItem>();
        public List<GameObject> shopitems = new List<GameObject>();
        bool open = false;
        ItemManager itemManager;
        void Start(){Utilla.Events.GameInitialized += OnGameInitialized;}
        void OnGameInitialized(object sender, EventArgs e)
        {
            itemManager = gameObject.AddComponent<ItemManager>();
            Stream str = Assembly.GetExecutingAssembly().GetManifestResourceStream("GorillaShop.Assets.shop");
            AssetBundle bundle = AssetBundle.LoadFromStream(str);
            GameObject s = Instantiate(bundle.LoadAsset<GameObject>("shopanch"));
            canvas = s.transform.GetChild(0).GetComponent<Canvas>();
            canvas.targetDisplay = 0;
            canvas.scaleFactor = 1.5f;
            StuffContainer = canvas.transform.GetChild(0).GetChild(0).GetChild(0).gameObject;
            Item = StuffContainer.transform.GetChild(0).gameObject;
            ammount = canvas.transform.GetChild(0).GetChild(3).GetComponent<Text>();
            totalall = canvas.transform.GetChild(0).GetChild(5).GetComponent<Text>();
            canvas.gameObject.SetActive(false);
            open = false;
        }
        void Update()
        {
            if (open == true)
            {
                canvas.gameObject.SetActive(true);
                itemManager.enabled = true;
            }
            if (open == false)
            {
                canvas.gameObject.SetActive(false);
                itemManager.enabled = false;
            }
        }
        private void OnGUI()
        {
            if (GUI.Button(new Rect(0, 0, 80, 20f), "Shop"))
            {
                open = !open;
            }

        }
    }
    public class ItemManager : MonoBehaviour
    {
        Plugin p;
        public int allcost;
        void Awake()
        {
            p = gameObject.GetComponent<Plugin>();
            allcost = 0;
        }
        void Update()
        {
            p.totalall.text = allcost.ToString();
            p.ammount.text = CosmeticsController.instance.currencyBalance.ToString();
            if (PhotonNetwork.IsConnectedAndReady && p.got == false)
            {
                GetItems();
                p.got = true;
            }
            foreach (CosmeticsController.CosmeticItem c in CosmeticsController.instance.unlockedCosmetics)
            {
                foreach (GameObject g in p.shopitems)
                {
                    if (c.displayName == g.GetComponent<ShopItem>().DispName.text)
                    {
                        allcost = allcost - c.cost;
                        p.shopitems.Remove(g);
                        p.Canbuy.Remove(g.GetComponent<ShopItem>().Item);
                        Destroy(g);
                    }
                }
            }
        }
        void GetItems()
        {
            foreach (CosmeticsController.CosmeticItem ci in CosmeticsController.instance.allCosmetics)
            {
                if (ci.cost > 0 && ci.canTryOn == true)
                {
                    p.Canbuy.Add(ci);
                    Instantiate(p.Item).AddComponent<ShopItem>().Item = ci;
                }
            }
        }
    }

    public class ShopItem : MonoBehaviour
    {
        Plugin p = GameObject.Find("BepInEx_Manager").GetComponent<Plugin>();
        ItemManager i;
        public CosmeticsController.CosmeticItem Item;
        Image image;
        public Text DispName;
        Text Price;
        Button button;
        void Awake()
        {  
            i = p.gameObject.GetComponent<ItemManager>();
            p.shopitems.Add(gameObject);
            transform.SetParent(p.StuffContainer.transform);
            image = transform.GetChild(0).GetComponent<Image>();
            DispName = transform.GetChild(1).GetComponent<Text>();
            Price = transform.GetChild(3).GetComponent<Text>();
            button = transform.GetChild(4).GetComponent<Button>();  
            DispName.text = Item.displayName;
            button.onClick.AddListener(BuyItem);
        }
        void Update()
        {
            if (image.sprite != Item.itemPicture || image.overrideSprite != Item.itemPicture)
            {
                image.sprite = Item.itemPicture;
                image.overrideSprite = Item.itemPicture;
            }
            if (Price.text != Item.cost.ToString())
            {
                Price.text = Item.cost.ToString();
                i.allcost = i.allcost + Item.cost;
            }
            if (DispName.text != Item.displayName)
            {
                DispName.text = Item.displayName;
                gameObject.name = Item.displayName;
            }
            if (DispName.text == "NOTHING")
            {
                Destroy(this.gameObject);
            }
        }
        void BuyItem()
        {
            CosmeticsController.instance.itemToBuy = Item;
            CosmeticsController.instance.PurchaseItem();
        }
    }
}
