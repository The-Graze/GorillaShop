using System;
using System.Collections.Generic;
using System.Text;
using GorillaNetworking;
using HarmonyLib;
using UnityEngine;
using static GorillaShop.ShopManager;

namespace GorillaShop.Patches
{
    [HarmonyPatch(typeof(CosmeticsController.CosmeticSet))]
    [HarmonyPatch("LoadFromPlayerPreferences", MethodType.Normal)]
    internal class CosmeticsGetPatch
    {
        private static void Postfix(CosmeticsController.CosmeticSet __instance)
        {
            foreach (CosmeticsController.CosmeticItem c in CosmeticsController.instance.allCosmetics)
            {
                if (!CosmeticsController.instance.unlockedCosmetics.Contains(c) && c.canTryOn)
                {
                    if (c.cost > 0)
                    {
                        ShopManager.Instance.Canbuy.Add(c);
                        GameObject.Instantiate(ShopManager.Instance.Item, ShopManager.Instance.StuffContainer.transform).AddComponent<ShopItem>().Item = c;
                    }
                    else
                    {
                        CosmeticsController.instance.itemToBuy = c;
                        CosmeticsController.instance.PurchaseItem();
                    }
                }
            }
        }
    }
}