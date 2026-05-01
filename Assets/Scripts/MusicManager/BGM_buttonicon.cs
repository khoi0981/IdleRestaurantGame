using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class BGM_buttonicon : MonoBehaviour
{
    [SerializeField] Image SoundOnIcon;
    [SerializeField] Image SoundOffIcon;
    [SerializeField] Slider VolumeSlider; // ✅ Reference đến slider để phối hợp
    
    private bool muted = false;
    private float savedVolume = 1f; // Lưu âm lượng trước khi tắt
    
    void Start()
    {
        if(!PlayerPrefs.HasKey("muted"))
        {
            PlayerPrefs.SetInt("muted", 0);
            load();
        }
        else
        {
            load();
        }

        UpdateButtonIcon();
        // ✅ FIX: Thay vì dùng AudioListener.pause, ta sẽ quản lý volume
        // Nếu muted=true, SetMasterVolume(0). Nếu false, Restore volume từ SavedVolume
        if (muted)
        {
            AudioListener.volume = 0f;
            Debug.Log("🔇 Âm thanh được tắt khi khởi động (từ PlayerPrefs)");
        }
        
        // ✅ NEW: Subscribe vào SoundManager callbacks để sync state
        SoundManager.OnMuteStateChanged += HandleMuteStateChanged;
        SoundManager.OnVolumeChanged += HandleVolumeChanged;
        Debug.Log("✅ BGM_buttonicon đã subscribe vào SoundManager callbacks");
    }
    
    void OnDestroy()
    {
        // ✅ Unsubscribe khi destroy để tránh memory leak
        SoundManager.OnMuteStateChanged -= HandleMuteStateChanged;
        SoundManager.OnVolumeChanged -= HandleVolumeChanged;
    }
    
    // ✅ NEW: Callback từ SoundManager khi mute state thay đổi
    private void HandleMuteStateChanged(bool isMuted)
    {
        if (isMuted && !muted)
        {
            // SoundManager nói là muted, nhưng button nói là unmuted → cập nhật
            muted = true;
            UpdateButtonIcon();
            save();
            Debug.Log("🔄 Button state synced: Muted từ SoundManager");
        }
        else if (!isMuted && muted)
        {
            // SoundManager nói là unmuted, nhưng button nói là muted → cập nhật
            muted = false;
            UpdateButtonIcon();
            save();
            Debug.Log("🔄 Button state synced: Unmuted từ SoundManager");
        }
    }
    
    // ✅ NEW: Callback từ SoundManager khi volume thay đổi
    private void HandleVolumeChanged(float newVolume)
    {
        // Nếu volume > 0 nhưng button còn muted → auto-unmute button icon
        if (newVolume > 0 && muted)
        {
            muted = false;
            UpdateButtonIcon();
            save();
            Debug.Log($"🔄 Auto-unmute button: Volume = {newVolume:F2}");
        }
        // Nếu volume = 0 nhưng button còn unmuted → auto-mute button icon
        else if (newVolume <= 0 && !muted)
        {
            muted = true;
            UpdateButtonIcon();
            save();
            Debug.Log($"🔄 Auto-mute button: Volume = {newVolume:F2}");
        }
    }

    public void OnbuttonPress()
    {
        if (muted == false)
        {
            // Tắt âm thanh: lưu âm lượng hiện tại và set về 0
            muted = true;
            savedVolume = AudioListener.volume;
            AudioListener.volume = 0f;
            Debug.Log($"🔇 Tắt âm thanh - Saved volume: {savedVolume:F2}");
        }
        else
        {
            // Bật âm thanh: phục hồi âm lượng trước đó hoặc từ slider
            muted = false;
            if (VolumeSlider != null)
            {
                AudioListener.volume = VolumeSlider.value;
                Debug.Log($"🔉 Bật âm thanh - Volume từ slider: {VolumeSlider.value:F2}");
            }
            else
            {
                AudioListener.volume = savedVolume > 0 ? savedVolume : 1f;
                Debug.Log($"🔉 Bật âm thanh - Volume phục hồi: {AudioListener.volume:F2}");
            }
        }

        save();
        UpdateButtonIcon();
    }

    private void UpdateButtonIcon()
    {
        if (muted == false)
        {
            if (SoundOnIcon != null) SoundOnIcon.enabled = true;
            if (SoundOffIcon != null) SoundOffIcon.enabled = false;
        }
        else
        {
            if (SoundOnIcon != null) SoundOnIcon.enabled = false;
            if (SoundOffIcon != null) SoundOffIcon.enabled = true;
        }
    }

    private void load()
    {
        muted = PlayerPrefs.GetInt("muted", 0) == 1;
    }

    private void save()
    {
        PlayerPrefs.SetInt("muted", muted ? 1 : 0);
        PlayerPrefs.Save();
    }
}
