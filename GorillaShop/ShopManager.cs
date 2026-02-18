using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GorillaNetworking;
using UnityEngine;

namespace GorillaShop
{
    public class ShopManager : MonoBehaviour
    {
        public static ShopManager Instance;

        private static List<CosmeticsController.CosmeticItem> AllItems =>
            CosmeticsController.instance.allCosmetics;

        private List<CosmeticsController.CosmeticItem> _allItems;
        private List<string> _setItems = [];
        private List<CosmeticsController.CosmeticItem> _visibleItems;

        private static readonly Material SpriteMat = new Material(Shader.Find("UI/Default"));
        private static readonly HashSet<Texture> FilteredTextures = new();

        private Vector2 _scrollPos;
        private Rect _windowRect;

        private bool _show;
        private bool _initialized;

        private string _search = "";
        private string _lastSearch = "";

        private const float RowHeight = 90f;
        private const float IconSize = 64f; 

        private void Awake()
        {
            Instance = this;
            
            var windowWidth = Mathf.Min(750, Screen.width * 0.5f);
            var windowHeight = Mathf.Min(750, Screen.height * 0.75f);
            _windowRect = new Rect(Screen.width * 0.25f, Screen.height * 0.1f, windowWidth, windowHeight);

            StartCoroutine(WaitForCosmeticsReady());
        }

        private IEnumerator WaitForCosmeticsReady()
        {
            while (!CosmeticsV2Spawner_Dirty.allPartsInstantiated)
                yield return null;

            while (!CosmeticsController.instance ||
                   CosmeticsController.instance.allCosmetics == null ||
                   CosmeticsController.instance.allCosmetics.Count == 0)
                yield return null;
            
            InitializeItems();
            
            _initialized = true;
        }
        
        private void InitializeItems()
        {
            _allItems = AllItems
                .Where(x => x.canTryOn)
                .ToList();
            
            _allItems.RemoveAll(item => CosmeticsController.instance.unlockedCosmetics.Contains(item));
            
            var setItems = new HashSet<string>(
                _allItems
                    .Where(item => item.itemCategory == CosmeticsController.CosmeticCategory.Set)
                    .SelectMany(item => item.bundledItems)
                    .Concat(
                        CosmeticsController.instance.bundleList.data
                            .SelectMany(bundle =>
                                _allItems
                                    .Where(item => item.itemName == bundle.playFabItemName)
                                    .SelectMany(item => item.bundledItems))
                    )
            );
            
            _allItems.RemoveAll(item => setItems.Contains(item.itemName));
            
            UnlockFreeItems(_allItems);
            _allItems.RemoveAll(item => item.cost == 0); // that stupid DJ set items still kept showing up :(
            _visibleItems = new List<CosmeticsController.CosmeticItem>(_allItems);
        }


        private static void UnlockFreeItems(List<CosmeticsController.CosmeticItem> list)
        {
            foreach (var item in list
                         .Where(i => i is { canTryOn: true, cost: 0 })
                         .Where(i => !CosmeticsController.instance.unlockedCosmetics.Contains(i)))
            {
                CosmeticsController.instance.itemToBuy = item;
                CosmeticsController.instance.PurchaseItem();
            }
        }

        private void OnGUI()
        {
            if (!_initialized) return;
            
            const float buttonWidth = 150f;
            const float buttonHeight = 40f;

            if (GUI.Button(new Rect(20, 20, buttonWidth, buttonHeight), "SHOP"))
                _show = !_show;

            if (!_show) return;

            _windowRect = GUI.Window(888, _windowRect, DrawWindow, "Gorilla Shop");
        }

        private void DrawWindow(int id)
        {
            DrawSearchBar();

            var scrollWidth = _windowRect.width - 20;
            var scrollHeight = _windowRect.height - 90;
            var scrollRect = new Rect(10, 60, scrollWidth, scrollHeight);
            var contentRect = new Rect(0, 0, scrollWidth - 20, _visibleItems.Count * RowHeight);

            _scrollPos = GUI.BeginScrollView(scrollRect, _scrollPos, contentRect);

            var firstVisible = Mathf.FloorToInt(_scrollPos.y / RowHeight);
            var visibleCount = Mathf.CeilToInt(scrollHeight / RowHeight) + 2;
            var lastVisible = Mathf.Min(firstVisible + visibleCount, _visibleItems.Count);

            for (var i = firstVisible; i < lastVisible; i++)
                DrawRow(_visibleItems[i], i);

            GUI.EndScrollView();
            GUI.DragWindow(new Rect(0, 0, 10000, 30));
        }

        private void DrawSearchBar()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Search:");
            _search = GUILayout.TextField(_search);
            GUILayout.EndHorizontal();

            if (_search == _lastSearch) return;
            ApplySearch();
            _lastSearch = _search;
        }

        private void ApplySearch()
        {
            if (string.IsNullOrEmpty(_search))
            {
                _visibleItems = new List<CosmeticsController.CosmeticItem>(_allItems);
            }
            else
            {
                var lower = _search.ToLowerInvariant();
                _visibleItems = _allItems
                    .Where(x =>
                    {
                        string name = string.IsNullOrEmpty(x.overrideDisplayName)
                            ? x.displayName
                            : x.overrideDisplayName;
                        return name != null && name.ToLowerInvariant().Contains(lower);
                    })
                    .ToList();
            }
            _scrollPos = Vector2.zero;
        }

        private void DrawRow(CosmeticsController.CosmeticItem item, int index)
        {
            var y = index * RowHeight;
            var rowRect = new Rect(0, y, _windowRect.width - 40, RowHeight - 5);

            GUI.Box(rowRect, GUIContent.none);

            if (item.itemPicture)
            {
                DrawSprite(new Rect(10, y + 13, IconSize, IconSize), item.itemPicture);
            }

            var displayName = string.IsNullOrEmpty(item.overrideDisplayName)
                ? item.displayName
                : item.overrideDisplayName;

            GUI.Label(new Rect(85, y + 15, 350, 25), displayName);
            GUI.Label(new Rect(85, y + 40, 200, 25), "Price: " + item.cost);

            var cart = CosmeticsController.instance.currentCart;
            var inCart = cart.Contains(item);

            GUI.backgroundColor = inCart ? Color.red : Color.green;

            if (GUI.Button(new Rect(rowRect.width - 140, y + 25, 120, 30), inCart ? "Remove" : "Add"))
            {
                if (inCart) cart.Remove(item);
                else if (cart.Count < 12) cart.Add(item);

                CosmeticsController.instance.UpdateShoppingCart();
            }

            GUI.backgroundColor = Color.white;
        }

        private static void DrawSprite(Rect rect, Sprite sprite)
        {
            if (!sprite || !sprite.texture) return;

            var texture = sprite.texture;

            if (!FilteredTextures.Contains(texture))
            {
                texture.filterMode = FilterMode.Bilinear;
                texture.wrapMode = TextureWrapMode.Clamp;
                FilteredTextures.Add(texture);
            }

            var tr = sprite.textureRect;

            var uv = new Rect(
                tr.x / texture.width,
                tr.y / texture.height,
                tr.width / texture.width,
                tr.height / texture.height);

            Graphics.DrawTexture(rect, texture, uv, 0, 0, 0, 0, Color.white, SpriteMat);
        }
    }
}
