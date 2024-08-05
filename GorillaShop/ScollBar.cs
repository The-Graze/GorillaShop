using GorillaLocomotion;
using GorillaShop;
using System.Collections;
using System.ComponentModel;
using UnityEngine;

public class ScollBar : MonoBehaviour
{
    public bool up;
    private bool isScrolling = false;
    private float scrollDistance = 65f;
    private float scrollSpeed = 15f;

    public void ButtonActivation(bool left)
    {
        if (!isScrolling)
        {
            StartCoroutine(SmoothScroll(left));
        }
    }

    private IEnumerator SmoothScroll(bool left)
    {
        isScrolling = true;
        float targetY = ShopManager.Instance.StuffContainer.transform.localPosition.y + (up ? scrollDistance : -scrollDistance);

        while (Mathf.Abs(ShopManager.Instance.StuffContainer.transform.localPosition.y - targetY) > 0.01f)
        {
            Vector3 newPosition = ShopManager.Instance.StuffContainer.transform.localPosition;
            newPosition.y = Mathf.Lerp(newPosition.y, targetY, Time.deltaTime * scrollSpeed);
            ShopManager.Instance.StuffContainer.transform.localPosition = newPosition;
            yield return null;
        }

        ShopManager.Instance.StuffContainer.transform.localPosition = new Vector3(
            ShopManager.Instance.StuffContainer.transform.localPosition.x,
            targetY,
            ShopManager.Instance.StuffContainer.transform.localPosition.z
        );
        GorillaTagger.Instance.offlineVRRig.PlayHandTapLocal(211, left, 1);
        GorillaTagger.Instance.StartVibration(left, GorillaTagger.Instance.tapHapticStrength / 2f, GorillaTagger.Instance.tapHapticDuration);
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
