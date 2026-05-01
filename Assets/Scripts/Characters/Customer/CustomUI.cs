using UnityEngine;
using UnityEngine.UI;

public class CustomerUI : MonoBehaviour
{
    [Header("UI Cài đặt - Order Bubble")]
    [SerializeField] private GameObject orderBubble; // Generic order bubble với Image component
    [SerializeField] private Image foodImageComponent; // Image component để hiển thị ảnh món ăn
    [SerializeField] private FoodDataList foodDataList; // Reference tới FoodDataList để lấy food data
    [SerializeField] private GameObject eatingBubble; // Bong bóng khi đang ăn (hiển thị khi đồ ăn được giao)

    private Transform bubbleContainer; // Container để chứa bubble

    // Ẩn tất cả khi khởi tạo
    void Start()
    {
        HideAllBubbles();
        SetupBubbleContainer();
    }

    /// <summary>
    /// Setup container để bubble hiển thị đúng vị trí trên đầu customer
    /// </summary>
    private void SetupBubbleContainer()
    {
        // Tìm object "Order_bubble_Menu" trong customer prefab
        Transform bubbleMenu = transform.Find("Order_bubble_Menu");
        if (bubbleMenu != null)
        {
            bubbleContainer = bubbleMenu;
        }
        else
        {
            // Nếu không tìm thấy, tìm parent của order bubble
            if (orderBubble != null)
            {
                bubbleContainer = orderBubble.transform.parent;
            }
            else
            {
                Debug.LogWarning("Không tìm thấy Order_bubble_Menu hoặc order bubble trong prefab customer!");
            }
        }

        // ✅ Tự động tìm FoodDataList nếu chưa được assign
        if (foodDataList == null)
        {
            FoodDataList[] allLists = Resources.FindObjectsOfTypeAll<FoodDataList>();
            if (allLists.Length > 0)
            {
                foodDataList = allLists[0];
                Debug.Log($"✅ Tự động tìm thấy FoodDataList: {foodDataList.name}");
            }
            else
            {
                Debug.LogWarning("⚠️ Không tìm thấy FoodDataList trong Resources! Hãy assign nó vào Customer Prefab hoặc đặt trong Resources folder.");
            }
        }

        // ✅ Tự động tìm Image component nếu chưa được assign
        if (foodImageComponent == null && orderBubble != null)
        {
            // Tìm Image trong children
            foodImageComponent = orderBubble.GetComponentInChildren<Image>();
            
            // Nếu vẫn không tìm thấy, tìm trong parent (Order_bubble_Menu)
            if (foodImageComponent == null && bubbleContainer != null)
            {
                foodImageComponent = bubbleContainer.GetComponentInChildren<Image>();
            }
            
            // Nếu vẫn không tìm thấy, tìm theo tên
            if (foodImageComponent == null)
            {
                Transform foodImageTrans = orderBubble.transform.Find("FoodImage");
                if (foodImageTrans != null)
                {
                    foodImageComponent = foodImageTrans.GetComponent<Image>();
                }
            }
            
            if (foodImageComponent != null)
            {
                Debug.Log($"✅ Tự động tìm thấy Image component: {foodImageComponent.gameObject.name}");
            }
            else
            {
                Debug.LogWarning("⚠️ Không tìm thấy Image component! Kiểm tra cấu trúc bubble hoặc assign thủ công.");
            }
        }
    }

    public void HideAllBubbles()
    {
        if (orderBubble != null) orderBubble.SetActive(false);
        if (eatingBubble != null) eatingBubble.SetActive(false);
    }

    /// <summary>
    /// Ẩn chỉ order bubble, không ẩn eating bubble
    /// Dùng khi staff nhận đơn - muốn ẩn order bubble nhưng giữ waiting bubble
    /// </summary>
    public void HideOrderBubblesOnly()
    {
        if (orderBubble != null) orderBubble.SetActive(false);
    }

    /// <summary>
    /// Ẩn toàn bộ Order_bubble_Menu container
    /// </summary>
    public void HideOrderBubbleMenuContainer()
    {
        // Tìm trực tiếp Order_bubble_Menu nếu bubbleContainer null
        if (bubbleContainer != null)
        {
            bubbleContainer.gameObject.SetActive(false);
            Debug.Log("✅ Ẩn Order_bubble_Menu container bằng cached reference");
        }
        else
        {
            // Tìm lại trực tiếp
            Transform orderBubbleMenu = transform.Find("Order_bubble_Menu");
            if (orderBubbleMenu != null)
            {
                orderBubbleMenu.gameObject.SetActive(false);
                Debug.Log("✅ Ẩn Order_bubble_Menu container bằng find");
            }
            else
            {
                // Nếu vẫn không tìm thấy, ẩn order bubble
                if (orderBubble != null) orderBubble.SetActive(false);
                Debug.LogWarning("⚠️ Không tìm thấy Order_bubble_Menu, ẩn order bubble riêng");
            }
        }
    }

    /// <summary>
    /// Bật Order_bubble_Menu container lại
    /// </summary>
    public void ShowOrderBubbleMenuContainer()
    {
        if (bubbleContainer != null)
        {
            bubbleContainer.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Hiển thị eating bubble khi customer nhận đồ ăn
    /// </summary>
    public void ShowEatingBubble()
    {
        HideAllBubbles();
        if (eatingBubble != null)
        {
            eatingBubble.SetActive(true);
            if (bubbleContainer != null)
            {
                eatingBubble.transform.SetParent(bubbleContainer);
            }
        }
    }

    /// <summary>
    /// Ẩn eating bubble khi customer ăn xong rời khỏi
    /// </summary>
    public void HideEatingBubble()
    {
        if (eatingBubble != null) eatingBubble.SetActive(false);
    }

    /// <summary>
    /// Hiển thị order bubble với ảnh từ FoodData dựa trên ItemType
    /// CustomerAI gọi hàm này khi khách chọn món
    /// </summary>
    public void ShowOrderBubble(ItemType orderedItem)
    {
        Debug.Log($"📍 ShowOrderBubble() được gọi với ItemType: {orderedItem}");
        HideAllBubbles();

        // ✅ Map ItemType sang FoodID
        string foodID = ConvertItemTypeToFoodID(orderedItem);
        if (string.IsNullOrEmpty(foodID))
        {
            Debug.LogError($"❌ Không thể map ItemType '{orderedItem}' sang FoodID!");
            return;
        }
        Debug.Log($"📍 Mapped to FoodID: {foodID}");

        // ✅ Lấy FoodData từ FoodDataList
        if (foodDataList == null)
        {
            Debug.LogError("❌ FoodDataList is null! Không thể lấy food data.");
            return;
        }
        Debug.Log($"📍 FoodDataList found: {foodDataList.name}");

        FoodData foodData = foodDataList.GetFoodByID(foodID);
        if (foodData == null)
        {
            Debug.LogWarning($"⚠️ Không tìm thấy FoodData với ID '{foodID}' trong FoodDataList");
            return;
        }
        Debug.Log($"📍 FoodData found: {foodData.FoodName}");

        // ✅ Hiển thị order bubble với ảnh từ FoodData
        if (orderBubble == null)
        {
            Debug.LogError("❌ orderBubble is null!");
            return;
        }
        Debug.Log($"📍 Order bubble found: {orderBubble.name}");

        orderBubble.SetActive(true);
        Debug.Log($"✅ Order bubble activated");
        
        // ✅ Set ảnh từ FoodData.Icon
        if (foodImageComponent != null)
        {
            if (foodData.Icon != null)
            {
                foodImageComponent.sprite = foodData.Icon;
                Debug.Log($"✅ Image set to: {foodData.Icon.name}");
            }
            else
            {
                Debug.LogWarning($"⚠️ FoodData.Icon is null for {foodData.FoodName}");
            }
        }
        else
        {
            Debug.LogWarning($"⚠️ foodImageComponent is null!");
        }

        // ✅ Đảm bảo bubble nằm trong container (trên đầu customer)
        if (bubbleContainer != null)
        {
            orderBubble.transform.SetParent(bubbleContainer);
            Debug.Log($"✅ Bubble moved to container: {bubbleContainer.name}");
        }
        else
        {
            Debug.LogWarning("⚠️ bubbleContainer is null!");
        }

        Debug.Log($"✅ Hiển thị order bubble cho: {foodData.FoodName} (ID: {foodID}) hoàn tất!");
    }

    /// <summary>
    /// Map ItemType sang FoodID
    /// Hỗ trợ: HAMBURGER → "1", COOKEDMEAT → "2"
    /// </summary>
    private string ConvertItemTypeToFoodID(ItemType itemType)
    {
        switch (itemType)
        {
            case ItemType.HAMBURGER:
                return "1";
            case ItemType.COOKEDMEAT:
                return "2";
            // ✅ Thêm mapping khác nếu cần
            default:
                Debug.LogWarning($"⚠️ Không có mapping cho ItemType: '{itemType}'");
                return null;
        }
    }



    /// <summary>
    /// Kích hoạt bubble ĐÚNG cho loại món đã gọi (dùng cho CountDown timer)
    /// Điều này để tránh kích hoạt bubble sai (vd: gọi COOKEDMEAT nhưng bubble lại show HAMBURGER)
    /// </summary>
    public void ShowCookingBubbleForFood(ItemType foodType)
    {
        // Hiển thị order bubble với ảnh từ FoodData
        ShowOrderBubble(foodType);
    }
}