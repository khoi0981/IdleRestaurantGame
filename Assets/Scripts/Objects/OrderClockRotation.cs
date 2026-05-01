using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class OrderClockRotation : MonoBehaviour
{
    [SerializeField] private RectTransform clockCircleRect; // OrderClockCircle RectTransform
    [SerializeField] private float duration; // Thời gian đếm ngược (giây)
    private float currentTime;
    private bool isRotating = false;
    private Vector3 initialPosition; // Lưu vị trí ban đầu
    private Vector3 initialRotation; // Lưu rotation ban đầu (X, Y, Z)

    private void OnEnable()
    {
        // ✅ Auto-find clockCircleRect nếu chưa được assign
        if (clockCircleRect == null)
        {
            // Cách 1: Tìm RectTransform của chính component này
            clockCircleRect = GetComponent<RectTransform>();
            
            // Cách 2: Tìm trong siblings theo tên "CookingCircleImage"
            if (clockCircleRect == null)
            {
                Transform cookingCircleImageTrans = transform.parent?.Find("CookingCircleImage");
                if (cookingCircleImageTrans != null)
                {
                    clockCircleRect = cookingCircleImageTrans.GetComponent<RectTransform>();
                }
            }
            
            // Cách 3: Tìm trong parent
            if (clockCircleRect == null && transform.parent != null)
            {
                clockCircleRect = transform.parent.GetComponent<RectTransform>();
            }
            
            // Cách 4: Tìm Image component trong children hoặc siblings
            if (clockCircleRect == null)
            {
                Image[] allImages = transform.parent?.GetComponentsInChildren<Image>();
                if (allImages != null && allImages.Length > 0)
                {
                    // Tìm Image đầu tiên có tên chứa "Circle"
                    foreach (Image img in allImages)
                    {
                        if (img.gameObject.name.Contains("Circle"))
                        {
                            clockCircleRect = img.GetComponent<RectTransform>();
                            if (clockCircleRect != null) break;
                        }
                    }
                }
            }
            
            // Nếu tìm thấy, log thông báo
            if (clockCircleRect != null)
            {
                Debug.Log($"✅ OrderClockRotation: Tự động tìm thấy RectTransform: {clockCircleRect.name}");
            }
            else
            {
                Debug.LogWarning("⚠️ OrderClockRotation: Không tìm thấy RectTransform. Hãy assign CookingCircleImage vào Inspector nếu auto-find không thành công.");
            }
        }
    }

    /// <summary>
    /// Bắt đầu quay đồng hồ
    /// </summary>
    /// <param name="countdownDuration">Thời gian đếm ngược (giây)</param>
    public void StartClockRotation(float countdownDuration)
    {
        // ✅ Auto-find nếu chưa có reference
        if (clockCircleRect == null)
        {
            OnEnable(); // Try auto-find again
        }
        
        // ✅ Final check - nếu vẫn null thì thử tìm last resort
        if (clockCircleRect == null)
        {
            // Last resort: Tìm Image component bất kỳ trong cooking bubble
            Image circleImage = GetComponentInChildren<Image>();
            if (circleImage != null)
            {
                clockCircleRect = circleImage.GetComponent<RectTransform>();
                Debug.LogWarning($"⚠️ OrderClockRotation: Tìm thấy Image component tạm thời: {circleImage.gameObject.name}");
            }
        }
        
        // Nếu vẫn không tìm thấy, báo lỗi
        if (clockCircleRect == null)
        {
            Debug.LogError("❌ OrderClockRotation: FAILED to find CookingCircleImage RectTransform! Rotation will NOT work. Please assign it in the Inspector: cooking bubble > CookingCircleImage");
            return;
        }

        Debug.Log("✅ Bắt đầu quay! Duration: " + countdownDuration + " giây");
        
        // Lưu vị trí ban đầu
        initialPosition = clockCircleRect.anchoredPosition;
        
        // Đặt rotation Z = 180 độ từ đầu, giữ X=0, Y=0 (dùng localEulerAngles)
        clockCircleRect.localEulerAngles = new Vector3(0, 0, 180);
        initialRotation = new Vector3(0, 0, 180);
        
        duration = countdownDuration;
        currentTime = duration;
        isRotating = true;

        // Dừng coroutine cũ nếu có
        StopCoroutine(RotateClock());
        
        // Bắt đầu coroutine quay
        StartCoroutine(RotateClock());
    }

    /// <summary>
    /// Dừng quay đồng hồ
    /// </summary>
    public void StopClockRotation()
    {
        isRotating = false;
        StopCoroutine(RotateClock());
        // Reset về vị trí và rotation ban đầu (Z = 180 độ) dùng localEulerAngles
        clockCircleRect.anchoredPosition = initialPosition;
        clockCircleRect.localEulerAngles = new Vector3(0, 0, 180);
    }

    /// <summary>
    /// Coroutine để xoay đồng hồ từ từ
    /// </summary>
    private IEnumerator RotateClock()
    {
        while (currentTime >= 0 && isRotating)
        {
            // Giữ vị trí ban đầu
            clockCircleRect.anchoredPosition = initialPosition;
            
            // ✅ Giữ Z = 180 độ CỐ ĐỊNH, không thay đổi
            // Vòng tròn sẽ hiển thị quay là do fillAmount giảm từ 1→0 (fill method radial)
            clockCircleRect.localEulerAngles = new Vector3(0, 0, 180);

            yield return new WaitForSeconds(0.016f); // ~60 FPS
            
            // Giảm thời gian
            currentTime -= Time.deltaTime;
        }

        // Khi hết thời gian, giữ Z = 180 cố định
        if (isRotating)
        {
            clockCircleRect.anchoredPosition = initialPosition;
            clockCircleRect.localEulerAngles = new Vector3(0, 0, 180f);
            isRotating = false;
        }
    }

    /// <summary>
    /// Lấy thời gian còn lại (giây)
    /// </summary>
    public float GetRemainingTime()
    {
        return Mathf.Max(0, currentTime);
    }

    /// <summary>
    /// Kiểm tra có đang quay không
    /// </summary>
    public bool IsRotating()
    {
        return isRotating;
    }
}
