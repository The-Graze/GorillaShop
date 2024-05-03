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
namespace GorillaShop
{
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        public static Canvas canvas;

        public static GameObject Item, StuffContainer;

        public static Text ammount, totalall;

        public static bool finished, open = false, hide = true, got;

        public static List<CosmeticsController.CosmeticItem> Canbuy = new List<CosmeticsController.CosmeticItem>();
        public static List<GameObject> shopitems = new List<GameObject>();

        public static ItemManager itemManager;

        bool ran;


        string ButtonText()
        {
            if (!finished)
            {
                return "LOADING...";
            }
            else
            {
                return "Shop - " + shopitems.Count;
            }
        }

        void OnGameInitialized()
        {
            Stream str = Assembly.GetExecutingAssembly().GetManifestResourceStream("GorillaShop.Assets.shop");
            AssetBundle bundle = AssetBundle.LoadFromStream(str);
            itemManager = gameObject.AddComponent<ItemManager>();
            GameObject s = Instantiate(bundle.LoadAsset<GameObject>("shopanch"));
            canvas = s.transform.GetChild(0).GetComponent<Canvas>();
            canvas.targetDisplay = 0;
            canvas.scaleFactor = 1.5f;
            StuffContainer = canvas.transform.GetChild(0).GetChild(0).GetChild(0).gameObject;
            Item = StuffContainer.transform.GetChild(0).gameObject;
            ammount = canvas.transform.GetChild(0).GetChild(3).GetComponent<Text>();
            totalall = canvas.transform.GetChild(0).GetChild(5).GetComponent<Text>();
            canvas.gameObject.SetActive(false);
            str.Close();
        }
        void Update()
        {
            if (!ran && PhotonNetwork.IsConnectedAndReady)
            {
                OnGameInitialized();
                ran = true;
            }
            if (got && !finished)
            {
                StartCoroutine(Delay());
            }
            if (Keyboard.current.oKey.wasPressedThisFrame)
            {
                hide = !hide;
            }
            if (!hide)
            {
                if (open == true)
                {
                    canvas.gameObject.SetActive(true);
                }
                if (open == false)
                {
                    canvas.gameObject.SetActive(false);
                }
            }
        }
        private void OnGUI()
        {
            if (!hide)
            {
                if (GUI.Button(new Rect(0, 0, 80, 20f), ButtonText()))
                {
                    if (finished)
                    {
                        open = !open;
                    }
                }
            }
            else
            {
                return;
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
        public int allcost = 0;
        public bool tryon;
        void Update()
        {
            Plugin.totalall.text = allcost.ToString();
            Plugin.ammount.text = CosmeticsController.instance.currencyBalance.ToString();
            if (PhotonNetwork.IsConnectedAndReady && Plugin.got == false)
            {
                GetItems();
            }
            tryon = GorillaTagger.Instance.offlineVRRig.inTryOnRoom;
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
                }
                Plugin.Canbuy.Add(ci);
                Instantiate(Plugin.Item).AddComponent<ShopItem>().Item = ci;
            }
            Plugin.got = true;
        }
    }

    public class ShopItem : MonoBehaviour
    {
        public CosmeticsController.CosmeticItem Item;
        Image image;
        public Text DispName, Price;
        Button button;
        void Awake()
        {
            Plugin.shopitems.Add(gameObject);
            transform.SetParent(Plugin.StuffContainer.transform);
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
            if (CosmeticsController.instance.unlockedCosmetics.Contains(Item))
            {
                Price.text = "OWNED";
                button.gameObject.SetActive(false);
            }
            else if(Item.canTryOn == false) 
            {
                Price.text = "UNAVALABLE";
                button.gameObject.SetActive(false);
            }
            else if(Price.text != Item.cost.ToString())
            {
                Price.text = Item.cost.ToString();
                Plugin.itemManager.allcost = Plugin.itemManager.allcost + Item.cost;
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
            switch (Plugin.itemManager.tryon)
            {
                case true:
                    button.transform.GetChild(0).GetComponent<Text>().text = "TRY";
                    break;
                case false:
                    button.transform.GetChild(0).GetComponent<Text>().text = "BUY";
                    break;
            }
        }
        void BuyItem()
        {
            if (Plugin.itemManager.tryon)
            {
                if (CosmeticsController.instance.currentCart.Count >= 12)
                {
                    CosmeticsController.instance.currentCart.Clear();
                }
                if (!CosmeticsController.instance.currentCart.Contains(Item))
                {
                    CosmeticsController.instance.currentCart.Add(Item);
                }

            }
            else
            {
                CosmeticsController.instance.itemToBuy = Item;
                CosmeticsController.instance.PurchaseItem();
                Plugin.itemManager.allcost = Plugin.itemManager.allcost - Item.cost;
            }
            CosmeticsController.instance.UpdateShoppingCart();
            CosmeticsController.instance.UpdateCurrencyBoard();

        }
    }
}
