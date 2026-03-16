using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public string itemName;      // tên item
    public Sprite icon;          // hình icon item
    public bool isPickable = true; // có thể nhặt không

    private bool isHolding = false;

    void Start()
    {
        Debug.Log("Item spawned: " + itemName);
    }

    // Khi player nhặt item
    public void PickItem(Transform hand)
    {
        if (!isPickable) return;

        isHolding = true;

        transform.SetParent(hand);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        Debug.Log("Picked item: " + itemName);
    }

    // Khi player thả item
    public void DropItem()
    {
        isHolding = false;

        transform.SetParent(null);

        Debug.Log("Dropped item: " + itemName);
    }

    // Kiểm tra item có đang được cầm không
    public bool IsHolding()
    {
        return isHolding;
    }
}