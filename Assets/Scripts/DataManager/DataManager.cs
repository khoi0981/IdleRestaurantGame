using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class DataManager : MonoBehaviour
{

    public static DataManager Instance;

    [Header("Giao diện (UI)")]
    public TextMeshProUGUI coinText;

    [Header("Dữ liệu Người chơi")]
    public int totalMoney = 0;

    [System.Serializable]
    public class TableData
    {
        public string idTable;
        public bool isUnlocked;
        public int currentLevel;
    }

    [System.Serializable]
    public class StaffData
    {
        public int levelServiceSpeed = 1;      // Cấp độ tốc độ phục vụ
        public int levelHireSpeed = 1;         // Cấp độ tốc độ tuyển dụng
        public int totalStaffHired = 1;        // Tổng số nhân viên được tuyển dụng

        public StaffData()
        {
            levelServiceSpeed = 1;
            levelHireSpeed = 1;
            totalStaffHired = 1;
        }
    }

    [System.Serializable]
    public class FoodPurchaseData
    {
        public List<string> purchasedFoodIDs = new List<string>();

        public FoodPurchaseData()
        {
            purchasedFoodIDs = new List<string>();
        }
    }

    void Awake()
    {

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);


            LoadData();
        }
        else
        {
           
            Destroy(gameObject);
        }
    }

    
    public void UpdateUI()
    {
        if (coinText != null)
        {
            coinText.text = totalMoney.ToString();
        }
    }

 
    public void SaveData()
    {

        PlayerPrefs.SetInt("CoinMoney", totalMoney);

        PlayerPrefs.Save();

        UpdateUI();
    }

    public void LoadData()
    { 
        totalMoney = PlayerPrefs.GetInt("CoinMoney", 0);

        UpdateUI();
    }
    public void SaveTable(TableData data)
    {
        string json = JsonUtility.ToJson(data);

        PlayerPrefs.SetString("Table_" + data.idTable, json);
        PlayerPrefs.Save();
    }
    public TableData LoadTable(string id)
    {
        string key = "Table_" + id;
        if (PlayerPrefs.HasKey(key))
        {
            string json = PlayerPrefs.GetString(key);
            return JsonUtility.FromJson<TableData>(json);
        }
        return new TableData();
    }
    public void SubstractGold(int amount)
    {
        totalMoney -= amount;
        SaveData();  // Tự động lưu lại sau khi trừ tiền
        Debug.Log("Số tiền còn lại: " + totalMoney);
    }

    // ========== STAFF UPGRADE DATA ==========

    /// <summary>
    /// Lưu dữ liệu nâng cấp nhân viên vào PlayerPrefs
    /// </summary>
    public void SaveStaffUpgrade(StaffData data)
    {
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString("StaffUpgrade", json);
        PlayerPrefs.Save();
        Debug.Log("Đã lưu dữ liệu Staff Upgrade: " + json);
    }

    /// <summary>
    /// Tải dữ liệu nâng cấp nhân viên từ PlayerPrefs
    /// </summary>
    public StaffData LoadStaffUpgrade()
    {
        if (PlayerPrefs.HasKey("StaffUpgrade"))
        {
            string json = PlayerPrefs.GetString("StaffUpgrade");
            StaffData data = JsonUtility.FromJson<StaffData>(json);
            Debug.Log("Đã tải dữ liệu Staff Upgrade: " + json);
            return data;
        }
        else
        {
            Debug.Log("Không tìm thấy dữ liệu Staff Upgrade, tạo mới với giá trị mặc định");
            return new StaffData();
        }
    }

    /// <summary>
    /// Lấy cấp độ tốc độ phục vụ nhân viên
    /// </summary>
    public int GetStaffServiceSpeedLevel()
    {
        StaffData data = LoadStaffUpgrade();
        return data.levelServiceSpeed;
    }

    /// <summary>
    /// Lấy cấp độ tốc độ tuyển dụng nhân viên
    /// </summary>
    public int GetStaffHireSpeedLevel()
    {
        StaffData data = LoadStaffUpgrade();
        return data.levelHireSpeed;
    }

    /// <summary>
    /// Lấy tổng số nhân viên được tuyển dụng
    /// </summary>
    public int GetTotalStaffHired()
    {
        StaffData data = LoadStaffUpgrade();
        return data.totalStaffHired;
    }

    // ========== FOOD PURCHASE DATA ==========

    /// <summary>
    /// Mua/Unlock một loại food
    /// </summary>
    public void BuyFood(string foodID)
    {
        if (string.IsNullOrEmpty(foodID))
        {
            Debug.LogError("❌ BuyFood() called with empty/null foodID!");
            return;
        }

        FoodPurchaseData data = LoadFoodPurchaseData();
        
        if (!data.purchasedFoodIDs.Contains(foodID))
        {
            data.purchasedFoodIDs.Add(foodID);
            SaveFoodPurchaseData(data);
            Debug.Log($"✅ Đã mua/unlock food: {foodID}");
            Debug.Log($"📊 Danh sách hiện tại: {string.Join(", ", data.purchasedFoodIDs)}");
        }
        else
        {
            Debug.LogWarning($"⚠️ Food {foodID} đã được mua rồi!");
        }
    }

    /// <summary>
    /// Kiểm tra xem food đã được mua chưa
    /// </summary>
    public bool IsFoodPurchased(string foodID)
    {
        FoodPurchaseData data = LoadFoodPurchaseData();
        return data.purchasedFoodIDs.Contains(foodID);
    }

    /// <summary>
    /// Lấy danh sách tất cả Food IDs đã mua
    /// </summary>
    public List<string> GetPurchasedFoodIDs()
    {
        FoodPurchaseData data = LoadFoodPurchaseData();
        return data.purchasedFoodIDs;
    }

    /// <summary>
    /// Lưu dữ liệu Food đã mua vào PlayerPrefs
    /// </summary>
    private void SaveFoodPurchaseData(FoodPurchaseData data)
    {
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString("FoodPurchaseData", json);
        PlayerPrefs.Save();
        Debug.Log("Đã lưu dữ liệu Food Purchase: " + json);
    }

    /// <summary>
    /// Tải dữ liệu Food đã mua từ PlayerPrefs
    /// </summary>
    public FoodPurchaseData LoadFoodPurchaseData()
    {
        if (PlayerPrefs.HasKey("FoodPurchaseData"))
        {
            string json = PlayerPrefs.GetString("FoodPurchaseData");
            FoodPurchaseData data = JsonUtility.FromJson<FoodPurchaseData>(json);
            Debug.Log("Đã tải dữ liệu Food Purchase: " + json);
            return data;
        }
        else
        {
            Debug.Log("Không tìm thấy dữ liệu Food Purchase, tạo mới");
            return new FoodPurchaseData();
        }
    }

    private void OnApplicationQuit()
    {
        SaveData();
    }
}