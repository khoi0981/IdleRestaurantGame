using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemBox : MonoBehaviour, IGetItem
{
    [Header("Trạng thái Vật phẩm")]
    [SerializeField] private ItemType item = ItemType.NONE; // Sửa thành NONE (viết hoa)

    [Header("Cài đặt Hiển thị (Kéo model trên bàn vào đây)")]
    public GameObject burgerModel;
    public GameObject meatModel;

    private void Start()
    {
        UpdateModelDisplay();
    }

    public virtual ItemType GetItem()
    {
        ItemType pickedItem = item;
        SetType(ItemType.NONE); // Sửa thành NONE
        return pickedItem;
    }

    public void SetType(ItemType type)
    {
        item = type;
        UpdateModelDisplay();
    }

    public ItemType GetCurrentType()
    {
        return item;
    }

    private void UpdateModelDisplay()
    {
        if (burgerModel != null) burgerModel.SetActive(false);
        if (meatModel != null) meatModel.SetActive(false);

        switch (item)
        {
            case ItemType.HAMBURGER:
                if (burgerModel != null) burgerModel.SetActive(true);
                break;
            case ItemType.COOKEDMEAT: // Sửa thành COOKEDMEAT
                if (meatModel != null) meatModel.SetActive(true);
                break;
        }
    }
}