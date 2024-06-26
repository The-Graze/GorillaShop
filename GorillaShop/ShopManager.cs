﻿
using System.Collections.Generic;
using Cinemachine;
using GorillaLocomotion;
using GorillaNetworking;
using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UniverseLib.UI.Widgets.ScrollView;

namespace GorillaShop
{
    public class ShopManager : MonoBehaviour
    {
        public int allcost = 0;

        public static ShopManager Instance;

        public List<GameObject> shopitems = new List<GameObject>();

        Canvas canvas;

        public GameObject Item, StuffContainer;

        public Text ammount, totalall;

        bool reset;

        public List<CosmeticsController.CosmeticItem> Canbuy = new List<CosmeticsController.CosmeticItem>();

        public static GorillaPressableButton ButtonBase;

        ScollBar Upscoll;
        ScollBar Downscoll;

        Vector3 CitySpot = new Vector3(-49.9966f, 16.826f, -117.5916f);
        void Start()
        {
            Instance = this;
            Wawa();
        }

        public void Wawa()
        {
            canvas = Instantiate(Plugin.temp, transform).transform.GetChild(0).GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.name = "GorillaShop";
            canvas.targetDisplay = 0;
            canvas.scaleFactor = 1.5f;
            StuffContainer = canvas.transform.GetChild(0).GetChild(0).GetChild(0).gameObject;
            Item = StuffContainer.transform.GetChild(0).gameObject;
            ammount = canvas.transform.GetChild(0).GetChild(3).GetComponent<Text>();
            totalall = canvas.transform.GetChild(0).GetChild(5).GetComponent<Text>();
            Plugin.temp = null;
            transform.GetChild(0).localPosition = Vector3.zero;
            transform.GetChild(0).GetChild(0).localPosition = Vector3.zero;
            transform.GetChild(0).GetChild(0).GetChild(0).localPosition = Vector3.zero;
            transform.localScale = new Vector3(0.001f, 0.001f, 0.001f);
            transform.position = new Vector3(-55.4996f, 16.8141f, -115.2831f);
            transform.rotation = Quaternion.Euler(359.9f, 299.4711f, 0);
            ButtonBase = Instantiate(FindObjectOfType<WardrobeFunctionButton>());
            Upscoll = MakeScroller(true);
            Upscoll.transform.localScale = new Vector3(60, 60, 60);
            Downscoll = MakeScroller(false);
            Downscoll.transform.localScale = new Vector3(60, 60, 60);
            Downscoll.transform.localPosition = new Vector3(-562.0765f, 347.5789f, 50.0053f);
            Upscoll.transform.localPosition = new Vector3(-562.0765f, -352.4211f, 50.0053f);
        }
        void Update()
        {
            totalall.text = allcost.ToString();
            ammount.text = CosmeticsController.instance.currencyBalance.ToString();
            if (!reset && PhotonNetwork.InRoom)
            {
                StuffContainer.transform.GetChild(0).GetChild(4).gameObject.SetActive(false);
                ShopManager.Instance.StuffContainer.transform.localPosition = new Vector3(0, -11375f, 0);
                reset = true;
            }
        }
        ScollBar MakeScroller(bool up)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            MeshFilter meshFilter = cube.GetComponent<MeshFilter>();
            ScollBar butt = Instantiate(ButtonBase, transform).AddComponent<ScollBar>();
            butt.GetComponent<MeshFilter>().mesh = meshFilter.mesh;
            butt.GetComponent<Renderer>().material = ButtonBase.GetComponent<WardrobeFunctionButton>().unpressedMaterial;
            butt.up = up;
            Destroy(butt.GetComponent<WardrobeFunctionButton>());
            Destroy(cube);
            return butt;
        }

        public class ShopItem : MonoBehaviour
        {
            public CosmeticsController.CosmeticItem Item;
            Image image;
            public Text DispName, Price;
            public Button button;
            TryButton but;
            void Awake()
            {
                ShopManager.Instance.shopitems.Add(gameObject);
                image = transform.GetChild(0).GetComponent<Image>();
                DispName = transform.GetChild(1).GetComponent<Text>();
                Price = transform.GetChild(3).GetComponent<Text>();
                button = transform.GetChild(4).GetComponent<Button>();
                button.onClick.AddListener(ButtonPress);
                DispName.text = Item.overrideDisplayName;
                button.transform.GetChild(0).GetComponent<Text>().text = "CART";
            }
            void Start()
            {
                image.sprite = Item.itemPicture;
                image.overrideSprite = Item.itemPicture;
                if (Price.text != Item.cost.ToString())
                {
                    Price.text = Item.cost.ToString();
                    ShopManager.Instance.allcost += Item.cost;
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
                but = MakeButton();
                but.name = "CartButton";
                but.transform.localScale = new Vector3(31.4587f, 153.8387f, 51.0874f);
                but.transform.localPosition = new Vector3(-77.2265f, 1.2782f, 2.1455f);
                but.item = this;
            }
            public void ButtonPress()
            {
                if (CosmeticsController.instance.currentCart.Count >= 12)
                {
                    CosmeticsController.instance.currentCart.Clear();
                    CosmeticsController.instance.UpdateShoppingCart();
                }
                if (!CosmeticsController.instance.currentCart.Contains(Item))
                {
                    CosmeticsController.instance.currentCart.Add(Item);
                    CosmeticsController.instance.UpdateShoppingCart();
                }
            }
            TryButton MakeButton()
            {
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                MeshFilter meshFilter = cube.GetComponent<MeshFilter>();
                TryButton butt = Instantiate(ButtonBase, button.transform).AddComponent<TryButton>();
                butt.GetComponent<MeshFilter>().mesh = meshFilter.mesh;
                butt.GetComponent<Renderer>().material = ButtonBase.GetComponent<GorillaPressableButton>().unpressedMaterial;
                Destroy(butt.GetComponent<ModeSelectButton>());
                Destroy(cube);
                return butt;
            }
        }
    }
}
