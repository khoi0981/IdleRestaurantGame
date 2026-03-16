using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerManager : MonoBehaviour
{
    public static CustomerManager Instance;

    [Header("Cài đặt sinh khách")]
    [SerializeField] private float timerSpeed = 1f;
    [SerializeField] private List<Customer> customerPrefabs = new List<Customer>(); // Danh sách các loại model khách
    [SerializeField] private Transform spawnPoint; // Điểm xuất phát (Cửa vào)
    [SerializeField] public Transform exitPoint; // Điểm thoát (Cửa ra)

    // Đã xóa biến exitPoint ở đây vì mỗi khách (Customer.cs) sẽ tự biết đường ra cửa

    private float currentTime = 0f;
    private float nextSpawnTime = 0f; // Thời gian ngẫu nhiên để sinh khách tiếp theo

    private void Awake()
    {
        Instance = this;
        // Khởi tạo thời gian chờ cho vị khách đầu tiên ngay khi mở quán
        SetRandomSpawnTime();
    }

    private void Update()
    {
        // Bộ đếm thời gian
        currentTime += Time.deltaTime * timerSpeed;

        // Nếu thời gian đếm đã vượt qua mốc chờ ngẫu nhiên
        if (currentTime >= nextSpawnTime)
        {
            SpawnCustomer(); // Gọi hàm sinh khách

            currentTime = 0f; // Reset đồng hồ
            SetRandomSpawnTime(); // Lên lịch cho vị khách tiếp theo
        }
    }

    private void SetRandomSpawnTime()
    {
        // Bạn có thể chỉnh lại khoảng thời gian khách đến ở đây (ví dụ: từ 5 đến 15 giây 1 khách)
        nextSpawnTime = Random.Range(5f, 15f);
    }

    private void SpawnCustomer()
    {
        if (customerPrefabs.Count == 0 || spawnPoint == null) return;

        // Chọn ngẫu nhiên 1 loại khách trong danh sách
        Customer randomPrefab = customerPrefabs[Random.Range(0, customerPrefabs.Count)];

        // Tạo khách tại vị trí Cửa (SpawnPoint)
        // Lưu ý: Không cần xếp hàng nên cứ tạo thẳng ở SpawnPoint
        Instantiate(randomPrefab, spawnPoint.position, spawnPoint.rotation);

        Debug.Log("Một vị khách mới vừa vào quán!");
    }
}