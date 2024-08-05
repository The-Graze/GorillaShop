using System.Collections.Generic;
using System.Linq;
using GorillaNetworking;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GorillaShop
{
    public class ShopManager : MonoBehaviour
    {
        public static ShopManager Instance { get; private set; }
        public int allCost = 0;

        public List<CosmeticsController.CosmeticItem> AllItems => CosmeticsController.instance.allCosmetics;
        public List<CosmeticsController.CosmeticItem> AvailableItems { get; private set; }

        private Canvas canvas;
        public GameObject ItemPrefab, StuffContainer;

        private bool reset, firstRun;

        public GorillaPressableButton ButtonBase;
        private ScollBar upScroll, downScroll;

        private Vector3 citySpot = new Vector3(-49.9966f, 16.826f, -117.5916f);

        void Start()
        {
            if (Instance == null)
            {
                Instance = this;
                InitializeShop();
            }
            else
            {
                Destroy(gameObject);
            }
            transform.SetParent(GameObject.Find("Environment Objects/LocalObjects_Prefab/City").transform, true);
        }

        void InitializeShop()
        {
            canvas = Instantiate(Plugin.temp, transform).transform.GetChild(0).GetComponent<Canvas>();
            Destroy(Plugin.temp);
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.transform.parent.name = "GorillaShop";
            canvas.targetDisplay = 0;
            canvas.scaleFactor = 1.5f;

            StuffContainer = canvas.transform.GetChild(0).GetChild(0).GetChild(0).gameObject;
            ItemPrefab = StuffContainer.transform.GetChild(0).gameObject;

            Transform baseTransform = transform.GetChild(0);
            baseTransform.localPosition = Vector3.zero;
            baseTransform.GetChild(0).localPosition = Vector3.zero;
            baseTransform.GetChild(0).GetChild(0).localPosition = Vector3.zero;

            transform.localScale = new Vector3(0.001f, 0.001f, 0.001f);
            transform.position = new Vector3(-55.4996f, 16.8141f, -115.2831f);
            transform.rotation = Quaternion.Euler(359.9f, 299.4711f, 0);
        }

        void UnlockFreeItems()
        {
            foreach (var item in AllItems)
            {
                if (item.canTryOn && item.cost == 0 && !CosmeticsController.instance.unlockedCosmetics.Contains(item))
                {
                    CosmeticsController.instance.itemToBuy = item;
                    CosmeticsController.instance.PurchaseItem();
                }
            }
        }

        void PopulateShopItems()
        {
            AvailableItems = AllItems.Where(item => item.canTryOn).ToList();

            foreach (var item in AvailableItems)
            {
                var shopItem = Instantiate(ItemPrefab, StuffContainer.transform).AddComponent<ShopItem>();
                shopItem.Item = item;
            }
        }

        void InitializeScrollBars()
        {
            if (upScroll == null)
            {
                upScroll = MakeScroller(true);
                upScroll.transform.localScale = new Vector3(30, 30, 30);
                upScroll.transform.localPosition = new Vector3(20.771f, -18.5237f, 6.6018f);

                downScroll = MakeScroller(false);
                downScroll.transform.localScale = new Vector3(30, 30, 30);
                downScroll.transform.localPosition = new Vector3(20.771f, 403.9127f, 6.6018f);
            }
        }

        void Update()
        {
            if (!firstRun)
            {
                UnlockFreeItems();
                PopulateShopItems();

                if (ButtonBase == null)
                {
                    ButtonBase = FindObjectOfType<GorillaPressableButton>();
                }

                InitializeScrollBars();

                firstRun = true;
            }
        }

        ScollBar MakeScroller(bool isUp)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var meshFilter = cube.GetComponent<MeshFilter>();
            var scollBar = Instantiate(ButtonBase, transform).AddComponent<ScollBar>();
            scollBar.GetComponent<MeshFilter>().mesh = meshFilter.mesh;
            scollBar.GetComponent<Renderer>().material = ButtonBase.GetComponent<GorillaPressableButton>().unpressedMaterial;
            Destroy(scollBar.GetComponent<GorillaPressableButton>());
            Destroy(cube);
            scollBar.GetComponent<ScollBar>().up = isUp;
            return scollBar;
        }


        public class ShopItem : MonoBehaviour
        {
            public CosmeticsController.CosmeticItem Item { get; set; }
            private Image image;
            private TextMeshProUGUI displayName, price;
            private GameObject button;
            private TryButton tryButton;

            void Awake()
            {
                image = transform.GetChild(0).GetComponent<Image>();
                displayName = transform.GetChild(1).GetComponent<TextMeshProUGUI>();
                price = transform.GetChild(3).GetComponent<TextMeshProUGUI>();
                button = transform.GetChild(4).gameObject;
                button.SetActive(true);
            }

            void Start()
            {
                InitializeItem();
            }

            void FixedUpdate()
            {
                if (CosmeticsController.instance.unlockedCosmetics.Contains(Item) || Item.itemCategory == CosmeticsController.CosmeticCategory.Set)
                {
                    Destroy(gameObject);
                }
            }

            void InitializeItem()
            {
                Instance.allCost += Item.cost;

                image.sprite = Item.itemPicture;
                image.overrideSprite = Item.itemPicture;

                price.text = Item.cost.ToString();

                displayName.text = !string.IsNullOrEmpty(Item.overrideDisplayName) ? Item.overrideDisplayName : Item.displayName;
                gameObject.name = displayName.text;

                if (displayName.text == "NOTHING")
                {
                    Destroy(gameObject);
                    return;
                }

                tryButton = MakeButton();
                tryButton.name = "CartButton";
                tryButton.transform.localScale = new Vector3(31.4587f, 153.8387f, 51.0874f);
                tryButton.transform.localPosition = new Vector3(-77.2265f, 1.2782f, 2.1455f);
                tryButton.Item = this;
            }

            public void ButtonPress()
            {
                var cart = CosmeticsController.instance.currentCart;

                if (cart.Count >= 12)
                {
                    cart.Clear();
                    CosmeticsController.instance.UpdateShoppingCart();
                }

                if (!cart.Contains(Item))
                {
                    cart.Add(Item);
                    CosmeticsController.instance.UpdateShoppingCart();
                }
            }

            TryButton MakeButton()
            {
                var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                var meshFilter = cube.GetComponent<MeshFilter>();
                var butt = Instantiate(Instance.ButtonBase, button.transform).AddComponent<TryButton>();
                butt.GetComponent<MeshFilter>().mesh = meshFilter.mesh;
                butt.GetComponent<Renderer>().material = Instance.ButtonBase.GetComponent<GorillaPressableButton>().unpressedMaterial;
                Destroy(butt.GetComponent<WardrobeFunctionButton>());
                Destroy(cube);
                return butt;
            }
        }
    }
}
