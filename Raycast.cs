﻿using ExpandedShop;
using Harmony;
using HutongGames.PlayMaker;
using MSCLoader;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UniversalShoppingSystem
{
    public class ItemShopRaycast : MonoBehaviour
    {
        public List<ItemShop> Shops = new List<ItemShop>();

        private Camera fpsCam;
        private RaycastHit hit;

        private FsmBool _guiBuy;
        private FsmString _guiText;
        private FsmBool storeOpen;

        private bool cartIconShowing;

        private static readonly HarmonyInstance harmony = HarmonyInstance.Create("UniversalShoppingSystem");

        private IEnumerator PatchES()
        {
            GameObject.Find("STORE/TeimoDrinksMod(Clone)").GetComponent<ShopRaycast>().ApplyFsmBool = false;
            yield break;
        }
        private void Awake()
        {
            if (ModLoader.IsModPresent("ExpandedShop"))
            {
                StartCoroutine(PatchES());
                
                if (System.Convert.ToDecimal(ModLoader.GetMod("ExpandedShop").Version) < 1.1m)
                {
                    ModUI.ShowCustomMessage("You are using an old version of ExpandedShop which is not compatible with UniversalShoppingSystem. Please update or uninstall ExpandedShop.", "Wrong Version", new MsgBoxBtn[]
                {
                    ModUI.CreateMessageBoxBtn("I will", () => { }, false)
                });
                }
            }

            fpsCam = GameObject.Find("PLAYER").transform.Find("Pivot/AnimPivot/Camera/FPSCamera/FPSCamera").GetComponent<Camera>();
            _guiBuy = PlayMakerGlobals.Instance.Variables.FindFsmBool("GUIbuy");
            _guiText = PlayMakerGlobals.Instance.Variables.FindFsmString("GUIinteraction");
            storeOpen = GameObject.Find("STORE").GetPlayMaker("OpeningHours").FsmVariables.FindFsmBool("OpenStore");
        }

        private void Update()
        {
            bool lmb = Input.GetKeyDown(KeyCode.Mouse0);
            bool rmb = Input.GetKeyDown(KeyCode.Mouse1);

            Physics.Raycast(fpsCam.ScreenPointToRay(Input.mousePosition), out hit, 1.35f);

            if (hit.collider.gameObject.GetComponent<ItemShop>())
            {
                if (storeOpen.Value == true)
                {
                    ItemShop shop = hit.collider.gameObject.GetComponent<ItemShop>();
                    cartIconShowing = true;
                    _guiBuy.Value = true;
                    _guiText.Value = $"{shop.ItemName} {shop.ItemPrice} mk";

                    if (lmb && shop.Stock > 0) shop.Buy();
                    else if (rmb && shop.Cart > 0) shop.Unbuy();
                }
            }

            else if (cartIconShowing)
            {
                cartIconShowing = false;
                _guiBuy.Value = false;
                _guiText.Value = "";
            }
        }
    }
}