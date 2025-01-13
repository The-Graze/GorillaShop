using System;
using System.Collections.Generic;
using System.Text;
using GorillaNetworking;
using HarmonyLib;
namespace GorillaShop
{
    internal class CosStartPatch
    {
        [HarmonyPatch(typeof(CosmeticsController))]
        [HarmonyPatch("InitializeCosmeticStands", MethodType.Normal)]

        static void Postfix()
        {
        }
    }
}
