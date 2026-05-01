using UnityEngine;

public class TableManager : MonoBehaviour
{
    [Header("Cấu hình định danh")]
    public string myTableID; // Ví dụ: bạn nhập "01", "02" vào Inspector

    [Header("Dữ liệu thực tế")]
    public DataManager.TableData myData; // Biến này sẽ chứa dữ liệu từ CSDL

    [Header("Các Object con")]
    public GameObject[] seats; // Kéo các cái ghế vào đây theo thứ tự

    [Header("Cấu hình giá")]
    public TableUpgradeConfig priceConfig;


    void Start()
    {
        // 1. Gán dữ liệu từ DataManager vào cho cái bàn này
        LoadTableData();
    }

    public void LoadTableData()
    {
        // Hỏi DataManager lấy dữ liệu của ID này
        myData = DataManager.Instance.LoadTable(myTableID);

        // Nếu là lần đầu chơi, ID này chưa có trong máy, hãy thiết lập mặc định
        if (string.IsNullOrEmpty(myData.idTable))
        {
            myData.idTable = myTableID;
            myData.isUnlocked = (myTableID == "01"); // Mặc định chỉ mở bàn 1
            myData.currentLevel = 1;

            // Lưu lại luôn để lần sau có dữ liệu
            DataManager.Instance.SaveTable(myData);
        }

        // 2. Cập nhật hình ảnh dựa trên dữ liệu đã gán
        UpdateVisuals();
    }

    public void UpdateVisuals()
    {
        // Ẩn/Hiện bàn dựa trên bool isUnlocked
        // (Ví dụ: Bạn có một cái "Model" bàn và một cái "Biển báo mua")
        // transform.GetChild(0).gameObject.SetActive(myData.isUnlocked);

        // Cập nhật số ghế dựa trên currentLevel
        for (int i = 0; i < seats.Length; i++)
        {
            seats[i].SetActive(i < myData.currentLevel);
        }
    }

    // Hàm gọi khi nhấn nút Nâng cấp bàn
    public void UpgradeTable()
    {
        if (priceConfig == null)
        {
            Debug.LogError("Chưa gán priceConfig!");
            return;
        }

        // 1. Tìm cấu hình giá cho cấp độ TIẾP THEO
        int nextLevel = myData.currentLevel + 1;
        int upgradeCost = priceConfig.GetCostForLevel(nextLevel);

        // 2. Kiểm tra điều kiện
        if (upgradeCost == 999999)
        {
            Debug.Log("Đã đạt cấp độ tối đa!");
            return;
        }

        // 3. Kiểm tra có đủ tiền không
        if (DataManager.Instance.totalMoney >= upgradeCost)
        {
            // Trừ tiền
            DataManager.Instance.SubstractGold(upgradeCost);
            DataManager.Instance.SaveData();

            // Nâng cấp
            myData.currentLevel++;
            UpdateVisuals();

            // Lưu dữ liệu
            DataManager.Instance.SaveTable(myData);
            Debug.Log("Nâng cấp thành công! Level hiện tại: " + myData.currentLevel);
        }
        else
        {
            int needMore = upgradeCost - DataManager.Instance.totalMoney;
            Debug.Log("Không đủ tiền! Cần thêm: " + needMore);
        }
    }
}
