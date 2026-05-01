using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Quản lý UpgradeMenu - hiển thị thông tin nâng cấp Staff
/// Hiển thị: Service Speed Level + Speed Value + Chi phí
///           Hire Staff Level + Chi phí
/// </summary>
public class StaffUpgradeMenuManager : MonoBehaviour
{
    [Header("Serving Speed Components")]
    [SerializeField] private TextMeshProUGUI servingSpeedLevelText;      // Level hiện tại (ví dụ: Level 1)
    [SerializeField] private TextMeshProUGUI servingSpeedValueText;      // Giá trị speed (ví dụ: 1.0 Spd)
    [SerializeField] private TextMeshProUGUI servingSpeedCostText;       // Chi phí nâng cấp
    [SerializeField] private Button servingSpeedUpgradeButton;           // Button nâng cấp

    [Header("Hire Staff Components")]
    [SerializeField] private TextMeshProUGUI hireStaffLevelText;         // Level hiện tại (ví dụ: Level 1)
    [SerializeField] private TextMeshProUGUI hireStaffCostText;          // Chi phí nâng cấp
    [SerializeField] private Button hireStaffUpgradeButton;              // Button nâng cấp

    [Header("Thông tin chung")]
    [SerializeField] private TextMeshProUGUI playerMoneyText;            // Số tiền hiện có
    [SerializeField] private TextMeshProUGUI totalStaffText;             // Tổng nhân viên được tuyển dụng

    [Header("Cấu hình")]
    [SerializeField] private StaffUpgradeConfig staffUpgradeConfig;      // Cấu hình giá

    private void Start()
    {
        // Tự động tìm các components nếu chưa được gán
        FindComponentsInChildren();

        // Gán các event cho buttons
        if (servingSpeedUpgradeButton != null)
            servingSpeedUpgradeButton.onClick.AddListener(OnUpgradeServingSpeed);

        if (hireStaffUpgradeButton != null)
            hireStaffUpgradeButton.onClick.AddListener(OnUpgradeHireStaff);

        // Hiển thị thông tin ban đầu
        RefreshDisplay();
    }

    /// <summary>
    /// Tự động tìm các components từ UI nếu chưa được gán
    /// </summary>
    private void FindComponentsInChildren()
    {
        if (servingSpeedLevelText == null || servingSpeedValueText == null || servingSpeedCostText == null)
        {
            TextMeshProUGUI[] allTexts = GetComponentsInChildren<TextMeshProUGUI>();

            foreach (var text in allTexts)
            {
                string name = text.name.ToLower();
                string parentName = text.transform.parent?.name.ToLower() ?? "";

                // Tìm Serving Speed components
                if ((name.Contains("level") || parentName.Contains("level")) && 
                    parentName.Contains("serving") && servingSpeedLevelText == null)
                {
                    servingSpeedLevelText = text;
                    Debug.Log($"[StaffUpgradeMenuManager] ✓ Found servingSpeedLevelText");
                }
                else if ((name.Contains("speed") || name.Contains("spd")) && 
                         parentName.Contains("serving") && servingSpeedValueText == null)
                {
                    servingSpeedValueText = text;
                    Debug.Log($"[StaffUpgradeMenuManager] ✓ Found servingSpeedValueText");
                }
                else if ((name.Contains("cost") || name.Contains("money")) && 
                         parentName.Contains("serving") && servingSpeedCostText == null)
                {
                    servingSpeedCostText = text;
                    Debug.Log($"[StaffUpgradeMenuManager] ✓ Found servingSpeedCostText");
                }

                // Tìm Hire Staff components
                else if ((name.Contains("level") || parentName.Contains("level")) && 
                         parentName.Contains("hire") && hireStaffLevelText == null)
                {
                    hireStaffLevelText = text;
                    Debug.Log($"[StaffUpgradeMenuManager] ✓ Found hireStaffLevelText");
                }
                else if ((name.Contains("cost") || name.Contains("money")) && 
                         parentName.Contains("hire") && hireStaffCostText == null)
                {
                    hireStaffCostText = text;
                    Debug.Log($"[StaffUpgradeMenuManager] ✓ Found hireStaffCostText");
                }

                // Tìm Money & Total Staff
                else if (name.Contains("money") && playerMoneyText == null)
                {
                    playerMoneyText = text;
                    Debug.Log($"[StaffUpgradeMenuManager] ✓ Found playerMoneyText");
                }
                else if (name.Contains("total") && name.Contains("staff") && totalStaffText == null)
                {
                    totalStaffText = text;
                    Debug.Log($"[StaffUpgradeMenuManager] ✓ Found totalStaffText");
                }
            }
        }

        // Tìm buttons
        if (servingSpeedUpgradeButton == null || hireStaffUpgradeButton == null)
        {
            Button[] allButtons = GetComponentsInChildren<Button>();

            foreach (var button in allButtons)
            {
                string name = button.name.ToLower();
                string parentName = button.transform.parent?.name.ToLower() ?? "";

                if (parentName.Contains("serving") && servingSpeedUpgradeButton == null)
                {
                    servingSpeedUpgradeButton = button;
                    Debug.Log($"[StaffUpgradeMenuManager] ✓ Found servingSpeedUpgradeButton");
                }
                else if (parentName.Contains("hire") && hireStaffUpgradeButton == null)
                {
                    hireStaffUpgradeButton = button;
                    Debug.Log($"[StaffUpgradeMenuManager] ✓ Found hireStaffUpgradeButton");
                }
            }
        }
    }

    /// <summary>
    /// Cập nhật toàn bộ hiển thị
    /// </summary>
    public void RefreshDisplay()
    {
        if (StaffUpgradeManager.Instance == null)
        {
            Debug.LogError("[StaffUpgradeMenuManager] StaffUpgradeManager không tồn tại!");
            return;
        }

        // Hiển thị Service Speed
        DisplayServiceSpeed();

        // Hiển thị Hire Staff
        DisplayHireStaff();

        // Hiển thị Tiền và Tổng staff
        DisplayPlayerInfo();
    }

    /// <summary>
    /// Hiển thị thông tin Service Speed
    /// </summary>
    private void DisplayServiceSpeed()
    {
        int currentLevel = StaffUpgradeManager.Instance.GetServiceSpeedLevel();
        float speedValue = StaffUpgradeManager.Instance.GetCurrentServiceSpeedValue();
        int nextCost = StaffUpgradeManager.Instance.GetNextServiceSpeedCost();
        bool isMaxLevel = StaffUpgradeManager.Instance.IsServiceSpeedMaxLevel();

        // Hiển thị cấp độ
        if (servingSpeedLevelText != null)
        {
            servingSpeedLevelText.text = $"Level {currentLevel}";
        }

        // Hiển thị speed value
        if (servingSpeedValueText != null)
        {
            servingSpeedValueText.text = $"{speedValue:F1} Spd";
        }

        // Hiển thị chi phí
        if (servingSpeedCostText != null)
        {
            if (isMaxLevel)
            {
                servingSpeedCostText.text = "MAX";
                servingSpeedCostText.color = Color.gray;
            }
            else
            {
                int playerMoney = DataManager.Instance.totalMoney;
                bool canAfford = playerMoney >= nextCost;

                servingSpeedCostText.text = $"{nextCost}";
                servingSpeedCostText.color = canAfford ? Color.white : Color.red;
            }
        }

        // Cập nhật trạng thái button
        if (servingSpeedUpgradeButton != null)
        {
            bool canUpgrade = !isMaxLevel && DataManager.Instance.totalMoney >= nextCost;
            servingSpeedUpgradeButton.interactable = canUpgrade;
        }

        Debug.Log($"[StaffUpgradeMenuManager] Service Speed: Level {currentLevel}, Speed {speedValue:F1}, Cost {nextCost}");
    }

    /// <summary>
    /// Hiển thị thông tin Hire Staff
    /// </summary>
    private void DisplayHireStaff()
    {
        int currentLevel = StaffUpgradeManager.Instance.GetHireSpeedLevel();
        int nextCost = StaffUpgradeManager.Instance.GetNextHireSpeedCost();
        bool isMaxLevel = StaffUpgradeManager.Instance.IsHireSpeedMaxLevel();

        // Hiển thị cấp độ
        if (hireStaffLevelText != null)
        {
            hireStaffLevelText.text = $"{currentLevel}/3";
        }

        // Hiển thị chi phí
        if (hireStaffCostText != null)
        {
            if (isMaxLevel)
            {
                hireStaffCostText.text = "MAX";
                hireStaffCostText.color = Color.gray;
            }
            else
            {
                int playerMoney = DataManager.Instance.totalMoney;
                bool canAfford = playerMoney >= nextCost;

                hireStaffCostText.text = $"{nextCost}";
                hireStaffCostText.color = canAfford ? Color.white : Color.red;
            }
        }

        // Cập nhật trạng thái button
        if (hireStaffUpgradeButton != null)
        {
            bool canUpgrade = !isMaxLevel && DataManager.Instance.totalMoney >= nextCost;
            hireStaffUpgradeButton.interactable = canUpgrade;
        }

        Debug.Log($"[StaffUpgradeMenuManager] Hire Staff: Level {currentLevel}, Cost {nextCost}");
    }

    /// <summary>
    /// Hiển thị tiền và tổng nhân viên
    /// </summary>
    private void DisplayPlayerInfo()
    {
        int playerMoney = DataManager.Instance.totalMoney;
        int totalStaff = StaffUpgradeManager.Instance.GetTotalStaffHired();

        if (playerMoneyText != null)
        {
            playerMoneyText.text = playerMoney.ToString();
        }

        if (totalStaffText != null)
        {
            totalStaffText.text = $"{totalStaff}";
        }

        Debug.Log($"[StaffUpgradeMenuManager] Money: {playerMoney}, Total Staff: {totalStaff}");
    }

    /// <summary>
    /// Xử lý khi nhấn button nâng cấp Service Speed
    /// </summary>
    public void OnUpgradeServingSpeed()
    {
        Debug.Log("[StaffUpgradeMenuManager] Upgrading Service Speed...");
        
        if (StaffUpgradeManager.Instance.UpgradeServiceSpeed())
        {
            Debug.Log("[StaffUpgradeMenuManager] ✓ Service Speed upgraded!");
            RefreshDisplay();
        }
        else
        {
            Debug.LogWarning("[StaffUpgradeMenuManager] ✗ Failed to upgrade Service Speed!");
        }
    }

    /// <summary>
    /// Xử lý khi nhấn button nâng cấp Hire Staff
    /// </summary>
    public void OnUpgradeHireStaff()
    {
        Debug.Log("[StaffUpgradeMenuManager] Upgrading Hire Staff...");
        
        if (StaffUpgradeManager.Instance.UpgradeHireSpeed())
        {
            Debug.Log("[StaffUpgradeMenuManager] ✓ Hire Staff upgraded!");
            RefreshDisplay();
        }
        else
        {
            Debug.LogWarning("[StaffUpgradeMenuManager] ✗ Failed to upgrade Hire Staff!");
        }
    }

    /// <summary>
    /// Method để kompatible với MultiCameraSwitcher (cho Table Upgrade)
    /// </summary>
    public void SetTableAndDisplay(TableManager tableManager)
    {
        // Stub implementation - không cần vì đây là Staff Upgrade Manager
        Debug.Log("[StaffUpgradeMenuManager] SetTableAndDisplay called - hiển thị Staff Upgrade UI");
        RefreshDisplay();
    }
}
