using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ✅ Quản lý tất cả Staff trong scene
/// - Thu thập Staff hiện có
/// - Đồng bộ số lượng Staff với StaffUpgradeManager
/// - Spawn/Destroy Staff khi nâng cấp
/// - Update tốc độ Staff khi nâng cấp Service Speed
/// </summary>
public class StaffManager : MonoBehaviour
{
    public static StaffManager Instance;

    [Header("Thiết lập Staff")]
    [SerializeField] private GameObject staffPrefab;  // Prefab Staff
    [SerializeField] private Transform spawnParent;   // Parent để spawn staff (nếu có)
    [SerializeField] private Vector3 spawnOffset = new Vector3(2f, 0f, 0f);  // Khoảng cách spawn
    [SerializeField] private Transform waitingPoint;  // Điểm chờ (WaitingPoint)

    private List<StaffAI> staffList = new List<StaffAI>();
    private int targetStaffCount = 1;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // Thu thập Staff hiện có trong scene
        GatherExistingStaff();

        // Nếu không có WaitingPoint, tìm nó
        if (waitingPoint == null)
        {
            GameObject waitingPointObj = GameObject.FindGameObjectWithTag("WaitingPoint");
            if (waitingPointObj != null)
            {
                waitingPoint = waitingPointObj.transform;
                Debug.Log("[StaffManager] ✅ Found WaitingPoint: " + waitingPoint.name);
            }
        }

        // Nếu không có spawnParent, gán transform của manager
        if (spawnParent == null)
        {
            spawnParent = transform;
        }

        // Lấy target staff count từ StaffUpgradeManager
        if (StaffUpgradeManager.Instance != null)
        {
            targetStaffCount = StaffUpgradeManager.Instance.GetTotalStaffHired();
            Debug.Log($"[StaffManager] ✅ Target Staff Count: {targetStaffCount}");
        }

        // Đồng bộ số lượng Staff
        SyncStaffCount();

        // Đăng ký sự kiện
        StaffUpgradeManager.OnServiceSpeedUpgraded += UpdateAllStaffSpeed;
        StaffUpgradeManager.OnHireStaffUpgraded += OnHireStaffUpgraded;
    }

    /// <summary>
    /// Thu thập tất cả Staff trong scene
    /// </summary>
    private void GatherExistingStaff()
    {
        staffList.Clear();
        StaffAI[] allStaff = FindObjectsByType<StaffAI>(FindObjectsSortMode.None);
        staffList.AddRange(allStaff);
        Debug.Log($"[StaffManager] 👥 Gathered {staffList.Count} existing staff");
    }

    /// <summary>
    /// Đồng bộ số lượng Staff hiện tại với target
    /// </summary>
    private void SyncStaffCount()
    {
        int currentCount = staffList.Count;

        if (currentCount < targetStaffCount)
        {
            // Spawn thêm Staff
            int needToSpawn = targetStaffCount - currentCount;
            SpawnStaff(needToSpawn);
        }
        else if (currentCount > targetStaffCount)
        {
            // Xóa bớt Staff
            int needToRemove = currentCount - targetStaffCount;
            RemoveStaff(needToRemove);
        }

        Debug.Log($"[StaffManager] 📊 Staff Count synced: {staffList.Count}/{targetStaffCount}");
    }

    /// <summary>
    /// Spawn thêm Staff
    /// </summary>
    private void SpawnStaff(int count)
    {
        if (staffPrefab == null)
        {
            Debug.LogError("[StaffManager] ❌ Staff Prefab chưa được gán!");
            return;
        }

        if (waitingPoint == null)
        {
            Debug.LogError("[StaffManager] ❌ WaitingPoint chưa được gán!");
            return;
        }

        for (int i = 0; i < count; i++)
        {
            // Tính vị trí spawn
            Vector3 spawnPos = waitingPoint.position + (spawnOffset * i);

            // Instantiate
            GameObject newStaffObj = Instantiate(staffPrefab, spawnPos, Quaternion.identity, spawnParent);
            StaffAI staffAI = newStaffObj.GetComponent<StaffAI>();

            if (staffAI != null)
            {
                staffList.Add(staffAI);
                Debug.Log($"[StaffManager] ✅ Spawned Staff #{staffList.Count} at {spawnPos}");
            }
            else
            {
                Debug.LogError("[StaffManager] ❌ Prefab không có StaffAI component!");
                Destroy(newStaffObj);
            }
        }
    }

    /// <summary>
    /// Xóa bớt Staff
    /// </summary>
    private void RemoveStaff(int count)
    {
        for (int i = 0; i < count && staffList.Count > 1; i++)
        {
            StaffAI staff = staffList[staffList.Count - 1];
            staffList.RemoveAt(staffList.Count - 1);
            Destroy(staff.gameObject);
            Debug.Log("[StaffManager] ❌ Removed Staff, remaining: " + staffList.Count);
        }
    }

    /// <summary>
    /// Sự kiện: Nâng cấp Service Speed
    /// </summary>
    private void UpdateAllStaffSpeed()
    {
        Debug.Log("[StaffManager] 🚀 Updating all staff speed...");
        foreach (StaffAI staff in staffList)
        {
            if (staff != null)
            {
                staff.ApplyServiceSpeed();
            }
        }
    }

    /// <summary>
    /// Sự kiện: Nâng cấp Hire Staff
    /// </summary>
    private void OnHireStaffUpgraded()
    {
        if (StaffUpgradeManager.Instance != null)
        {
            targetStaffCount = StaffUpgradeManager.Instance.GetTotalStaffHired();
            Debug.Log($"[StaffManager] 👥 Hire Staff Upgraded! New target: {targetStaffCount}");
        }

        SyncStaffCount();
    }

    private void OnDestroy()
    {
        // Hủy đăng ký sự kiện
        StaffUpgradeManager.OnServiceSpeedUpgraded -= UpdateAllStaffSpeed;
        StaffUpgradeManager.OnHireStaffUpgraded -= OnHireStaffUpgraded;
    }

    /// <summary>
    /// Lấy danh sách Staff hiện tại
    /// </summary>
    public List<StaffAI> GetStaffList()
    {
        return staffList;
    }

    /// <summary>
    /// Lấy số lượng Staff hiện tại
    /// </summary>
    public int GetCurrentStaffCount()
    {
        return staffList.Count;
    }
}
