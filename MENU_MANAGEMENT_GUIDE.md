# 📚 Hướng Dẫn Sử Dụng Menu Management Scripts

## 🎯 Tổng Quan
Đã tạo 3 script để quản lý menu trong game:
1. **MenuToggler.cs** - Script cốt lõi, quản lý ẩn/hiện menu
2. **MenuExitButton.cs** - Helper script cho nút exit
3. **SettingsMenuManager.cs** - Ví dụ cụ thể cho Settings menu

**Quan trọng:** Những script này **KHÔNG** ảnh hưởng đến âm nhạc (BGM/SoundManager)

---

## 🚀 Cách Sử Dụng (2 Phương Pháp)

### Phương Pháp 1: Đơn Giản (Khuyến Nghị)
**Cho Settings menu hoặc bất kỳ menu nào:**

1. **Đặt script trên object chứa menu:**
   - Chọn GameObject của Settings menu panel trong Hierarchy
   - Add Component: `MenuToggler`
   - Drag & drop chính object đó vào field `Target Menu`

2. **Gắn vào nút Exit Button:**
   - Chọn Exit Button trong Hierarchy
   - Trong Inspector, tìm Button Component
   - Click "+" trong On Click event
   - Drag `MenuToggler` script vào object field
   - Chọn: `MenuToggler > HideMenu()`

### Phương Pháp 2: Code (Nâng Cao)
**Nếu muốn tự động hóa:**

```csharp
// Gắn MenuToggler lên Settings Menu Panel
var settingsPanel = GameObject.Find("SettingsPanel");
MenuToggler toggler = settingsPanel.AddComponent<MenuToggler>();

// Hoặc nếu đã có trong Inspector
MenuToggler toggler = settingsPanel.GetComponent<MenuToggler>();

// Gắn sự kiện cho nút Exit
exitButton.onClick.AddListener(() => toggler.HideMenu());
```

### Phương Pháp 3: Sử Dụng SettingsMenuManager
**Cho Settings menu cụ thể:**

1. Tạo một GameObject mới để quản lý Settings
2. Add Component: `SettingsMenuManager`
3. Trong Inspector:
   - Assign Settings Menu Panel vào `Settings Menu Panel` field
   - Assign MenuToggler vào `Menu Toggler` field

---

## 📋 Public Methods (Gọi từ Code)

### MenuToggler
```csharp
// Ẩn menu
menuToggler.HideMenu();

// Hiện menu
menuToggler.ShowMenu();

// Chuyển đổi (toggle)
menuToggler.ToggleMenu();

// Kiểm tra menu có mở?
bool isOpen = menuToggler.IsMenuOpen();

// Gán target menu từ code
menuToggler.SetTargetMenu(someGameObject);
```

### MenuExitButton
```csharp
// Gọi khi nút Exit được nhấp
exitButton.GetComponent<MenuExitButton>().OnExitButtonClicked();
```

### SettingsMenuManager
```csharp
// Mở Settings
settingsManager.OpenSettings();

// Đóng Settings
settingsManager.CloseSettings();
```

---

## ⚙️ Tùy Chỉnh

### Animation (Fade In/Out)
Trong MenuToggler Inspector:
- Enable `Use Animation`
- Điều chỉnh `Animation Duration` (mặc định 0.3s)

**Lưu ý:** Khi sử dụng animation, script sẽ tự động thêm `CanvasGroup` component nếu chưa có.

---

## ✅ Kiểm Tra Tính Năng

### Test 1: Ẩn/Hiện Menu
- Nhấp nút Exit → Menu phải ẩn đi
- Một lần khác nhấp để hiện lại
- Check Console: Nên thấy log ✅ hoặc ⚠️

### Test 2: BGM Không Bị Ảnh Hưởng
- Mở/Đóng menu
- Âm nhạc vẫn phát bình thường
- Không có lệnh dừng/pause AudioListener

### Test 3: Animation (Nếu Bật)
- Menu phải fade in/out mượt mà
- Không jump hoặc glitch

---

## 🔄 Tái Sử Dụng cho Menu Khác

**Cho Pause Menu, Options, Inventory, v.v.:**

1. Copy MenuToggler.cs vào script của menu mới (hoặc dùng cùng script)
2. Gán vào nút Exit/Close
3. Thêm `menuToggler.HideMenu()` vào onclick event

**Script này hoàn toàn generic, không phải thay đổi code gì!**

---

## 📝 Lưu Ý Quan Trọng

✅ **Các script này:**
- Không ảnh hưởng BGM/SoundManager
- Chỉ ẩn/hiện GameObject (SetActive)
- Có thể dùng cho mọi menu trong game
- Support fade animation optional

❌ **Các script này KHÔNG:**
- Tắt âm thanh
- Pause game
- Thoát application (dùng Menu_controller.cs cho việc đó)
- Thay đổi scene

---

## 🐛 Gỡ Lỗi

| Vấn Đề | Giải Pháp |
|--------|----------|
| Menu không ẩn | Kiểm tra `Target Menu` đã được gán chưa |
| Lỗi null reference | Đảm bảo MenuToggler và target menu được set trước khi nhấp |
| Animation không hoạt động | Bật `Use Animation` và kiểm tra duration > 0 |
| Không thấy log | Check Console (Ctrl+Shift+C) |

---

## 📁 File Location
- `Assets/Scripts/Systems/MenuToggler.cs`
- `Assets/Scripts/Systems/MenuExitButton.cs`
- `Assets/Scripts/Systems/SettingsMenuManager.cs`
