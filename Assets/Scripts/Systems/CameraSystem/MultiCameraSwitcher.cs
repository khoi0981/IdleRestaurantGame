using Unity.Cinemachine;
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class CameraViewConfig
{
    [Tooltip("Đối tượng để nhấp vào")]
    public GameObject clickableObject;
    
    [Tooltip("Camera khi nhấp vào đối tượng")]
    public CinemachineCamera targetCamera;
    
    [Tooltip("UI Menu hiển thị khi camera được kích hoạt")]
    public GameObject uiMenu;
    
    [Tooltip("Tên hiển thị cho view này (tùy chọn)")]
    public string viewName = "Unnamed View";
}

/// <summary>
/// Quản lý nhiều camera có thể chuyển đổi dựa trên các object được nhấp
/// Mỗi object có thể liên kết với một camera riêng và UI menu riêng
/// </summary>
public class MultiCameraSwitcher : MonoBehaviour
{
    [Header("Cấu hình Camera")]
    [Tooltip("Camera mặc định khi khởi động")]
    public CinemachineCamera defaultCamera;
    
    [Tooltip("Layer mask cho các đối tượng có thể nhấp")]
    public LayerMask targetObjectLayer;
    
    [Header("Quản lý UI")]
    [Tooltip("Script quản lý Table Upgrade Menu")]
    public TableUpgradeMenuManager upgradeMenuManager;
    
    [Header("Các View Camera")]
    [Tooltip("Danh sách tất cả các camera view có thể chuyển đổi")]
    [SerializeField] private List<CameraViewConfig> cameraViews = new List<CameraViewConfig>();
    
    [Header("Cấu hình")]
    [Tooltip("Thời gian chuyển đổi camera (có thể sử dụng với smooth transitions)")]
    public float switchDuration = 0.5f;
    
    [Tooltip("Có ẩn UI menu của view trước đó khi chuyển đổi?")]
    public bool hideOldMenuOnSwitch = true;
    
    private CinemachineCamera currentCamera;
    private GameObject currentMenu;
    private CameraViewConfig currentViewConfig;
    
    private Dictionary<GameObject, CameraViewConfig> objectToCameraMap;

    private void Start()
    {
        InitializeCameraMap();
        
        // Thiết lập camera mặc định
        if (defaultCamera != null)
        {
            currentCamera = defaultCamera;
            currentCamera.gameObject.SetActive(true);
        }
        
        Debug.Log($"[MultiCameraSwitcher] Đã khởi tạo với {cameraViews.Count} view camera");
    }

    private void InitializeCameraMap()
    {
        objectToCameraMap = new Dictionary<GameObject, CameraViewConfig>();
        
        foreach (var config in cameraViews)
        {
            if (config.clickableObject != null)
            {
                if (objectToCameraMap.ContainsKey(config.clickableObject))
                {
                    Debug.LogWarning($"[MultiCameraSwitcher] Đối tượng {config.clickableObject.name} được liên kết nhiều lần!");
                }
                else
                {
                    objectToCameraMap[config.clickableObject] = config;
                    Debug.Log($"[MultiCameraSwitcher] Thêm mapping: {config.clickableObject.name} -> {config.viewName}");
                }
            }
            else
            {
                Debug.LogWarning("[MultiCameraSwitcher] Một CameraViewConfig không có clickableObject được gán!");
            }
        }
    }

    private void Update()
    {
        if (MenuToggler.isOpen) return;
        HandleMouseClick();
    }

    private void HandleMouseClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, targetObjectLayer))
            {
                GameObject clickedObject = hit.collider.gameObject;
                Debug.Log($"[MultiCameraSwitcher] Hit object: {clickedObject.name}");
                
                if (objectToCameraMap.TryGetValue(clickedObject, out CameraViewConfig config))
                {
                    Debug.Log($"[MultiCameraSwitcher] Tìm thấy config, chuyển đổi sang: {config.viewName}");
                    SwitchToView(config);
                    
                    // Cập nhật UpgradeMenu nếu object này là bàn
                    if (upgradeMenuManager != null)
                    {
                        TableManager tableManager = clickedObject.GetComponent<TableManager>();
                        if (tableManager != null)
                        {
                            upgradeMenuManager.SetTableAndDisplay(tableManager);
                            Debug.Log($"[MultiCameraSwitcher] Đã cập nhật UpgradeMenu cho bàn {tableManager.myTableID}");
                        }
                    }
                }
                else
                {
                    Debug.LogWarning($"[MultiCameraSwitcher] Object {clickedObject.name} không trong bản đồ! Map có {objectToCameraMap.Count} entry");
                    foreach (var entry in objectToCameraMap)
                    {
                        Debug.Log($"  - {entry.Key.name} -> {entry.Value.viewName}");
                    }
                }
            }
            else
            {
                Debug.LogWarning("[MultiCameraSwitcher] Raycast không hit vào object nào");
            }
        }
    }

    /// <summary>
    /// Chuyển đổi sang một view camera cụ thể
    /// </summary>
    public void SwitchToView(CameraViewConfig config)
    {
        if (config == null)
        {
            Debug.LogError("[MultiCameraSwitcher] CameraViewConfig là null!");
            return;
        }

        if (config.targetCamera == null)
        {
            Debug.LogError("[MultiCameraSwitcher] Target camera là null trong config!");
            return;
        }

        // Ẩn menu và camera cũ
        if (hideOldMenuOnSwitch && currentMenu != null)
        {
            currentMenu.SetActive(false);
        }

        if (currentCamera != null && currentCamera != config.targetCamera)
        {
            currentCamera.gameObject.SetActive(false);
        }

        // Kích hoạt camera mới
        config.targetCamera.gameObject.SetActive(true);
        
        // Hiển thị UI menu mới
        if (config.uiMenu != null)
        {
            config.uiMenu.SetActive(true);
        }

        // Cập nhật trạng thái hiện tại
        currentCamera = config.targetCamera;
        currentMenu = config.uiMenu;
        currentViewConfig = config;
        
        Debug.Log($"[MultiCameraSwitcher] Đã chuyển đổi sang view: {config.viewName}");
    }

    /// <summary>
    /// Quay lại camera mặc định
    /// </summary>
    public void ReturnToDefaultCamera()
    {
        if (defaultCamera != null)
        {
            if (hideOldMenuOnSwitch && currentMenu != null)
            {
                currentMenu.SetActive(false);
            }

            if (currentCamera != defaultCamera)
            {
                currentCamera.gameObject.SetActive(false);
            }

            defaultCamera.gameObject.SetActive(true);
            currentCamera = defaultCamera;
            currentMenu = null;
            currentViewConfig = null;
            
            Debug.Log("[MultiCameraSwitcher] Đã quay lại camera mặc định");
        }
    }

    /// <summary>
    /// Lấy camera hiện tại
    /// </summary>
    public CinemachineCamera GetCurrentCamera()
    {
        return currentCamera;
    }

    /// <summary>
    /// Lấy menu UI hiện tại
    /// </summary>
    public GameObject GetCurrentMenu()
    {
        return currentMenu;
    }

    /// <summary>
    /// Thêm một view camera mới (có thể gọi từ lúc runtime)
    /// </summary>
    public void AddCameraView(GameObject clickableObject, CinemachineCamera camera, GameObject menu, string viewName = "")
    {
        if (clickableObject == null || camera == null)
        {
            Debug.LogError("[MultiCameraSwitcher] clickableObject hoặc camera không được null!");
            return;
        }

        var config = new CameraViewConfig
        {
            clickableObject = clickableObject,
            targetCamera = camera,
            uiMenu = menu,
            viewName = string.IsNullOrEmpty(viewName) ? clickableObject.name : viewName
        };

        cameraViews.Add(config);
        objectToCameraMap[clickableObject] = config;
        
        Debug.Log($"[MultiCameraSwitcher] Đã thêm camera view mới: {config.viewName}");
    }

    /// <summary>
    /// Xóa một view camera
    /// </summary>
    public void RemoveCameraView(GameObject clickableObject)
    {
        cameraViews.RemoveAll(c => c.clickableObject == clickableObject);
        objectToCameraMap.Remove(clickableObject);
        
        Debug.Log($"[MultiCameraSwitcher] Đã xóa camera view cho đối tượng: {clickableObject.name}");
    }

    /// <summary>
    /// Lấy tất cả camera views
    /// </summary>
    public List<CameraViewConfig> GetAllCameraViews()
    {
        return new List<CameraViewConfig>(cameraViews);
    }
}
