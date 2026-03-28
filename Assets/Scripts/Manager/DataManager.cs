using UnityEngine;
using TMPro; // BẮT BUỘC THÊM DÒNG NÀY ĐỂ DÙNG TEXT (TMP)

public class DataManager : MonoBehaviour
{
    // Tạo một bản sao duy nhất để gọi từ bất kỳ file nào khác (Singleton)
    public static DataManager Instance;

    [Header("Giao diện (UI)")]
    public TextMeshProUGUI coinText; // Kéo chữ Text (TMP) hiển thị tiền vào đây

    [Header("Dữ liệu Người chơi")]
    public int totalMoney = 0; // Tổng số tiền hiện có
    public int unlockedTables = 1; // Số bàn đã mở khóa (Dùng cho phần Shop sau này)

    void Awake()
    {
        // Kỹ thuật Singleton + Kim bài miễn tử (DontDestroyOnLoad)
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Giữ nguyên file này khi chuyển Scene

            // Tự động tải tiền cũ ngay khi game vừa bật lên
            LoadData();
        }
        else
        {
            // Nếu qua Scene mới mà lỡ có 1 cái DataManager khác sinh ra thì tự hủy bản sao mới này
            Destroy(gameObject);
        }
    }

    // --- HÀM CẬP NHẬT GIAO DIỆN ---
    public void UpdateUI()
    {
        if (coinText != null)
        {
            coinText.text = totalMoney.ToString(); // Đổi số tiền thành chữ và hiển thị lên màn hình
        }
    }

    // --- HÀM LƯU DỮ LIỆU ---
    public void SaveData()
    {
        // Dùng PlayerPrefs để ghi thẳng vào bộ nhớ máy (Điện thoại/Máy tính)
        PlayerPrefs.SetInt("PlayerMoney", totalMoney);
        PlayerPrefs.SetInt("UnlockedTables", unlockedTables);

        PlayerPrefs.Save(); // Chốt lưu
        Debug.Log("Đã lưu dữ liệu! Tiền hiện tại: " + totalMoney);

        // MẸO Ở ĐÂY: Vì CustomerAI luôn gọi SaveData() sau khi ăn xong, ta sẽ cho update UI luôn tại đây!
        UpdateUI();
    }

    // --- HÀM TẢI DỮ LIỆU ---
    public void LoadData()
    {
        // Lấy dữ liệu ra. Số 0 và 1 ở đằng sau là giá trị mặc định cho người mới chơi lần đầu
        totalMoney = PlayerPrefs.GetInt("PlayerMoney", 0);
        unlockedTables = PlayerPrefs.GetInt("UnlockedTables", 1);

        // Hiển thị số tiền lên màn hình ngay khi game vừa load xong
        UpdateUI();
    }

    // (Tùy chọn) Tự động lưu khi người chơi bấm nút X thoát game trên PC hoặc vuốt tắt app trên Mobile
    private void OnApplicationQuit()
    {
        SaveData();
    }
}