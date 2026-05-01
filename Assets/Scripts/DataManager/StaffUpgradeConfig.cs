using UnityEngine;

/// <summary>
/// Cấu hình chi phí và speed nâng cấp nhân viên
/// Service Speed và Hire Staff được tách riêng
/// </summary>
[CreateAssetMenu(menuName = "Configs/Staff Upgrade Config", fileName = "Staff_Price_Config")]
public class StaffUpgradeConfig : ScriptableObject
{
    [System.Serializable]
    public class ServiceSpeedLevel
    {
        [SerializeField] public int level;                // Cấp độ (0, 1, 2, 3...)
        [SerializeField] public int cost;                 // Chi phí nâng cấp
        [SerializeField] public float speedValue = 1.0f;  // Giá trị speed (1.0, 1.1, 1.2...)
        
        public ServiceSpeedLevel()
        {
            level = 0;
            cost = 0;
            speedValue = 1.0f;
        }
    }

    [System.Serializable]
    public class HireStaffLevel
    {
        [SerializeField] public int level;                // Cấp độ (0, 1, 2, 3...)
        [SerializeField] public int cost;                 // Chi phí nâng cấp
        
        public HireStaffLevel()
        {
            level = 0;
            cost = 0;
        }
    }

    [Header("=== SERVICE SPEED LEVELS ===")]
    [SerializeField] private ServiceSpeedLevel[] serviceSpeedLevels = new ServiceSpeedLevel[]
    {
        new ServiceSpeedLevel { level = 0, cost = 0, speedValue = 1.0f },
        new ServiceSpeedLevel { level = 1, cost = 100, speedValue = 1.0f },
        new ServiceSpeedLevel { level = 2, cost = 200, speedValue = 1.1f },
        new ServiceSpeedLevel { level = 3, cost = 300, speedValue = 1.2f },
        new ServiceSpeedLevel { level = 4, cost = 500, speedValue = 1.3f },
        new ServiceSpeedLevel { level = 5, cost = 800, speedValue = 1.4f },
    };

    [Header("=== HIRE STAFF LEVELS ===")]
    [SerializeField] private HireStaffLevel[] hireStaffLevels = new HireStaffLevel[]
    {
        new HireStaffLevel { level = 0, cost = 0 },
        new HireStaffLevel { level = 1, cost = 50 },
        new HireStaffLevel { level = 2, cost = 100 },
        new HireStaffLevel { level = 3, cost = 150 },
        new HireStaffLevel { level = 4, cost = 250 },
        new HireStaffLevel { level = 5, cost = 400 },
    };

    /// <summary>
    /// Lấy chi phí nâng cấp Service Speed cho cấp độ tiếp theo
    /// </summary>
    public int GetServiceSpeedCost(int currentLevel)
    {
        int nextLevel = currentLevel + 1;
        if (nextLevel >= 0 && nextLevel < serviceSpeedLevels.Length)
        {
            return serviceSpeedLevels[nextLevel].cost;
        }
        return 999999; // Max level
    }

    /// <summary>
    /// Lấy giá trị speed cho cấp độ
    /// </summary>
    public float GetServiceSpeedValue(int level)
    {
        if (level >= 0 && level < serviceSpeedLevels.Length)
        {
            return serviceSpeedLevels[level].speedValue;
        }
        return serviceSpeedLevels[serviceSpeedLevels.Length - 1].speedValue; // Giá trị max
    }

    /// <summary>
    /// Lấy chi phí nâng cấp Hire Staff cho cấp độ tiếp theo
    /// </summary>
    public int GetHireStaffCost(int currentLevel)
    {
        int nextLevel = currentLevel + 1;
        if (nextLevel >= 0 && nextLevel < hireStaffLevels.Length)
        {
            return hireStaffLevels[nextLevel].cost;
        }
        return 999999; // Max level
    }

    /// <summary>
    /// Lấy cấp độ tối đa của Service Speed
    /// </summary>
    public int GetMaxServiceSpeedLevel()
    {
        return serviceSpeedLevels.Length - 1;
    }

    /// <summary>
    /// Lấy cấp độ tối đa của Hire Staff
    /// </summary>
    public int GetMaxHireStaffLevel()
    {
        return hireStaffLevels.Length - 1;
    }

    /// <summary>
    /// Kiểm tra xem Service Speed đã max level hay chưa
    /// </summary>
    public bool IsServiceSpeedMaxLevel(int currentLevel)
    {
        return currentLevel >= GetMaxServiceSpeedLevel();
    }

    /// <summary>
    /// Kiểm tra xem Hire Staff đã max level hay chưa
    /// </summary>
    public bool IsHireStaffMaxLevel(int currentLevel)
    {
        return currentLevel >= GetMaxHireStaffLevel();
    }

    /// <summary>
    /// Lấy tất cả Service Speed levels
    /// </summary>
    public ServiceSpeedLevel[] GetAllServiceSpeedLevels()
    {
        return serviceSpeedLevels;
    }

    /// <summary>
    /// Lấy tất cả Hire Staff levels
    /// </summary>
    public HireStaffLevel[] GetAllHireStaffLevels()
    {
        return hireStaffLevels;
    }
}
