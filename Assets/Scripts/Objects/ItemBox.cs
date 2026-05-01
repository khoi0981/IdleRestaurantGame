using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemBox : MonoBehaviour, IGetItem
{
    [Header("Cài đặt Queue")]
    [SerializeField] private float baseYOffset = 0.5f; // Y offset cho item đầu tiên
    [SerializeField] private float itemSpacingY = 0.3f; // Khoảng cách giữa các item

    [Header("Cài đặt Hiển thị (Kéo model trên bàn vào đây)")]
    public GameObject burgerModel;
    public GameObject meatModel;

    // ✅ QUEUE SYSTEM
    private Queue<ItemType> itemQueue = new Queue<ItemType>();
    private List<GameObject> visualItems = new List<GameObject>();

    private void Start()
    {
        // Queue trống lúc đầu
        Debug.Log("ItemBox được khởi tạo (queue trống)");
    }

    /// <summary>
    /// Thêm item vào queue và spawn visual
    /// </summary>
    public void SetType(ItemType type)
    {
        if (type == ItemType.NONE)
            return;

        itemQueue.Enqueue(type);
        Debug.Log($"✅ Item {type} được thêm vào queue. Tổng trong queue: {itemQueue.Count}");

        // Spawn visual item
        SpawnVisualItem(type);

        // Cập nhật vị trí tất cả items
        UpdateQueueDisplay();
    }

    /// <summary>
    /// Lấy item từ đầu queue (FIFO) và ẩn visual
    /// </summary>
    public ItemType GetItem()
    {
        Debug.Log($"📦 GetItem() gọi - Queue count: {itemQueue.Count}, Visual count: {visualItems.Count}");

        if (itemQueue.Count == 0)
        {
            Debug.LogWarning("⚠️ Queue rỗng! Không có item để lấy");
            return ItemType.NONE;
        }

        ItemType takenItem = itemQueue.Dequeue();
        Debug.Log($"✅ Lấy item: {takenItem} từ queue. Còn lại: {itemQueue.Count}");

        // ✅ Xóa visual item đầu tiên (vừa lấy)
        if (visualItems.Count > 0)
        {
            GameObject visualToDestroy = visualItems[0];
            Debug.Log($"🗑️ Xóa visual item: {visualToDestroy.name}");
            visualItems.RemoveAt(0);
            Destroy(visualToDestroy);
            Debug.Log($"✅ Xóa visual item khỏi queue thành công. Visual còn lại: {visualItems.Count}");
        }
        else
        {
            Debug.LogError("❌ ERROR: visualItems empty nhưng queue không empty!");
        }

        // Cập nhật vị trí items còn lại
        UpdateQueueDisplay();

        return takenItem;
    }

    public ItemType GetCurrentType()
    {
        if (itemQueue.Count > 0)
            return itemQueue.Peek();
        return ItemType.NONE;
    }

    public GameObject GetFoodModel(ItemType foodType)
    {
        switch (foodType)
        {
            case ItemType.HAMBURGER:
                return burgerModel;
            case ItemType.COOKEDMEAT:
                return meatModel;
            default:
                return null;
        }
    }

    /// <summary>
    /// Spawn visual item model tại itembox
    /// </summary>
    private void SpawnVisualItem(ItemType type)
    {
        GameObject modelPrefab = GetFoodModel(type);
        if (modelPrefab == null)
        {
            Debug.LogWarning($"⚠️ Không tìm thấy model cho: {type}");
            return;
        }

        // ✅ Tạo instance mới với parent là ItemBox
        GameObject visualItem = Instantiate(modelPrefab);
        visualItem.name = $"{type}_Visual_{visualItems.Count}";
        
        // ✅ Set parent để nó nằm dưới ItemBox hierarchy
        visualItem.transform.SetParent(transform);
        visualItem.transform.localPosition = Vector3.zero; // Vị trí sẽ được update bởi UpdateQueueDisplay()
        visualItem.transform.localRotation = Quaternion.identity;
        visualItem.transform.localScale = Vector3.one;
        
        // ✅ Tắt collider nếu có (để items không xung đột)
        Collider boxCollider = visualItem.GetComponent<Collider>();
        if (boxCollider != null)
            boxCollider.enabled = false;
        
        visualItems.Add(visualItem);
        Debug.Log($"✅ Spawn visual item: {visualItem.name}");
    }

    /// <summary>
    /// Cập nhật vị trí tất cả visual items trong queue
    /// Xếp chồng: item 0 ở baseYOffset, item 1 ở baseYOffset + itemSpacingY, v.v.
    /// </summary>
    private void UpdateQueueDisplay()
    {
        for (int i = 0; i < visualItems.Count; i++)
        {
            if (visualItems[i] != null)
            {
                // ✅ Đặt vị trí world tính từ transform của ItemBox
                Vector3 basePos = transform.position;
                Vector3 newPos = new Vector3(basePos.x, basePos.y + baseYOffset + (i * itemSpacingY), basePos.z);
                visualItems[i].transform.position = newPos;
                
                // ✅ Đảm bảo item hiển thị (SetActive true)
                if (!visualItems[i].activeSelf)
                    visualItems[i].SetActive(true);
                
                Debug.Log($"✅ Item [{i}]: {visualItems[i].name} → Position: {newPos}");
            }
        }
    }
}