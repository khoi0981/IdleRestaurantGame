using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Helper script cho các nút chung trong UI menu của camera views
/// Cung cấp các phương thức hữu ích để tương tác với hệ thống
/// </summary>
public class CameraMenuActions : MonoBehaviour
{
    [Header("Tham chiếu")]
    [Tooltip("Script MultiCameraSwitcher")]
    public MultiCameraSwitcher cameraSwitcher;
    
    [Tooltip("Object hiện tại đang tương tác (ví dụ: bàn, máy nấu)")]
    public GameObject currentInteractableObject;
    
    [Header("Nút Hành Động")]
    public Button upgradeButton;
    public Button confirmButton;
    public Button cancelButton;
    
    [Header("Text Hiển Thị")]
    [SerializeField] private Text titleText;
    [SerializeField] private Text descriptionText;
    [SerializeField] private Text infoText;

    private void Start()
    {
        // Liên kết các nút
        if (upgradeButton != null)
            upgradeButton.onClick.AddListener(OnUpgradeClicked);
        
        if (confirmButton != null)
            confirmButton.onClick.AddListener(OnConfirmClicked);
        
        if (cancelButton != null)
            cancelButton.onClick.AddListener(OnCancelClicked);
    }

    /// <summary>
    /// Gọi khi nút Upgrade được nhấp
    /// </summary>
    private void OnUpgradeClicked()
    {
        if (currentInteractableObject == null)
        {
            Debug.LogWarning("[CameraMenuActions] Không có đối tượng tương tác!");
            return;
        }

        Debug.Log($"[CameraMenuActions] Nâng cấp: {currentInteractableObject.name}");
        
        // TODO: Thêm logic nâng cấp của bạn ở đây
        // Ví dụ: Gọi mở UI nâng cấp chi tiết
    }

    /// <summary>
    /// Gọi khi nút Confirm được nhấp
    /// </summary>
    private void OnConfirmClicked()
    {
        if (currentInteractableObject == null)
        {
            Debug.LogWarning("[CameraMenuActions] Không có đối tượng tương tác!");
            return;
        }

        Debug.Log($"[CameraMenuActions] Xác nhận: {currentInteractableObject.name}");
        
        // TODO: Thêm logic xác nhận của bạn ở đây
    }

    /// <summary>
    /// Gọi khi nút Cancel/Close được nhấp
    /// </summary>
    private void OnCancelClicked()
    {
        Debug.Log("[CameraMenuActions] Rời khỏi menu");
        
        if (cameraSwitcher != null)
        {
            cameraSwitcher.ReturnToDefaultCamera();
        }
    }

    /// <summary>
    /// Cập nhật text hiển thị
    /// </summary>
    public void UpdateMenuDisplay(string title, string description, string info = "")
    {
        if (titleText != null)
            titleText.text = title;
        
        if (descriptionText != null)
            descriptionText.text = description;
        
        if (infoText != null)
            infoText.text = info;
    }

    /// <summary>
    /// Thiết lập đối tượng tương tác hiện tại
    /// </summary>
    public void SetCurrentInteractable(GameObject obj)
    {
        currentInteractableObject = obj;
        Debug.Log($"[CameraMenuActions] Đặt đối tượng tương tác: {obj.name}");
    }

    /// <summary>
    /// Lấy đối tượng tương tác hiện tại
    /// </summary>
    public GameObject GetCurrentInteractable()
    {
        return currentInteractableObject;
    }

    /// <summary>
    /// Kích hoạt/Tắt các nút
    /// </summary>
    public void SetButtonsInteractable(bool interactable)
    {
        if (upgradeButton != null)
            upgradeButton.interactable = interactable;
        
        if (confirmButton != null)
            confirmButton.interactable = interactable;
        
        if (cancelButton != null)
            cancelButton.interactable = interactable;
    }

    /// <summary>
    /// Hiển thị/Ẩn menu
    /// </summary>
    public void ShowMenu(bool show)
    {
        gameObject.SetActive(show);
    }

    /// <summary>
    /// Lấy tham chiếu đến cameraSwitcher (nếu chưa gán)
    /// </summary>
    public void SetCameraSwitcher(MultiCameraSwitcher switcher)
    {
        cameraSwitcher = switcher;
    }
}
