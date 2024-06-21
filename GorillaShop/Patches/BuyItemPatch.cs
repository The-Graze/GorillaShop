using System;
using System.Collections.Generic;
using System.Text;
using GorillaNetworking;
using HarmonyLib;
using UnityEngine;

namespace GorillaShop.Patches
{
    [HarmonyPatch(typeof(CosmeticsController))]
    [HarmonyPatch("PurchaseItem", MethodType.Normal)]
    class BuyItemPatch
    {
        private static void Postfix(CosmeticsController __instance)
        {
            foreach (GameObject g in ShopManager.Instance.shopitems)
            {
                if (g.GetComponent<ShopManager.ShopItem>() != null)
                {
                    if (g.GetComponent<ShopManager.ShopItem>().Item.itemName == __instance.itemToBuy.itemName)
                    {
                        ShopManager.Instance.Canbuy.Remove(g.GetComponent<ShopManager.ShopItem>().Item);
                        ShopManager.Instance.allcost = (int.Parse(ShopManager.Instance.totalall.text) - __instance.itemToBuy.cost);
                        ShopManager.Instance.ammount.text = (int.Parse(ShopManager.Instance.ammount.text) - 1).ToString();
                        GameObject.Destroy(g);
                    }
                }
            }
        }
    }
}
