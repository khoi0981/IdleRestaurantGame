using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ObjectnType
{
    public GameObject item;
    public ItemType type;
}

public class Inventory : MonoBehaviour
{
    [SerializeField] private List<ObjectnType> itemsToHold = new List<ObjectnType>();
    private ItemType currentType;
    public ItemType CurrentType { get { return currentType; } }

    private void Start()
    {
        currentType = ItemType.NONE;
    }

    public void TakeItem(ItemType type)
    {
        if (currentType != ItemType.NONE) return;
        currentType = type;

        Debug.Log("Inventory đang cố gắng hiển thị món: " + type); // Báo cáo ra Console

        bool foundModel = false;

        foreach (ObjectnType itemHold in itemsToHold)
        {
            if (itemHold.type != type)
            {
                if (itemHold.item != null) itemHold.item.SetActive(false);
            }
            else
            {
                if (itemHold.item != null)
                {
                    itemHold.item.SetActive(true);
                    foundModel = true;
                    Debug.Log("Đã BẬT thành công model 3D cho món: " + type);
                }
            }
        }

        if (!foundModel)
        {
            Debug.LogWarning("CẢNH BÁO: Player đã cầm " + type + " nhưng KHÔNG TÌM THẤY model 3D nào được cài đặt cho món này trong danh sách Items To Hold!");
        }
    }

    public ItemType PutItem()
    {
        if (currentType == ItemType.NONE) return ItemType.NONE;
        return currentType;
    }

    public void ClearHand()
    {
        currentType = ItemType.NONE;
        itemsToHold.ForEach(obj => {
            if (obj.item != null) obj.item.SetActive(false);
        });
        Debug.Log("Đã xóa đồ trên tay Player");
    }

    public ItemType GetItem()
    {
        return currentType;
    }
}