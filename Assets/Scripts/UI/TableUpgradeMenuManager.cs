using UnityEngine;
using TMPro;

/// <summary>
/// Quản lý UpgradeMenu - hiển thị thông tin nâng cấp của bàn
/// </summary>
public class TableUpgradeMenuManager : MonoBehaviour
{
    [Header("Tham chiếu UpgradeMenu Components")]
    [SerializeField] private TextMeshProUGUI tableNameText;      // Tên bàn
    [SerializeField] private TextMeshProUGUI levelText;           // Level hiện tại
    [SerializeField] private TextMeshProUGUI upgradeCostText;     // Chi phí nâng cấp
    [SerializeField] private TextMeshProUGUI moneyText;           // Số tiền hiện có
    
    [Header("Cấu hình")]
    [SerializeField] private TableUpgradeConfig priceConfig;      // Cấu hình giá nâng cấp
    
    private TableManager currentTableManager;
    private string currentTableID;

    private void Start()
    {
        // Tự động tìm các Text components nếu chưa được gán
        FindTextComponents();
    }

    /// <summary>
    /// Tự động tìm các Text components từ UpgradeMenu nếu chưa được gán
    /// </summary>
    private void FindTextComponents()
    {
        if (tableNameText == null || levelText == null || upgradeCostText == null || moneyText == null)
        {
            TextMeshProUGUI[] allTexts = GetComponentsInChildren<TextMeshProUGUI>();
            
            Debug.Log($"[UpgradeMenuManager] Tìm thấy {allTexts.Length} Text components");
            
            foreach (var text in allTexts)
            {
                string path = GetHierarchyPath(text.transform);
                Debug.Log($"  - {path}: {text.text}");
                
                // Gán theo tên hoặc vị trí parent
                if (text.transform.parent != null && text.transform.parent.name == "Image" && tableNameText == null)
                {
                    tableNameText = text;
                    Debug.Log($"[UpgradeMenuManager] ✓ Gán tableNameText từ {path}");
                }
                else if (text.name.Contains("money") && moneyText == null)
                {
                    moneyText = text;
                    Debug.Log($"[UpgradeMenuManager] ✓ Gán moneyText từ {path}");
                }
                else if (text.transform.parent != null && text.transform.parent.name == "SeatIcon" && text.transform.childCount == 0 && levelText == null)
                {
                    levelText = text;
                    Debug.Log($"[UpgradeMenuManager] ✓ Gán levelText từ {path}");
                }
                else if (text.transform.parent != null && text.transform.parent.name == "Text (TMP)" && upgradeCostText == null)
                {
                    upgradeCostText = text;
                    Debug.Log($"[UpgradeMenuManager] ✓ Gán upgradeCostText từ {path}");
                }
            }
        }
    }

    /// <summary>
    /// Lấy đường dẫn phân cấp của một Transform
    /// </summary>
    private string GetHierarchyPath(Transform t)
    {
        string path = t.name;
        Transform parent = t.parent;
        
        while (parent != null)
        {
            if (parent.name == "UpgradeMenu")
                break;
                
            path = parent.name + "/" + path;
            parent = parent.parent;
        }
        return path;
    }

    /// <summary>
    /// Gọi khi nhấn vào một bàn
    /// </summary>
    public void SetTableAndDisplay(TableManager tableManager)
    {
        if (tableManager == null)
        {
            Debug.LogError("[UpgradeMenuManager] TableManager là null!");
            return;
        }

        currentTableManager = tableManager;
        currentTableID = tableManager.myTableID;
        
        // Tự động tìm các Text components nếu chưa gán
        if (tableNameText == null || levelText == null || upgradeCostText == null || moneyText == null)
        {
            FindTextComponents();
        }

        DisplayTableInfo();
    }

    /// <summary>
    /// Gọi khi nhấn vào một bàn (dùng GameObject và tìm TableManager)
    /// </summary>
    public void SetTableByGameObject(GameObject tableObject)
    {
        if (tableObject == null)
        {
            Debug.LogError("[UpgradeMenuManager] GameObject là null!");
            return;
        }

        TableManager tableManager = tableObject.GetComponent<TableManager>();
        if (tableManager == null)
        {
            Debug.LogError($"[UpgradeMenuManager] {tableObject.name} không có TableManager component!");
            return;
        }

        SetTableAndDisplay(tableManager);
    }

    /// <summary>
    /// Hiển thị thông tin bàn từ PlayerPrefs
    /// </summary>
    private void DisplayTableInfo()
    {
        if (currentTableManager == null)
        {
            Debug.LogWarning("[UpgradeMenuManager] Không có bàn được chọn!");
            return;
        }

        // Lấy dữ liệu bàn từ PlayerPrefs qua DataManager
        DataManager.TableData tableData = DataManager.Instance.LoadTable(currentTableID);
        
        if (string.IsNullOrEmpty(tableData.idTable))
        {
            Debug.LogWarning($"[UpgradeMenuManager] Không tìm thấy dữ liệu cho bàn {currentTableID}");
            return;
        }

        int currentLevel = tableData.currentLevel;
        int nextLevel = currentLevel + 1;
        
        // Lấy giá nâng cấp từ Table_Price_Config
        int upgradeCost = 999999;
        if (priceConfig != null)
        {
            upgradeCost = priceConfig.GetCostForLevel(nextLevel);
        }
        else if (currentTableManager.priceConfig != null)
        {
            upgradeCost = currentTableManager.priceConfig.GetCostForLevel(nextLevel);
        }

        int playerMoney = DataManager.Instance.totalMoney;

        // Hiển thị tên bàn
        if (tableNameText != null)
        {
            tableNameText.text = $" {currentTableID}";
            Debug.Log($"[UpgradeMenuManager] Set tableNameText: Bàn {currentTableID}");
        }

        // Hiển thị level hiện tại
        if (levelText != null)
        {
            levelText.text = $"Level {currentLevel}";
            Debug.Log($"[UpgradeMenuManager] Set levelText: Level {currentLevel}");
        }

        // Hiển thị tiền hiện có
        if (moneyText != null)
        {
            moneyText.text = playerMoney.ToString();
            Debug.Log($"[UpgradeMenuManager] Set moneyText: {playerMoney}");
        }

        // Hiển thị chi phí nâng cấp
        if (upgradeCostText != null)
        {
            if (upgradeCost == 999999)
            {
                upgradeCostText.text = "MAX";
                Debug.Log($"[UpgradeMenuManager] Set upgradeCostText: Đã đạt cấp độ tối đa!");
            }
            else
            {
                bool canAfford = playerMoney >= upgradeCost;
                upgradeCostText.text = $"{upgradeCost}";
                
                
                Debug.Log($"[UpgradeMenuManager] Set upgradeCostText: {upgradeCost} (Can afford: {canAfford})");
            }
        }

        Debug.Log($"[UpgradeMenuManager] ✓ Đã hiển thị thông tin bàn {currentTableID} Level {currentLevel}");
    }

    /// <summary>
    /// Nâng cấp bàn hiện tại
    /// </summary>
    public void UpgradeCurrentTable()
    {
        if (currentTableManager == null)
        {
            Debug.LogWarning("[UpgradeMenuManager] Không có bàn được chọn!");
            return;
        }

        // Gán priceConfig vào TableManager nếu chưa có
        if (currentTableManager.priceConfig == null)
        {
            if (priceConfig != null)
            {
                currentTableManager.priceConfig = priceConfig;
                Debug.Log($"[UpgradeMenuManager] Đã gán priceConfig vào TableManager {currentTableID}");
            }
            else
            {
                Debug.LogError("[UpgradeMenuManager] Price Config chưa được gán!");
                return;
            }
        }

        currentTableManager.UpgradeTable();
        
        // Cập nhật lại hiển thị
        DisplayTableInfo();
    }

    /// <summary>
    /// Lấy bàn hiện tại đang hiển thị
    /// </summary>
    public TableManager GetCurrentTable()
    {
        return currentTableManager;
    }

    /// <summary>
    /// Lấy ID bàn hiện tại
    /// </summary>
    public string GetCurrentTableID()
    {
        return currentTableID;
    }
}
