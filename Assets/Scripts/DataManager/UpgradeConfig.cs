using UnityEngine;

[CreateAssetMenu(fileName = "NewConfig", menuName = "Configs/Upgrade")]
public class TableUpgradeConfig : ScriptableObject
{
    [System.Serializable]
    public class LevelPrice
    {
        public int level;
        public int upgradeCost;
    }

    public LevelPrice[] levels;

    public int GetCostForLevel(int targetLevel)
    {
        foreach (var l in levels)
        {
            if (l.level == targetLevel) return l.upgradeCost;
        }
        return 999999;
    }
}