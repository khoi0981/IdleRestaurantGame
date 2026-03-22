using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Camera mainCam;

    void Start()
    {
        // Tự động tìm Camera chính trong game
        mainCam = Camera.main;
    }

    void LateUpdate()
    {
        if (mainCam != null)
        {
            // Ép góc xoay của bong bóng luôn bằng với góc xoay của Camera
            transform.rotation = mainCam.transform.rotation;
        }
    }
}