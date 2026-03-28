using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Quản lý UI menu cho mỗi camera view
/// Cung cấp các nút để đóng menu và quay lại camera mặc định
/// </summary>
public class CameraViewMenu : MonoBehaviour
{
    [Header("Tham chiếu")]
    [Tooltip("Script MultiCameraSwitcher để điều khiển camera")]
    public MultiCameraSwitcher cameraSwitcher;
    
    [Header("UI Menu")]
    [Tooltip("Nút để đóng menu và quay lại camera mặc định")]
    public Button closeButton;
    
    [Tooltip("Nút upgrades hoặc nút hành động chính (tùy chọn)")]
    public Button primaryActionButton;
    
    [Header("Cấu hình")]
    [Tooltip("Kích hoạt menu khi script start (không cần nếu được điều khiển bởi cameraSwitcher)")]
    public bool activateOnStart = false;

    private CanvasGroup canvasGroup;

    private void Start()
    {
        // Thiết lập canvas group nếu có để dễ dàng fade in/out
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        // Liên kết sự kiện nút
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(OnCloseButtonClicked);
        }
        else
        {
            Debug.LogWarning($"[CameraViewMenu] Close button không được gán cho menu: {gameObject.name}");
        }

        if (primaryActionButton != null)
        {
            // Bạn có thể đặt các sự kiện khác cho nút này
            primaryActionButton.onClick.AddListener(OnPrimaryActionClicked);
        }

        // Ẩn menu khi khởi động nếu không được yêu cầu hiện
        if (!activateOnStart)
        {
            gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Gọi khi nút đóng được nhấp
    /// </summary>
    private void OnCloseButtonClicked()
    {
        Debug.Log($"[CameraViewMenu] Đóng menu: {gameObject.name}");
        
        if (cameraSwitcher != null)
        {
            cameraSwitcher.ReturnToDefaultCamera();
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Gọi khi nút hành động chính được nhấp
    /// </summary>
    private void OnPrimaryActionClicked()
    {
        Debug.Log($"[CameraViewMenu] Primary action clicked: {gameObject.name}");
        // Thêm logic của bạn cho hành động chính ở đây
    }

    /// <summary>
    /// Hiệu ứng fade in menu (yêu cầu LeanTween nếu muốn hiệu ứng mềm)
    /// </summary>
    public void FadeIn(float duration = 0.3f)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            // Nếu có LeanTween, có thể dùng:
            // LeanTween.alphaCanvas(canvasGroup, 1f, duration);
        }
    }

    /// <summary>
    /// Hiệu ứng fade out menu (yêu cầu LeanTween nếu muốn hiệu ứng mềm)
    /// </summary>
    public void FadeOut(float duration = 0.3f)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            // Nếu có LeanTween, có thể dùng:
            // LeanTween.alphaCanvas(canvasGroup, 0f, duration);
        }
    }

    /// <summary>
    /// Lấy tham chiếu đến cameraSwitcher (hữu ích nếu được gán lúc runtime)
    /// </summary>
    public void SetCameraSwitcher(MultiCameraSwitcher switcher)
    {
        cameraSwitcher = switcher;
    }
}
