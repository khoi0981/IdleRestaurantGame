using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BGM_Manager : MonoBehaviour
{
    private static BGM_Manager BGM_manage;

    void Awake()
    {
        if (BGM_manage == null)
        {
            BGM_manage = this;
            // ✅ FIX: Đảm bảo gọi DontDestroyOnLoad trên GameObject gốc (root)
            // Nếu GameObject này là child, hãy di chuyển nó lên top-level trước
            if (transform.parent != null)
            {
                transform.SetParent(null);
                Debug.Log("⚠️ BGM_Manager được di chuyển lên root level");
            }
            DontDestroyOnLoad(this.gameObject);
            Debug.Log("✅ BGM_Manager đã được set DontDestroyOnLoad");
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
}
