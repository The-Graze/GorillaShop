using System;
using System.Collections.Generic;
using System.Text;
using Photon.Pun;
using PlayFab.ClientModels;
using UnityEngine;
using UnityEngine.UI;

namespace GorillaShop
{
    public class ScollBar : MonoBehaviour
    {
        public bool up;
        public float debounceTime = 0.25f;

        public float touchTime;

        void Start()
        {
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
                ButtonActivation();
            }
        }
        public void ButtonActivation()
        {
            if (up)
            {
                ShopManager.Instance.StuffContainer.transform.localPosition = new Vector3(ShopManager.Instance.StuffContainer.transform.localPosition.x, ShopManager.Instance.StuffContainer.transform.localPosition.y + 300, ShopManager.Instance.StuffContainer.transform.localPosition.z);
            }
            else
            {
                ShopManager.Instance.StuffContainer.transform.localPosition = new Vector3(ShopManager.Instance.StuffContainer.transform.localPosition.x, ShopManager.Instance.StuffContainer.transform.localPosition.y - 300, ShopManager.Instance.StuffContainer.transform.localPosition.z);
            }
        }

    }
}
