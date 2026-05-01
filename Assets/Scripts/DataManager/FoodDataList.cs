using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "FoodDataList", menuName = "Food/Food Data List", order = 2)]
public class FoodDataList : ScriptableObject
{
    [SerializeField] private List<FoodData> foodList = new List<FoodData>();

    public List<FoodData> GetFoodList() => foodList;
    
    public FoodData GetFoodByID(string foodID)
    {
        foreach (var food in foodList)
        {
            if (food.FoodID == foodID)
                return food;
        }
        return null;
    }

    public int GetFoodCount() => foodList.Count;
}
