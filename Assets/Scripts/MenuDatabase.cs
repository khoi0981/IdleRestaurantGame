using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MenuDatabase", menuName = "Minishop/MenuDatabase")]
public class MenuDatabase : ScriptableObject
{
    public List<FoodItem> items = new List<FoodItem>();
}