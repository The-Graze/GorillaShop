
using System.Collections.Generic;
using System.Linq;
using GorillaNetworking;
using GorillaTag;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

namespace GorillaShop
{
    public class ShopManager : MonoBehaviourPunCallbacks
    {
        public int allcost = 0;

        public static ShopManager Instance;

        public List<CosmeticsController.CosmeticItem> AllItems => CosmeticsController.instance.allCosmetics;
        public List<CosmeticsController.CosmeticItem> AvailableItems;

        Canvas canvas;

        public GameObject Item, StuffContainer;

        public Text ammount, totalall;

        bool reset, Firstrun;

        ScollBar Upscoll, Downscoll;

        Vector3 CitySpot = new Vector3(-49.9966f, 16.826f, -117.5916f);
        void Start()
        {
            Instance = this;
            canvas = Instantiate(Plugin.temp, transform).transform.GetChild(0).GetComponent<Canvas>();
            Destroy(Plugin.temp);
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.transform.parent.name = "GorillaShop";
            canvas.targetDisplay = 0;
            canvas.scaleFactor = 1.5f;
            StuffContainer = canvas.transform.GetChild(0).GetChild(0).GetChild(0).gameObject;
            Item = StuffContainer.transform.GetChild(0).gameObject;
            ammount = canvas.transform.GetChild(0).GetChild(3).GetComponent<Text>();
            totalall = canvas.transform.GetChild(0).GetChild(5).GetComponent<Text>();
            transform.GetChild(0).localPosition = Vector3.zero;
            transform.GetChild(0).GetChild(0).localPosition = Vector3.zero;
            transform.GetChild(0).GetChild(0).GetChild(0).localPosition = Vector3.zero;
            transform.localScale = new Vector3(0.001f, 0.001f, 0.001f);
            transform.position = new Vector3(-55.4996f, 16.8141f, -115.2831f);
            transform.rotation = Quaternion.Euler(359.9f, 299.4711f, 0);
        }
        public override void OnJoinedRoom()
        {
            if (!Firstrun)
            {
                foreach (CosmeticsController.CosmeticItem si in AllItems)
                {
                    if (si.canTryOn == true && si.cost == 0 && !CosmeticsController.instance.unlockedCosmetics.Contains(si))
                    {
                        CosmeticsController.instance.itemToBuy = si;
                        CosmeticsController.instance.PurchaseItem();
                    }
                }
                for (int i = 0; i < AvailableItems.Count; i++)
                {
                    ShopItem item = Instantiate(Item, StuffContainer.transform).AddComponent<ShopItem>();
                    item.Item = AvailableItems[i];
                }
                if (Upscoll == null)
                {
                    Upscoll = MakeScroller(true);
                    Upscoll.transform.localScale = new Vector3(60, 60, 60);
                    Upscoll.transform.localPosition = new Vector3(-562.0765f, -352.4211f, 50.0053f);
                    Upscoll.GetComponent<Renderer>().material = FindObjectOfType<GorillaPressableButton>().unpressedMaterial;

                    Downscoll = MakeScroller(false);
                    Downscoll.transform.localScale = new Vector3(60, 60, 60);
                    Downscoll.transform.localPosition = new Vector3(-562.0765f, 347.5789f, 50.0053f);
                    Downscoll.GetComponent<Renderer>().material = FindObjectOfType<GorillaPressableButton>().unpressedMaterial;
                }
                Firstrun = true;
            }
        }
        void Update()
        {
            AvailableItems = AllItems.Where(item => item.canTryOn == true).ToList();
            totalall.text = allcost.ToString();
            ammount.text = CosmeticsController.instance.currencyBalance.ToString();
            if (!reset && PhotonNetwork.InRoom)
            {
                StuffContainer.transform.GetChild(0).GetChild(4).gameObject.SetActive(false);
                StuffContainer.transform.localPosition = new Vector3(0, -11375f, 0);
                reset = true;
            }
        }
        ScollBar MakeScroller(bool up)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            MeshFilter meshFilter = cube.GetComponent<MeshFilter>();
            ScollBar butt = Instantiate(Plugin.ButtonBase, transform).AddComponent<ScollBar>();
            butt.GetComponent<MeshFilter>().mesh = meshFilter.mesh;
            butt.GetComponent<Renderer>().material = Plugin.ButtonBase.GetComponent<GorillaPressableButton>().unpressedMaterial;
            butt.up = up;
            Destroy(butt.GetComponent<GorillaPressableButton>());
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
                image = transform.GetChild(0).GetComponent<Image>();
                DispName = transform.GetChild(1).GetComponent<Text>();
                Price = transform.GetChild(3).GetComponent<Text>();
                button = transform.GetChild(4).GetComponent<Button>();
                button.onClick.AddListener(ButtonPress);
                DispName.text = Item.overrideDisplayName;
                button.transform.GetChild(0).GetComponent<Text>().text = "CART";
            }

            void FixedUpdate()
            {
                if (CosmeticsController.instance.unlockedCosmetics.Contains(Item) || Item.itemCategory == CosmeticsController.CosmeticCategory.Set)
                {
                    Destroy(gameObject);
                }
            }
            void Start()
            {
                Instance.allcost += Item.cost;
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
                TryButton butt = Instantiate(Plugin.ButtonBase, button.transform).AddComponent<TryButton>();
                butt.GetComponent<MeshFilter>().mesh = meshFilter.mesh;
                butt.GetComponent<Renderer>().material = Plugin.ButtonBase.GetComponent<GorillaPressableButton>().unpressedMaterial;
                Destroy(butt.GetComponent<GorillaPressableButton>());
                Destroy(cube);
                return butt;
            }
        }
    }
}
