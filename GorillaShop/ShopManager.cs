using System.Collections.Generic;
using System.Linq;
using GorillaNetworking;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace GorillaShop
{
    public class ShopManager : MonoBehaviour
    {
        public static ShopManager Instance { get; private set; }

        public List<CosmeticsController.CosmeticItem> AllItems => CosmeticsController.instance.allCosmetics;
        public List<CosmeticsController.CosmeticItem> AvailableItems { get; private set; }
        public List<CosmeticsController.CosmeticItem> SetItems = new List<CosmeticsController.CosmeticItem>();

        private Canvas canvas;
        public GameObject ItemPrefab, StuffContainer;

        private bool reset, firstRun;

        public GorillaPressableButton ButtonBase;
        private ScollBar upScroll, downScroll;

        private Vector3 citySpot = new Vector3(-49.9966f, 16.826f, -117.5916f);

        void Start() => InitializeShop();

        void InitializeShop()
        {
            Instance = this;
            transform.SetParent(GameObject.Find("Environment Objects/LocalObjects_Prefab/City_WorkingPrefab").transform, true);
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
            transform.position = new Vector3(-52.7105f,16.7912f, -121.9634f);
            transform.localRotation = Quaternion.Euler(0, 321, 0);
            StuffContainer.transform.localPosition = new Vector3(0, -999999, 0);
        }

        void UnlockFreeItems()
        {
            foreach (var item in AllItems)
            {
                if (item.canTryOn && item.cost == 0 && !CosmeticsController.instance.unlockedCosmetics.Contains(item))
                {
                    CosmeticsController.instance.itemToBuy = item;
                    CosmeticsController.instance.PurchaseItem();
                    StuffContainer.transform.localPosition = new Vector3(0, -999999, 0);
                }
            }
        }

        void PopulateShopItems()
        {
            AvailableItems = AllItems.Where(item => item.canTryOn).ToList();

            foreach (var item in AvailableItems)
            {
                if (item.itemCategory == CosmeticsController.CosmeticCategory.Set)
                {
                    if (item.bundledItems.Count() >= 1)
                    {
                        foreach (string s in item.bundledItems)
                        {
                            SetItems.Add(CosmeticsController.instance.GetItemFromDict(s));
                        }
                    }
                }
                var shopItem = Instantiate(ItemPrefab, StuffContainer.transform).AddComponent<ShopItem>();
                shopItem.Item = item;
                StuffContainer.transform.localPosition = new Vector3(0, -999999, 0);
            }
            StuffContainer.transform.localPosition = new Vector3(0, -999999, 0);
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
                downScroll.transform.localRotation = Quaternion.Euler(0, 0, 90);


                StuffContainer.transform.localPosition = new Vector3(0, -999999, 0);
            }
        }

        void FixedUpdate()
        {
            if (firstRun)
            {
                return;
            }
            else if(CosmeticsController.instance.allCosmeticsItemIDsfromDisplayNamesDict_isInitialized)
            {
                UnlockFreeItems();
                PopulateShopItems();

                if (ButtonBase == null)
                {
                    ButtonBase = FindObjectOfType<GorillaPressableButton>();
                }

                InitializeScrollBars();
                StuffContainer.transform.localPosition = new Vector3(0, -999999, 0);
                MakeToggle();
                firstRun = true;
            }
        }

        ScollBar MakeScroller(bool isUp)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var meshFilter = cube.GetComponent<MeshFilter>();
            var scollBar = Instantiate(ButtonBase, transform).AddComponent<ScollBar>();
            scollBar.GetComponent<MeshFilter>().mesh = meshFilter.mesh;
            scollBar.GetComponent<MeshRenderer>().material = Plugin.mat;
            Destroy(scollBar.GetComponent<GorillaPressableButton>());
            Destroy(cube);
            scollBar.GetComponent<ScollBar>().up = isUp;
            scollBar.name = "Scroll Button: Up = " + isUp;
            return scollBar;
        }

        ToggleButton MakeToggle()
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var meshFilter = cube.GetComponent<MeshFilter>();
            var Show = Instantiate(ButtonBase, transform).AddComponent<ToggleButton>();
            Show.GetComponent<MeshFilter>().mesh = meshFilter.mesh;
            Show.GetComponent<MeshRenderer>().material = Plugin.mat;
            Destroy(Show.GetComponent<GorillaPressableButton>());
            Destroy(cube);
            Show.transform.SetParent(transform);
            Show.name = "ShowHide Button";
            return Show;
        }

        public void Toggle(bool toggle)
        {
            upScroll.gameObject.SetActive(toggle);
            downScroll.gameObject.SetActive(toggle);
            transform.GetChild(0).gameObject.SetActive(toggle);
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

            void Start() => InitializeItem();

            void InitializeItem()
            {        
                if (displayName.text == "NOTHING" || ShopManager.Instance.SetItems.Contains(Item))
                {
                    Destroy(gameObject);
                    return;
                }
                image.sprite = Item.itemPicture;
                image.overrideSprite = Item.itemPicture;

                price.text = Item.cost.ToString();

                displayName.text = !string.IsNullOrEmpty(Item.overrideDisplayName) ? Item.overrideDisplayName : Item.displayName;
                gameObject.name = displayName.text;



                tryButton = MakeButton();
                tryButton.name = "CartButton";
                tryButton.transform.localScale = new Vector3(108, 109, 1);
                tryButton.transform.localPosition = new Vector3(2, 0, 0);
                tryButton.Item = this;
                if (CosmeticsController.instance.unlockedCosmetics.Contains(Item))
                {
                    Destroy(gameObject);
                    Instance.StuffContainer.transform.localPosition = new Vector3(0, -999999, 0);
                }
                StartCoroutine(OwnedCheck());
            }

            IEnumerator OwnedCheck()
            {
                yield return new WaitForSeconds(10);
                if (CosmeticsController.instance.unlockedCosmetics.Contains(Item))
                {
                    Destroy(gameObject);
                    StopCoroutine(OwnedCheck());
                }
                yield return StartCoroutine(OwnedCheck());
            }

            public void ButtonPress()
            {
                var cart = CosmeticsController.instance.currentCart;

                if (cart.Count >= 12)
                {
                    cart.Clear();
                    CosmeticsController.instance.UpdateShoppingCart();
                }
                if (Item.itemCategory == CosmeticsController.CosmeticCategory.Set)
                {
                    foreach (string bundledItemId in Item.bundledItems)
                    {
                        var bundledItem = CosmeticsController.instance.GetItemFromDict(bundledItemId);

                        if (cart.Contains(bundledItem))
                        {
                            cart.Remove(bundledItem);
                        }
                        else
                        {
                            cart.Add(bundledItem);
                        }
                    }
                }
                else
                {
                    if (cart.Contains(Item))
                    {
                        cart.Remove(Item);
                    }
                    else
                    {
                        cart.Add(Item);
                    }
                }
                CosmeticsController.instance.UpdateShoppingCart();
            }

            TryButton MakeButton()
            {
                var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                var meshFilter = cube.GetComponent<MeshFilter>();
                var butt = Instantiate(Instance.ButtonBase, button.transform).AddComponent<TryButton>();
                butt.GetComponent<MeshFilter>().mesh = meshFilter.mesh;
                Destroy(butt.GetComponent<WardrobeFunctionButton>());
                Destroy(cube);
                foreach (Transform t in butt.transform)
                {
                    Destroy(t.gameObject);
                }
                butt.GetComponent<Renderer>().enabled = false;
                return butt;
            }
        }
    }
}
