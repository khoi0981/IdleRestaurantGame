using UnityEngine;

[CreateAssetMenu(fileName = "Food_", menuName = "Food/Food Data", order = 1)]
public class FoodData : ScriptableObject
{
    [SerializeField] private string foodID;
    [SerializeField] private string foodName;
    [SerializeField] private int cost; // Giá để mua
    [SerializeField] private int price; // Giá bán
    [SerializeField] private Sprite icon;
    [TextArea(3, 5)]
    [SerializeField] private string description; // Mô tả thêm

    public string FoodID => foodID;
    public string FoodName => foodName;
    public int Cost => cost;
    public int Price => price;
    public Sprite Icon => icon;
    public string Description => description;
}
