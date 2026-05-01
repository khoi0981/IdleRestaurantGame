using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour
{
    [SerializeField] public Slider VolumeSlider;
    private bool isMuted = false;
    private float savedVolume = 1f;
    
    // ✅ Static callback để BGM_buttonicon có thể subscribe
    public static System.Action<bool> OnMuteStateChanged;
    public static System.Action<float> OnVolumeChanged;

    void Start()
    {
        // ✅ Setup: Đảm bảo slider event listener được kết nối chắc chắn
        if (VolumeSlider != null)
        {
            // Xóa các listener trước để tránh duplicate
            VolumeSlider.onValueChanged.RemoveListener(SetVolume);
            // Thêm event listener
            VolumeSlider.onValueChanged.AddListener(SetVolume);
            
            Debug.Log("✅ VolumeSlider event listener đã được kết nối!");
        }
        else
        {
            Debug.LogError("❌ VolumeSlider không được gán! Vui lòng gán Slider component trong Inspector!");
            return;
        }

        // Load volume từ PlayerPrefs
        if (PlayerPrefs.HasKey("soundVolume"))
        {
            LoadVolume();
        }
        else
        {
            PlayerPrefs.SetFloat("soundVolume", 1);
            LoadVolume();
        }

        // Load mute state từ BGM_buttonicon
        isMuted = PlayerPrefs.GetInt("muted", 0) == 1;
        if (isMuted)
        {
            AudioListener.volume = 0f;
            Debug.Log("🔇 Âm thanh đã bị tắt từ PlayerPrefs");
        }
    }

    // ✅ FIX: Thay đổi signature để chấp nhận slider value parameter
    public void SetVolume(float volume)
    {
        // ✅ NEW: Nếu user kéo slider lên, tự động unmute
        if (volume > 0 && isMuted)
        {
            isMuted = false;
            Debug.Log($"🔉 Auto-unmute: Volume từ slider = {volume:F2}");
            OnMuteStateChanged?.Invoke(false); // Notify button
        }
        
        // Nếu không mute, cập nhật volume bình thường
        if (!isMuted)
        {
            AudioListener.volume = Mathf.Clamp01(volume); // Đảm bảo giá trị từ 0-1
            savedVolume = AudioListener.volume;
        }
        
        SaveVolume();
        // ✅ Notify về volume change
        OnVolumeChanged?.Invoke(AudioListener.volume);
        Debug.Log($"🔊 Volume đã cập nhật: {AudioListener.volume:F2}");
    }

    // ✅ Overload: hỗ trợ gọi từ code mà không có parameter
    public void SetVolume()
    {
        if (VolumeSlider != null)
        {
            SetVolume(VolumeSlider.value);
        }
    }

    public void SaveVolume()
    {
        if (VolumeSlider != null)
        {
            PlayerPrefs.SetFloat("soundVolume", VolumeSlider.value);
            PlayerPrefs.Save(); // ✅ Đảm bảo dữ liệu được lưu ngay lập tức
        }
    }

    public void LoadVolume()
    {
        if (VolumeSlider != null)
        {
            float loadedVolume = PlayerPrefs.GetFloat("soundVolume", 1f);
            VolumeSlider.value = loadedVolume;
            
            // Cập nhật AudioListener.volume nếu không mute
            if (!isMuted)
            {
                AudioListener.volume = loadedVolume;
            }
            
            Debug.Log($"📢 Volume đã được khôi phục: {loadedVolume:F2}");
        }
    }

    // ✅ Hàm hỗ trợ: Kiểm tra audio có bị mute không
    public bool IsMuted()
    {
        return isMuted;
    }

    // ✅ Hàm hỗ trợ: Quản lý mute state
    public void SetMuteState(bool muteState)
    {
        isMuted = muteState;
        if (isMuted)
        {
            AudioListener.volume = 0f;
        }
        else
        {
            SetVolume(); // Phục hồi volume từ slider
        }
        OnMuteStateChanged?.Invoke(isMuted); // Notify button
    }
    
    // ✅ Getter: để check mute state từ bgm_buttonicon
    public bool IsMutedState()
    {
        return isMuted;
    }
}