using GorillaLocomotion;
using GorillaShop;
using System.Collections;
using UnityEngine;

namespace GorillaShop
{
    public class ScollBar : MonoBehaviour
    {
        public bool up;
        private bool isScrolling = false;
        private float scrollDistance = 65f;
        private float scrollSpeed = 15f;
        private float timeoutDuration = 5f;

        void Start()
        {
            foreach (Transform t in transform)
            {
                if (t.name != "Casual Button Text")
                {
                    Destroy(t.gameObject);
                }
            }
        }

        public void ButtonActivation(bool left)
        {
            if (!isScrolling)
            {
                StartCoroutine(SmoothScroll(up));
            }
        }

        IEnumerator SmoothScroll(bool scrollUp)
        {
            isScrolling = true;

            Transform stuffContainer = ShopManager.Instance.StuffContainer.transform;
            float startY = stuffContainer.localPosition.y;
            float targetY = startY + (scrollUp ? scrollDistance : -scrollDistance);

            float contentHeight = ShopManager.Instance.StuffContainer.GetComponent<RectTransform>().rect.height;
            float viewportHeight = ShopManager.Instance.StuffContainer.transform.parent.GetComponent<RectTransform>().rect.height;
            float minY = Mathf.Min(0, viewportHeight - contentHeight);
            float maxY = 0;

            targetY = Mathf.Clamp(targetY, minY, maxY);

            float elapsedTime = 0f;

            while (Mathf.Abs(stuffContainer.localPosition.y - targetY) > 0.01f && elapsedTime < timeoutDuration)
            {
                Vector3 currentPosition = stuffContainer.localPosition;
                currentPosition.y = Mathf.Lerp(currentPosition.y, targetY, Time.deltaTime * scrollSpeed);
                stuffContainer.localPosition = currentPosition;

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            stuffContainer.localPosition = new Vector3(
                stuffContainer.localPosition.x,
                Mathf.Clamp(targetY, minY, maxY),
                stuffContainer.localPosition.z
            );

            GorillaTagger.Instance.offlineVRRig.PlayHandTapLocal(211, scrollUp, 1);
            GorillaTagger.Instance.StartVibration(scrollUp, GorillaTagger.Instance.tapHapticStrength / 2f, GorillaTagger.Instance.tapHapticDuration);
            isScrolling = false;
        }

        private void OnTriggerStay(Collider other)
        {
            GorillaTriggerColliderHandIndicator component = other.GetComponent<GorillaTriggerColliderHandIndicator>();
            if (component != null)
            {
                ButtonActivation(component.isLeftHand);
            }
        }
    }
}
