using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UITimer : MonoBehaviour
{
    [SerializeField] private Image clockFilled;

    private void Awake()
    {
        // ✅ Auto-discover Image component nếu chưa assign
        if (clockFilled == null)
        {
            // Cách 1: Tìm trong children trước (bao gồm nested children)
            clockFilled = GetComponentInChildren<Image>();
            
            // Cách 2: Nếu vẫn không tìm thấy, tìm theo tên "CountDown"
            if (clockFilled == null)
            {
                Transform countDownTrans = transform.Find("CountDown");
                if (countDownTrans != null)
                {
                    clockFilled = countDownTrans.GetComponent<Image>();
                }
            }

            // Cách 3: Nếu vẫn không tìm, recursive search
            if (clockFilled == null)
            {
                clockFilled = FindImageInChildren(transform, "CountDown");
            }

            // Nếu tìm thấy, log thông báo
            if (clockFilled != null)
            {
                Debug.Log($"✅ UITimer tự động tìm thấy Image component: {clockFilled.gameObject.name}");
            }
            else
            {
                Debug.LogWarning("⚠️ UITimer không tìm thấy Image component! Hộ assign trong Inspector hoặc đảm bảo có CountDown child object.");
            }
        }
    }

    /// <summary>
    /// Tìm Image component trong nested children
    /// </summary>
    private Image FindImageInChildren(Transform parent, string targetName)
    {
        foreach (Transform child in parent.GetComponentsInChildren<Transform>())
        {
            if (child.name == targetName)
            {
                Image img = child.GetComponent<Image>();
                if (img != null)
                {
                    return img;
                }
            }
        }
        return null;
    }

    public void UpdateClock(float amount, float maxValue)
    {
        if (clockFilled != null && maxValue > 0)
        {
            clockFilled.fillAmount = amount / maxValue;
        }
        else
        {
            if (clockFilled == null)
                Debug.LogWarning("⚠️ clockFilled is null in UITimer!");
            if (maxValue <= 0)
                Debug.LogWarning($"⚠️ maxValue is invalid: {maxValue}");
        }
    }
}
