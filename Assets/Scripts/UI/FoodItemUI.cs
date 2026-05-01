using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class FoodItemUI : MonoBehaviour
{
    [SerializeField] private Image foodImage;
    [SerializeField] private TextMeshProUGUI foodNameText;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private TextMeshProUGUI btnText;
    [SerializeField] private Button buyButton;
    [SerializeField] private Button equippedButton;
    [SerializeField] private Color purchasedColor = Color.green;

    private FoodData foodData;
    private ShopManager shopManager;
    private const string PURCHASED_KEY_PREFIX = "Food_";

    public void Initialize(FoodData food, ShopManager manager)
    {
        foodData = food;
        shopManager = manager;

        // Hiển thị dữ liệu
        foodImage.sprite = food.Icon;
        foodNameText.text = food.FoodName;
        priceText.text = $"Price: {food.Price}";
        btnText.text = $"{food.Cost}";

        // Thiết lập nút mua
        buyButton.onClick.AddListener(OnBuyButtonClicked);
        equippedButton.onClick.AddListener(OnEquippedButtonClicked);

        // Kiểm tra xem đã mua chưa
        UpdatePurchaseStatus();
    }

    private void UpdatePurchaseStatus()
    {
        string purchaseKey = PURCHASED_KEY_PREFIX + foodData.FoodID;
        bool isPurchased = PlayerPrefs.GetInt(purchaseKey, 0) == 1;

        buyButton.gameObject.SetActive(!isPurchased);
        equippedButton.gameObject.SetActive(isPurchased);

        if (isPurchased)
        {
            btnText.text = "OWNED";
            btnText.color = purchasedColor;
        }
    }

    private void OnBuyButtonClicked()
    {
        if (shopManager != null)
        {
            bool success = shopManager.BuyFood(foodData);
            if (success)
            {
                UpdatePurchaseStatus();
            }
        }
    }

    private void OnEquippedButtonClicked()
    {
        // Xử lý khi nhấn nút EQUIPPED (có thể thêm logic trang bị đồ)
        Debug.Log($"Equipped: {foodData.FoodName}");
    }

    public FoodData GetFoodData() => foodData;
}
