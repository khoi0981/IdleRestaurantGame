using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class ShopManager : MonoBehaviour
{
    [SerializeField] private FoodDataList foodDataList;
    [SerializeField] private Transform scrollViewContent;
    [SerializeField] private FoodItemUI foodItemPrefab;
    [SerializeField] private TextMeshProUGUI playerCoinText;
    [SerializeField] private int initialCoin = 1000;
    [SerializeField] private int maxCoinDisplay = 9999;

    private const string PLAYER_COIN_KEY = "DiamondMoney";
    private const string PURCHASED_KEY_PREFIX = "Food_";
    private int currentCoin;

    private List<FoodItemUI> spawnedItems = new List<FoodItemUI>();

    private void Start()
    {
        InitializePlayerCoin();
        GenerateFoodItems();
        UpdateCoinDisplay();
    }

    private void InitializePlayerCoin()
    {
        // Nếu không có dữ liệu tiền trước đó, khởi tạo
        if (!PlayerPrefs.HasKey(PLAYER_COIN_KEY))
        {
            PlayerPrefs.SetInt(PLAYER_COIN_KEY, initialCoin);
        }

        currentCoin = PlayerPrefs.GetInt(PLAYER_COIN_KEY, initialCoin);
    }

    private void GenerateFoodItems()
    {
        if (foodDataList == null)
        {
            Debug.LogError("FoodDataList not assigned in ShopManager!");
            return;
        }

        List<FoodData> foods = foodDataList.GetFoodList();

        foreach (FoodData food in foods)
        {
            FoodItemUI itemUI = Instantiate(foodItemPrefab, scrollViewContent);
            itemUI.Initialize(food, this);
            spawnedItems.Add(itemUI);
        }
    }

    public bool BuyFood(FoodData food)
    {
        if (food == null)
        {
            Debug.LogError("FoodData is null!");
            return false;
        }

        // Kiểm tra đã mua chưa
        string purchaseKey = PURCHASED_KEY_PREFIX + food.FoodID;
        if (PlayerPrefs.GetInt(purchaseKey, 0) == 1)
        {
            Debug.LogWarning($"Food {food.FoodName} already purchased!");
            return false;
        }

        // Kiểm tra có đủ tiền không
        if (currentCoin < food.Cost)
        {
            Debug.LogWarning($"Not enough coins! Cost: {food.Cost}, Current: {currentCoin}");
            ShowNotification($"Không đủ tiền! Cần ${food.Cost}, Hiện có: ${currentCoin}");
            return false;
        }

        // Trừ tiền
        currentCoin -= food.Cost;

        // Lưu trạng thái mua vào PlayerPrefs
        PlayerPrefs.SetInt(purchaseKey, 1);
        PlayerPrefs.SetInt(PLAYER_COIN_KEY, currentCoin);
        PlayerPrefs.Save();

        // 🔥 SYNC: Cập nhật DataManager để customer có thể gọi món
        Debug.Log($"🔥 SYNCING to DataManager: FoodID='{food.FoodID}', FoodName='{food.FoodName}'");
        
        if (string.IsNullOrEmpty(food.FoodID))
        {
            Debug.LogError($"❌ SYNC FAILED! FoodID is empty/null for {food.FoodName}! Check FoodData asset!");
        }
        else
        {
            DataManager.Instance.BuyFood(food.FoodID);
            Debug.Log($"✅ SYNC SUCCESS! Called DataManager.BuyFood('{food.FoodID}')");
        }

        Debug.Log($"Successfully purchased: {food.FoodName}. Remaining coins: {currentCoin}");
        ShowNotification($"Đã mua: {food.FoodName}!");

        // Cập nhật hiển thị tiền
        UpdateCoinDisplay();

        return true;
    }

    public bool IsFoodPurchased(string foodID)
    {
        string purchaseKey = PURCHASED_KEY_PREFIX + foodID;
        return PlayerPrefs.GetInt(purchaseKey, 0) == 1;
    }

    public void AddCoin(int amount)
    {
        currentCoin += amount;
        PlayerPrefs.SetInt(PLAYER_COIN_KEY, currentCoin);
        PlayerPrefs.Save();
        UpdateCoinDisplay();
    }

    public void RemoveCoin(int amount)
    {
        currentCoin -= amount;
        if (currentCoin < 0) currentCoin = 0;
        PlayerPrefs.SetInt(PLAYER_COIN_KEY, currentCoin);
        PlayerPrefs.Save();
        UpdateCoinDisplay();
    }

    public int GetCurrentCoin() => currentCoin;

    public List<string> GetPurchasedFoodIDs()
    {
        List<string> purchasedIDs = new List<string>();
        foreach (FoodData food in foodDataList.GetFoodList())
        {
            if (IsFoodPurchased(food.FoodID))
            {
                purchasedIDs.Add(food.FoodID);
            }
        }
        return purchasedIDs;
    }

    private void UpdateCoinDisplay()
    {
        if (playerCoinText != null)
        {
            playerCoinText.text = $"{currentCoin}";
        }
    }

    private void ShowNotification(string message)
    {
        Debug.Log(message);
        // Có thể thêm notification UI ở đây
    }

    // Reset dữ liệu (chỉ dùng cho testing)
    public void ResetAllData()
    {
        PlayerPrefs.DeleteKey(PLAYER_COIN_KEY);

        foreach (FoodData food in foodDataList.GetFoodList())
        {
            string purchaseKey = PURCHASED_KEY_PREFIX + food.FoodID;
            PlayerPrefs.DeleteKey(purchaseKey);
        }

        PlayerPrefs.Save();
        InitializePlayerCoin();
        UpdateCoinDisplay();

        // Cập nhật lại UI
        foreach (FoodItemUI item in spawnedItems)
        {
            item.GetComponent<FoodItemUI>();
        }
    }
}
