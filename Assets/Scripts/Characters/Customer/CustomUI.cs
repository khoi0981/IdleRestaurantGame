using UnityEngine;

public class CustomerUI : MonoBehaviour
{
    [Header("UI Cài đặt - Các loại bong bóng")]
    [SerializeField] private GameObject burgerOrderBubble;
    [SerializeField] private GameObject meatOrderBubble;
    [SerializeField] private GameObject waitingFoodBubble; // Bong bóng khi đang chờ đồ

    // Ẩn tất cả khi khởi tạo
    void Start()
    {
        HideAllBubbles();
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
                if (burgerOrderBubble != null) burgerOrderBubble.SetActive(true);
                break;
            case ItemType.COOKEDMEAT:      // Đảm bảo "MEAT" khớp với Enum ItemType của bạn
                if (meatOrderBubble != null) meatOrderBubble.SetActive(true);
                break;
                // Bạn có thể thêm các món khác vào đây
        }
    }

    // CustomerAI gọi hàm này sau khi Player bấm "Xác nhận Order"
    public void ShowWaitingBubble()
    {
        HideAllBubbles();
        if (waitingFoodBubble != null) waitingFoodBubble.SetActive(true);
    }
}