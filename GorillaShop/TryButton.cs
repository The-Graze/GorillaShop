using System;
using System.Collections.Generic;
using System.Text;
using GorillaLocomotion;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Events;

namespace GorillaShop
{
    public class TryButton : MonoBehaviour
    {
        public ShopManager.ShopItem Item;

        public float debounceTime = 0.25f;

        public float touchTime;

        public void Start()
        {
           GetComponent<Renderer>().enabled = false;
        }

        private void OnTriggerEnter(Collider collider)
        {
            if (!base.enabled || !(touchTime + debounceTime < Time.time) || collider.GetComponentInParent<GorillaTriggerColliderHandIndicator>() == null)
            {
                return;
            }

            touchTime = Time.time;
            GorillaTriggerColliderHandIndicator component = collider.GetComponent<GorillaTriggerColliderHandIndicator>();
            if (!(component == null))
            {
                Item.ButtonPress();
                GorillaTagger.Instance.offlineVRRig.PlayHandTapLocal(211, component.isLeftHand, 1);
                GorillaTagger.Instance.StartVibration(component.isLeftHand, GorillaTagger.Instance.tapHapticStrength / 2f, GorillaTagger.Instance.tapHapticDuration);
            }
        }
    }
}