using BepInEx;
using GorillaNetworking;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

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
        public bool got;
        bool finished;
        public List<CosmeticsController.CosmeticItem> Canbuy = new List<CosmeticsController.CosmeticItem>();
        public List<GameObject> shopitems = new List<GameObject>();
        bool open;
        ItemManager itemManager;
        public static Plugin i;

        string ButtonText()
        {
            if (!finished)
            {
                return "LOADING...";
            }
            else
            {
                return "Shop";
            }
        }

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
            i = this;
            str.Close();
            bundle.Unload(false);
        }
        void Update()
        {
            if (open == true)
            {
                canvas.gameObject.SetActive(true);
            }
            if (open == false)
            {
                canvas.gameObject.SetActive(false);
            }
            if (got && !finished)
            {
                StartCoroutine(Delay());
            }
        }
        private void OnGUI()
        {
            if (GUI.Button(new Rect(0, 0, 80, 20f), ButtonText()))
            {
                if (finished)
                {
                    open = !open;
                }
            }
        }
        IEnumerator Delay()
        {
            yield return new WaitForSeconds(2);
            finished = true;
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
            }
        }

        void GetItems()
        {
            foreach (CosmeticsController.CosmeticItem ci in CosmeticsController.instance.allCosmetics)
            {
                if (!CosmeticsController.instance.unlockedCosmetics.Contains(ci) && ci.canTryOn == true)
                {
                    if (ci.cost == 0)
                    {
                        CosmeticsController.instance.itemToBuy = ci;
                        CosmeticsController.instance.PurchaseItem();
                    }
                    else
                    {
                        p.Canbuy.Add(ci);
                        Instantiate(p.Item).AddComponent<ShopItem>().Item = ci;
                    }
                }
            }
            p.got = true;
        }
    }

    public class ShopItem : MonoBehaviour
    {
        Plugin p = Plugin.i;
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
            DispName.text = Item.overrideDisplayName;
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
            if (Item.overrideDisplayName != "")
            {
                if (DispName.text != Item.overrideDisplayName)
                {
                    DispName.text = Item.overrideDisplayName;
                    gameObject.name = Item.overrideDisplayName;
                }
            }
            else 
            {
                if (DispName.text != Item.displayName)
                {
                    DispName.text = Item.displayName;
                    gameObject.name = Item.displayName;
                }
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
