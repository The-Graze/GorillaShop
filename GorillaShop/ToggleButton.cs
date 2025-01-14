using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace GorillaShop
{
    class ToggleButton : MonoBehaviour
    {

        public float debounceTime = 0.25f;

        public float touchTime;

        bool toggle;

        Text text;

        void Start()
        {
            foreach (Transform t in transform)
            {
                if (t.name != "Casual Button Text")
                {
                    Destroy(t.gameObject);
                }
                else
                {
                    text = t.GetComponent<Text>();
                    text.gameObject.SetActive(true);
                }
            }
            transform.localScale = new Vector3(50, 50, 50);
            transform.position = new Vector3(-52.2633f, 16.8036f, - 121.474f);
            transform.rotation = Quaternion.Euler(346.8203f, 41.924f, 0);
            ShopManager.Instance.Toggle(toggle);
            text.text = "SHOW";
        }
        private void OnTriggerEnter(Collider collider)
        {
            if (!base.enabled || !(touchTime + debounceTime < Time.time) || collider.GetComponentInParent<GorillaTriggerColliderHandIndicator>() == null)
            {
                return;
            }

            touchTime = Time.time;
            GorillaTriggerColliderHandIndicator component = collider.GetComponent<GorillaTriggerColliderHandIndicator>();
            if (component != null)
            {
                toggle = !toggle;
                ShopManager.Instance.Toggle(toggle);
                text.text = toggle ? "HIDE" : "SHOW";
                GorillaTagger.Instance.offlineVRRig.PlayHandTapLocal(211, component.isLeftHand, 1);
                GorillaTagger.Instance.StartVibration(component.isLeftHand, GorillaTagger.Instance.tapHapticStrength / 2f, GorillaTagger.Instance.tapHapticDuration);
            }
        }
    }
}
