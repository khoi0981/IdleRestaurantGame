using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CountDown : MonoBehaviour
{
    [SerializeField] GameObject panel;
    [SerializeField] Image timeImage;
    [SerializeField] Text timeText;
    [SerializeField] float duration, currentTime;
    [SerializeField] private OrderClockRotation orderClockRotation; // Tham chiếu tới script quay

    void Start()
    {
        // Khởi tạo trạng thái ban đầu
        if (panel != null)
            panel.SetActive(false);
        
        currentTime = duration;
        if (timeText != null)
            timeText.text = currentTime.ToString();
        
        // ✅ Auto-find OrderClockRotation - tìm trong hierarchy trước (parent/siblings)
        if (orderClockRotation == null)
        {
            // Cách 1: Tìm trong parent
            if (transform.parent != null)
            {
                orderClockRotation = transform.parent.GetComponentInChildren<OrderClockRotation>();
            }
            
            // Cách 2: Nếu vẫn không tìm thấy, tìm trong siblings
            if (orderClockRotation == null && transform.parent != null)
            {
                Transform parentParent = transform.parent.parent;
                if (parentParent != null)
                {
                    orderClockRotation = parentParent.GetComponentInChildren<OrderClockRotation>();
                }
            }
            
            // Cách 3: Last resort - tìm trong scene (để tương thích với Customer bubble)
            if (orderClockRotation == null)
            {
                orderClockRotation = FindFirstObjectByType<OrderClockRotation>();
                if (orderClockRotation != null)
                {
                    Debug.LogWarning("⚠️ OrderClockRotation tìm được từ scene (FindFirstObjectByType). Có thể có nhiệm OrderClockRotation, hãy assign thủ công nếu cần chính xác.");
                }
            }
            
            if (orderClockRotation != null)
            {
                Debug.Log($"✅ CountDown: Tự động tìm thấy OrderClockRotation: {orderClockRotation.gameObject.name}");
            }
            else
            {
                Debug.LogWarning("⚠️ CountDown: Không tìm thấy OrderClockRotation! Hãy assign nó vào Inspector hoặc đảm bảo nó trong hierarchy của bubble.");
            }
        }
    }

    // Gọi method này khi order bubble hiển thị
    public void StartCountdown()
    {
        currentTime = duration;
        if (timeText != null)
            timeText.text = currentTime.ToString();
        
        // Bắt đầu đếm ngược
        StartCoroutine(TimeIn());
    }

    // Set duration và bắt đầu countdown (dùng từ StaffAI / OvenStation)
    public void SetDurationAndStartCountdown(float newDuration)
    {
        duration = newDuration;
        currentTime = duration;
        if (timeText != null)
            timeText.text = currentTime.ToString();
        
        // ✅ Auto-find OrderClockRotation trước khi bắt đầu, nếu vẫn null
        if (orderClockRotation == null)
        {
            // Tìm lại trong hierarchy
            if (transform.parent != null)
            {
                orderClockRotation = transform.parent.GetComponentInChildren<OrderClockRotation>();
            }
            
            if (orderClockRotation == null && transform.parent != null)
            {
                Transform parentParent = transform.parent.parent;
                if (parentParent != null)
                {
                    orderClockRotation = parentParent.GetComponentInChildren<OrderClockRotation>();
                }
            }
            
            if (orderClockRotation == null)
            {
                orderClockRotation = FindFirstObjectByType<OrderClockRotation>();
            }
            
            if (orderClockRotation == null)
            {
                Debug.LogError("❌ CountDown.SetDurationAndStartCountdown: KHÔNG THỂ tìm OrderClockRotation! Vòng tròn sẽ không xoay. Hãy assign OrderClockRotation vào Inspector.");
            }
        }
        
        // Bắt đầu đếm ngược
        StartCoroutine(TimeIn());
    }

    // Lấy duration hiện tại
    public float GetDuration()
    {
        return duration;
    }

    IEnumerator TimeIn()
    {
        // ✅ Auto-find timeImage nếu chưa được assign
        if (timeImage == null)
        {
            // Cách 1: Tìm trong children của CountDown component
            timeImage = GetComponentInChildren<Image>();
            
            // Cách 2: Tìm trong parent (OrderClockCircle)
            if (timeImage == null && transform.parent != null)
            {
                timeImage = transform.parent.GetComponentInChildren<Image>();
            }
            
            // Cách 3: Tìm theo tên "CookingCircleImage" hoặc chứa "Circle"
            if (timeImage == null && transform.parent != null)
            {
                Transform circleImageTrans = transform.parent.Find("CookingCircleImage");
                if (circleImageTrans != null)
                {
                    timeImage = circleImageTrans.GetComponent<Image>();
                }
            }
            
            if (timeImage != null)
            {
                Debug.Log($"✅ CountDown: Tự động tìm thấy Image component: {timeImage.gameObject.name}");
            }
            else
            {
                Debug.LogWarning("⚠️ CountDown: Không tìm thấy Image component để cập nhật fillAmount! Hãy assign timeImage trong Inspector hoặc đảm bảo CookingCircleImage có Image component.");
            }
        }
        
        // Bắt đầu quay đồng hồ
        if (orderClockRotation != null)
        {
            Debug.Log("✅ OrderClockRotation tìm thấy, bắt đầu quay với duration: " + duration);
            orderClockRotation.StartClockRotation(duration);
        }
        else
        {
            Debug.LogError("❌ OrderClockRotation là NULL! Vòng tròn không thể quay. Hãy đảm bảo OrderClockRotation component được add vào hierarchy hoặc assign trong Inspector.");
        }

        float elapsedTime = 0f;
        float nextSecondUpdate = 0f;

        while (elapsedTime <= duration)
        {
            // Cập nhật fillAmount mỗi frame (mượt từ 1 → 0)
            if (timeImage != null)
                timeImage.fillAmount = 1f - (elapsedTime / duration);
            
            // Cập nhật timeText mỗi giây
            if (elapsedTime >= nextSecondUpdate)
            {
                currentTime = Mathf.Max(0, duration - elapsedTime);
                if (timeText != null)
                    timeText.text = Mathf.Max(0, currentTime).ToString("F0");
                nextSecondUpdate += 1f;
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Đảm bảo fillAmount = 0 khi kết thúc
        if (timeImage != null)
            timeImage.fillAmount = 0f;

        // Gọi hàm mở bảng thông báo khi hết thời gian
        OpenPanel();
    }

    void OpenPanel()
    {
        if (timeText != null)
            timeText.text = "";
        if (panel != null)
            panel.SetActive(true);
        
        // Dừng quay đồng hồ
        if (orderClockRotation != null)
        {
            orderClockRotation.StopClockRotation();
        }
    }
}
