using UnityEngine;
using System;

/// <summary>
/// Quản lý logic nâng cấp nhân viên
/// </summary>
public class StaffUpgradeManager : MonoBehaviour
{
    public static StaffUpgradeManager Instance;

    [Header("Cấu hình")]
    [SerializeField] private StaffUpgradeConfig staffUpgradeConfig;

    private DataManager.StaffData currentStaffData;

    // ✅ Events để thông báo khi nâng cấp
    public static event Action OnServiceSpeedUpgraded;
    public static event Action OnHireStaffUpgraded;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadStaffData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Tải dữ liệu nhân viên từ DataManager
    /// </summary>
    private void LoadStaffData()
    {
        currentStaffData = DataManager.Instance.LoadStaffUpgrade();
        
        // ✅ FIX: Ensure totalStaffHired always matches levelHireSpeed
        // Nếu dữ liệu bị desync (vd: undo), tự động sync lại
        if (currentStaffData.totalStaffHired != currentStaffData.levelHireSpeed)
        {
            currentStaffData.totalStaffHired = currentStaffData.levelHireSpeed;
            DataManager.Instance.SaveStaffUpgrade(currentStaffData);
            Debug.Log($"[StaffUpgradeManager] ⚠️ Dữ liệu bị desync! Đã reset: levelHireSpeed={currentStaffData.levelHireSpeed}, totalStaffHired={currentStaffData.totalStaffHired}");
        }
        
        Debug.Log($"[StaffUpgradeManager] Đã tải StaffData: Service Speed Level {currentStaffData.levelServiceSpeed}, Hire Speed Level {currentStaffData.levelHireSpeed}, Total Staff {currentStaffData.totalStaffHired}");
    }

    /// <summary>
    /// Nâng cấp Service Speed
    /// </summary>
    public bool UpgradeServiceSpeed()
    {
        if (staffUpgradeConfig == null)
        {
            Debug.LogError("[StaffUpgradeManager] StaffUpgradeConfig chưa được gán!");
            return false;
        }

        // Kiểm tra đã max level chưa
        if (staffUpgradeConfig.IsServiceSpeedMaxLevel(currentStaffData.levelServiceSpeed))
        {
            Debug.LogWarning("[StaffUpgradeManager] Service Speed đã đạt cấp độ tối đa!");
            return false;
        }

        // Lấy chi phí
        int cost = staffUpgradeConfig.GetServiceSpeedCost(currentStaffData.levelServiceSpeed);

        // Kiểm tra tiền đủ không
        if (DataManager.Instance.totalMoney < cost)
        {
            Debug.LogWarning($"[StaffUpgradeManager] Tiền không đủ! Cần {cost}, có {DataManager.Instance.totalMoney}");
            return false;
        }

        // Trừ tiền
        DataManager.Instance.SubstractGold(cost);

        // Nâng cấp
        currentStaffData.levelServiceSpeed++;

        // Lưu lại
        DataManager.Instance.SaveStaffUpgrade(currentStaffData);

        Debug.Log($"[StaffUpgradeManager] ✓ Nâng cấp Service Speed lên Level {currentStaffData.levelServiceSpeed}. Chi phí: {cost}");

        // ✅ Gọi event để thông báo Staff cập nhật tốc độ
        OnServiceSpeedUpgraded?.Invoke();

        return true;
    }

    /// <summary>
    /// Nâng cấp Hire Speed
    /// </summary>
    public bool UpgradeHireSpeed()
    {
        if (staffUpgradeConfig == null)
        {
            Debug.LogError("[StaffUpgradeManager] StaffUpgradeConfig chưa được gán!");
            return false;
        }

        // Kiểm tra đã max level chưa
        if (staffUpgradeConfig.IsHireStaffMaxLevel(currentStaffData.levelHireSpeed))
        {
            Debug.LogWarning("[StaffUpgradeManager] Hire Speed đã đạt cấp độ tối đa!");
            return false;
        }

        // Lấy chi phí
        int cost = staffUpgradeConfig.GetHireStaffCost(currentStaffData.levelHireSpeed);

        // Kiểm tra tiền đủ không
        if (DataManager.Instance.totalMoney < cost)
        {
            Debug.LogWarning($"[StaffUpgradeManager] Tiền không đủ! Cần {cost}, có {DataManager.Instance.totalMoney}");
            return false;
        }

        // Trừ tiền
        DataManager.Instance.SubstractGold(cost);

        // Nâng cấp
        currentStaffData.levelHireSpeed++;
        // ✅ FIX: totalStaffHired = levelHireSpeed (direct assignment, not increment)
        currentStaffData.totalStaffHired = currentStaffData.levelHireSpeed;

        // Lưu lại
        DataManager.Instance.SaveStaffUpgrade(currentStaffData);

        Debug.Log($"[StaffUpgradeManager] ✓ Nâng cấp Hire Speed lên Level {currentStaffData.levelHireSpeed}. Tổng nhân viên: {currentStaffData.totalStaffHired}. Chi phí: {cost}");

        // ✅ Gọi event để thông báo Staff Manager spawn staff mới
        OnHireStaffUpgraded?.Invoke();

        return true;
    }

    /// <summary>
    /// Lấy cấp độ Service Speed hiện tại
    /// </summary>
    public int GetServiceSpeedLevel()
    {
        return currentStaffData.levelServiceSpeed;
    }

    /// <summary>
    /// Lấy cấp độ Hire Speed hiện tại
    /// </summary>
    public int GetHireSpeedLevel()
    {
        return currentStaffData.levelHireSpeed;
    }

    /// <summary>
    /// Lấy giá trị speed hiện tại
    /// </summary>
    public float GetCurrentServiceSpeedValue()
    {
        if (staffUpgradeConfig == null)
            return 1.0f;
        return staffUpgradeConfig.GetServiceSpeedValue(currentStaffData.levelServiceSpeed);
    }

    /// <summary>
    /// Lấy tổng số nhân viên được tuyển dụng
    /// </summary>
    public int GetTotalStaffHired()
    {
        return currentStaffData.totalStaffHired;
    }

    /// <summary>
    /// Lấy chi phí nâng cấp Service Speed tiếp theo
    /// </summary>
    public int GetNextServiceSpeedCost()
    {
        if (staffUpgradeConfig == null)
            return 0;
        return staffUpgradeConfig.GetServiceSpeedCost(currentStaffData.levelServiceSpeed);
    }

    /// <summary>
    /// Lấy chi phí nâng cấp Hire Speed tiếp theo
    /// </summary>
    public int GetNextHireSpeedCost()
    {
        if (staffUpgradeConfig == null)
            return 0;
        return staffUpgradeConfig.GetHireStaffCost(currentStaffData.levelHireSpeed);
    }

    /// <summary>
    /// Kiểm tra xem Service Speed đã max level chưa
    /// </summary>
    public bool IsServiceSpeedMaxLevel()
    {
        if (staffUpgradeConfig == null)
            return true;
        return staffUpgradeConfig.IsServiceSpeedMaxLevel(currentStaffData.levelServiceSpeed);
    }

    /// <summary>
    /// Kiểm tra xem Hire Speed đã max level chưa
    /// </summary>
    public bool IsHireSpeedMaxLevel()
    {
        if (staffUpgradeConfig == null)
            return true;
        return staffUpgradeConfig.IsHireStaffMaxLevel(currentStaffData.levelHireSpeed);
    }

    /// <summary>
    /// Lấy dữ liệu Staff hiện tại
    /// </summary>
    public DataManager.StaffData GetCurrentStaffData()
    {
        return currentStaffData;
    }

    /// <summary>
    /// Cập nhật dữ liệu Staff (dùng cho debug hoặc reset)
    /// </summary>
    public void SetStaffData(DataManager.StaffData newData)
    {
        currentStaffData = newData;
        DataManager.Instance.SaveStaffUpgrade(currentStaffData);
    }

    private void OnApplicationQuit()
    {
        if (currentStaffData != null)
        {
            DataManager.Instance.SaveStaffUpgrade(currentStaffData);
        }
    }
}
