using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FoodItem
{
    public string name;
    public string category; // "Đồ ăn" / "Đồ uống"
    public string description;
    public int price;
    public Sprite icon;
}