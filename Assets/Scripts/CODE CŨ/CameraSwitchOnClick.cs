using Unity.Cinemachine;
using UnityEngine;

public class CameraSwitchOnClick : MonoBehaviour
{
    [Header("Cấu hình Camera")]
    public CinemachineCamera activeCam;
    public CinemachineCamera defaultCam;

    [Header("Đối tượng cần nhấp")]
    public LayerMask targetObjectLayer;
    public GameObject clickableObject;

    [Header("UI Menu")]
    public GameObject upgradeMenu;

    private void Update()
    {
        // Kiểm tra sự kiện nhấp chuột trái
        if (Input.GetMouseButtonDown(0))
        {
            // Bắn tia ray từ vị trí chuột
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Kiểm tra xem tia có chạm vào đối tượng mục tiêu không
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, targetObjectLayer))
            {
                // Kiểm tra xem đối tượng được nhấp có trùng với đối tượng đã đặt không
                if (hit.collider.gameObject == clickableObject)
                {
                    SwitchCamera();
                }
            }
        }
    }

    private void SwitchCamera()
    {
        // Vô hiệu hóa camera mặc định và kích hoạt camera được chọn
        defaultCam.gameObject.SetActive(false);
        activeCam.gameObject.SetActive(true);

        // Kích hoạt UpgradeMenu
        if (upgradeMenu != null)
        {
            upgradeMenu.SetActive(true);
        }

        // Hoặc bạn có thể sử dụng phương pháp điều chỉnh độ ưu tiên cho chuyển đổi mượt mà hơn
        // defaultCam.Priority = 10;
        // activeCam.Priority = 20;
    }
}