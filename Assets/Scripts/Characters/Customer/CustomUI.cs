using UnityEngine;

public class CustomerUI : MonoBehaviour
{
    [Header("UI Cài đặt - Các loại bong bóng")]
    [SerializeField] private GameObject burgerOrderBubble;
    [SerializeField] private GameObject meatOrderBubble;
    [SerializeField] private GameObject waitingFoodBubble; // Bong bóng khi đang chờ đồ

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
            Debug.LogWarning("Không tìm thấy Order_bubble_Menu trong prefab customer!");
        }
    }

    public void HideAllBubbles()
    {
        if (burgerOrderBubble != null) burgerOrderBubble.SetActive(false);
        if (meatOrderBubble != null) meatOrderBubble.SetActive(false);
        if (waitingFoodBubble != null) waitingFoodBubble.SetActive(false);
    }

    // CustomerAI gọi hàm này khi khách chọn món
    public void ShowOrderBubble(ItemType orderedItem)
    {
        HideAllBubbles();

        // Tùy theo món ăn mà bật đúng bong bóng
        switch (orderedItem)
        {
            case ItemType.HAMBURGER: // Đảm bảo "HAMBURGER" khớp với Enum ItemType của bạn
                if (burgerOrderBubble != null)
                {
                    burgerOrderBubble.SetActive(true);
                    // Đảm bảo bubble nằm trong container (trên đầu customer)
                    if (bubbleContainer != null)
                    {
                        burgerOrderBubble.transform.SetParent(bubbleContainer);
                    }
                }
                break;
            case ItemType.COOKEDMEAT:      // Đảm bảo "MEAT" khớp với Enum ItemType của bạn
                if (meatOrderBubble != null)
                {
                    meatOrderBubble.SetActive(true);
                    // Đảm bảo bubble nằm trong container (trên đầu customer)
                    if (bubbleContainer != null)
                    {
                        meatOrderBubble.transform.SetParent(bubbleContainer);
                    }
                }
                break;
                // Bạn có thể thêm các món khác vào đây
        }
    }

    // CustomerAI gọi hàm này sau khi Player bấm "Xác nhận Order"
    public void ShowWaitingBubble()
    {
        HideAllBubbles();
        if (waitingFoodBubble != null)
        {
            waitingFoodBubble.SetActive(true);
            // Đảm bảo bubble nằm trong container (trên đầu customer)
            if (bubbleContainer != null)
            {
                waitingFoodBubble.transform.SetParent(bubbleContainer);
            }
        }
    }
}