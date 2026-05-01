using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Camera mainCam;
    private bool shouldRotate = true;

    void Start()
    {
        // Tự động tìm Camera chính trong game
        mainCam = Camera.main;
        
        if (mainCam == null)
        {
            Debug.LogWarning("⚠️ Billboard: Camera.main không tìm thấy! Billboard won't work.");
            shouldRotate = false;
        }
        else
        {
            Debug.Log($"✅ Billboard: Kết nối với Camera: {mainCam.gameObject.name}");
        }
    }

    void LateUpdate()
    {
        if (mainCam != null && shouldRotate)
        {
            // ✨ Ép góc xoay của bong bóp luôn bằng với góc xoay của Camera (Billboard effect)
            // Điều này làm cho bubble luôn hướng về phía người chơi
            transform.rotation = mainCam.transform.rotation;
        }
    }

    /// <summary>
    /// Enable/Disable billboard rotation
    /// </summary>
    public void SetRotationEnabled(bool enabled)
    {
        shouldRotate = enabled;
    }

    /// <summary>
    /// Kiểm tra xem Billboard có hoạt động không
    /// </summary>
    public bool IsRotating()
    {
        return shouldRotate && mainCam != null;
    }
}