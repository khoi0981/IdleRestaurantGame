using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OvenStation : Functionality
{
    [Header("Oven References")]
    [SerializeField] private ItemBox kitchenDesk; // Quầy bếp để đặt đồ ăn nấu xong
    [SerializeField] private Transform ovenPosition; // Vị trí oven để Chef đi tới
    [SerializeField] private GameObject cookingBubbleUI; // Bubble UI hiển thị đồ ăn đang nấu
    [SerializeField] private Image cookingFoodImageComponent; // Image component để hiển thị ảnh món ăn đang nấu
    [SerializeField] private FoodDataList foodDataList; // Reference tới FoodDataList để lấy food data
    [SerializeField] private OrderClockRotation cookingClockRotation; // Reference tới OrderClockRotation để quay vòng tròn nấu
    [SerializeField] private CountDown countDownComponent; // CountDown component để quản lý countdown (giống Customer bubble)
    
    private Queue<ItemType> cookingQueue = new Queue<ItemType>();
    private ItemType currentCookingFood = ItemType.NONE;
    private float cookTime = 3f; // 3 giây nấu một món
    
    private void Start()
    {
        if (kitchenDesk == null)
        {
            GameObject sellAreaObj = GameObject.FindGameObjectWithTag("SellArea");
            if (sellAreaObj != null)
            {
                kitchenDesk = sellAreaObj.GetComponent<ItemBox>();
            }
        }

        if (ovenPosition == null)
        {
            ovenPosition = transform;
        }

        // ✅ Auto-find timer nếu chưa assign
        if (timer == null)
        {
            timer = GetComponent<UITimer>();
            if (timer == null)
            {
                Debug.LogWarning("⚠️ UITimer not found in OvenStation. Assigning manually or create UITimer component!");
            }
        }

        // ✅ Auto-find FoodDataList nếu chưa được assign
        if (foodDataList == null)
        {
            FoodDataList[] allLists = Resources.FindObjectsOfTypeAll<FoodDataList>();
            if (allLists.Length > 0)
            {
                foodDataList = allLists[0];
                Debug.Log($"✅ Tự động tìm thấy FoodDataList: {foodDataList.name}");
            }
            else
            {
                Debug.LogWarning("⚠️ Không tìm thấy FoodDataList trong Resources! Hãy assign nó vào OvenStation hoặc đặt trong Resources folder.");
            }
        }

        // ✅ Auto-find Image component nếu chưa được assign
        if (cookingFoodImageComponent == null && cookingBubbleUI != null)
        {
            // Tìm Image trong children
            cookingFoodImageComponent = cookingBubbleUI.GetComponentInChildren<Image>();
            
            // Nếu vẫn không tìm thấy, tìm theo tên
            if (cookingFoodImageComponent == null)
            {
                Transform foodImageTrans = cookingBubbleUI.transform.Find("FoodImage");
                if (foodImageTrans != null)
                {
                    cookingFoodImageComponent = foodImageTrans.GetComponent<Image>();
                }
            }
            
            if (cookingFoodImageComponent != null)
            {
                Debug.Log($"✅ Tự động tìm thấy Image component: {cookingFoodImageComponent.gameObject.name}");
            }
            else
            {
                Debug.LogWarning("⚠️ Không tìm thấy Image component! Kiểm tra cấu trúc bubble hoặc assign thủ công.");
            }
        }

        // ✅ Auto-find OrderClockRotation nếu chưa được assign
        if (cookingClockRotation == null && cookingBubbleUI != null)
        {
            // Tìm trong children của cooking bubble
            cookingClockRotation = cookingBubbleUI.GetComponentInChildren<OrderClockRotation>();
            
            // Nếu vẫn không tìm thấy, tìm trong Oven container
            if (cookingClockRotation == null)
            {
                cookingClockRotation = GetComponentInChildren<OrderClockRotation>();
            }
            
            if (cookingClockRotation != null)
            {
                Debug.Log($"✅ Tự động tìm thấy OrderClockRotation: {cookingClockRotation.gameObject.name}");
            }
            else
            {
                Debug.LogWarning("⚠️ Không tìm thấy OrderClockRotation! Kiểm tra cấu trúc bubble hoặc assign thủ công.");
            }
        }

        // ✅ Auto-find CountDown component nếu chưa được assign
        if (countDownComponent == null && cookingBubbleUI != null)
        {
            // Tìm CountDown trong children của cooking bubble (giống như Customer bubble)
            countDownComponent = cookingBubbleUI.GetComponentInChildren<CountDown>();
            
            // Nếu vẫn không tìm thấy, tìm trong Oven container
            if (countDownComponent == null)
            {
                countDownComponent = GetComponentInChildren<CountDown>();
            }
            
            if (countDownComponent != null)
            {
                Debug.Log($"✅ Tự động tìm thấy CountDown component: {countDownComponent.gameObject.name}");
            }
            else
            {
                Debug.LogWarning("⚠️ Không tìm thấy CountDown component! Hãy add CountDown script vào OrderClockCircle hoặc assign thủ công trong Inspector.");
            }
        }

        // ✅ Auto-attach Billboard script để cooking bubble luôn hướng về camera (giống customer bubble)
        if (cookingBubbleUI != null)
        {
            // Tìm parent container của cooking bubble
            Transform bubbleContainer = cookingBubbleUI.transform.parent;
            
            if (bubbleContainer != null)
            {
                // Kiểm tra xem đã có Billboard chưa
                Billboard billboard = bubbleContainer.GetComponent<Billboard>();
                if (billboard == null)
                {
                    // Thêm Billboard script nếu chưa có
                    bubbleContainer.gameObject.AddComponent<Billboard>();
                    Debug.Log($"✅ Tự động thêm Billboard script vào: {bubbleContainer.gameObject.name}");
                }
                else
                {
                    Debug.Log($"✅ Billboard script đã tồn tại trên: {bubbleContainer.gameObject.name}");
                }
            }
        }

        if (cookingBubbleUI != null)
            cookingBubbleUI.SetActive(false);

        maxTime = cookTime;
    }

    /// <summary>
    /// Thêm món ăn vào hàng chờ nấu
    /// </summary>
    public void AddFoodToCookingQueue(ItemType foodType)
    {
        if (foodType == ItemType.NONE)
        {
            Debug.LogWarning("❌ Cannot add NONE food type to cooking queue");
            return;
        }

        cookingQueue.Enqueue(foodType);
        Debug.Log($"🍳 Food added to cooking queue: {foodType}. Queue size: {cookingQueue.Count}");

        // Nếu chưa nấu gì, bắt đầu nấu món đầu
        if (currentCookingFood == ItemType.NONE && cookingQueue.Count > 0)
        {
            StartCooking();
        }
    }

    /// <summary>
    /// Bắt đầu nấu món từ queue
    /// </summary>
    private void StartCooking()
    {
        if (cookingQueue.Count == 0)
        {
            Debug.LogWarning("⚠️ No food in queue to cook");
            return;
        }

        currentCookingFood = cookingQueue.Dequeue();
        currentTime = 0f;
        processStarted = true;

        // 🚫 Bubble sẽ được hiển thị bởi ChefAI khi nó đã tới oven, không phải ở đây
        // Xem: ChefAI.StartCooking() → ShowCookingBubble()

        Debug.Log($"🍳 Started cooking: {currentCookingFood}. Time: {cookTime}s (bubble sẽ hiển thị khi chef tới oven)");
    }

    /// <summary>
    /// ✨ Hiển thị cooking bubble khi Chef đã tới oven
    /// Được gọi từ ChefAI khi nó vào trạng thái Cooking
    /// </summary>
    public void ShowCookingBubble()
    {
        if (currentCookingFood == ItemType.NONE)
        {
            Debug.LogWarning("⚠️ No food cooking to display bubble");
            return;
        }

        // Hiển thị bubble
        if (cookingBubbleUI != null)
        {
            cookingBubbleUI.SetActive(true);
            // Cập nhật model trong bubble nếu cần (tuỳ vào UI design)
            UpdateCookingBubbleDisplay();
        }
        else
        {
            Debug.LogWarning("⚠️ cookingBubbleUI is null, cannot display bubble");
        }

        if (timer != null)
        {
            timer.gameObject.SetActive(true);
            timer.UpdateClock(0f, cookTime);
        }
        else
        {
            Debug.LogWarning("⚠️ Timer is null in ShowCookingBubble");
        }

        // ✅ Sử dụng CountDown component để bắt đầu countdown (giống Customer bubble)
        // CountDown sẽ tự động:
        //  1. Gọi orderClockRotation.StartClockRotation()
        //  2. Cập nhật fillAmount mỗi frame
        //  3. Cập nhật timeText mỗi giây
        if (countDownComponent != null)
        {
            countDownComponent.SetDurationAndStartCountdown(cookTime);
            Debug.Log($"✅ CountDown bắt đầu với duration: {cookTime}s (Cooking: {currentCookingFood})");
        }
        else
        {
            // Fallback: nếu không có CountDown, gọi OrderClockRotation trực tiếp
            if (cookingClockRotation != null)
            {
                cookingClockRotation.StartClockRotation(cookTime);
                Debug.LogWarning($"⚠️ CountDown không tìm thấy, fallback to OrderClockRotation trực tiếp");
            }
            else
            {
                Debug.LogError("❌ Cả CountDown và OrderClockRotation đều null! Hãy add CountDown hoặc OrderClockRotation vào Cooking_bubble");
            }
        }

        Debug.Log($"✨ Cooking bubble displayed: {currentCookingFood}");
    }

    /// <summary>
    /// Cập nhật hiển thị cooking bubble với ảnh từ FoodData
    /// </summary>
    private void UpdateCookingBubbleDisplay()
    {
        if (cookingBubbleUI == null || currentCookingFood == ItemType.NONE)
        {
            Debug.LogWarning("❌ cookingBubbleUI is null or no food cooking");
            return;
        }

        // ✅ Map ItemType sang FoodID
        string foodID = ConvertItemTypeToFoodID(currentCookingFood);
        if (string.IsNullOrEmpty(foodID))
        {
            Debug.LogError($"❌ Không thể map ItemType '{currentCookingFood}' sang FoodID!");
            return;
        }

        // ✅ Lấy FoodData từ FoodDataList
        if (foodDataList == null)
        {
            Debug.LogError("❌ FoodDataList is null!");
            return;
        }

        FoodData foodData = foodDataList.GetFoodByID(foodID);
        if (foodData == null)
        {
            Debug.LogWarning($"⚠️ Không tìm thấy FoodData với ID '{foodID}'");
            return;
        }

        // ✅ Set ảnh từ FoodData.Icon
        if (cookingFoodImageComponent != null && foodData.Icon != null)
        {
            cookingFoodImageComponent.sprite = foodData.Icon;
            Debug.Log($"🎨 Cooking bubble updated: {foodData.FoodName} (ID: {foodID})");
        }
        else
        {
            Debug.LogWarning($"⚠️ Không thể set image! cookingFoodImageComponent={cookingFoodImageComponent}, foodData.Icon={foodData.Icon}");
        }
    }

    /// <summary>
    /// Map ItemType sang FoodID
    /// Hỗ trợ: HAMBURGER → "1", COOKEDMEAT → "2"
    /// </summary>
    private string ConvertItemTypeToFoodID(ItemType itemType)
    {
        switch (itemType)
        {
            case ItemType.HAMBURGER:
                return "1";
            case ItemType.COOKEDMEAT:
                return "2";
            // ✅ Thêm mapping khác nếu cần
            default:
                Debug.LogWarning($"⚠️ Không có mapping cho ItemType: '{itemType}'");
                return null;
        }
    }

    /// <summary>
    /// Xử lý nấu ăn mỗi frame
    /// </summary>
    public override ItemType Process()
    {
        if (!processStarted || currentCookingFood == ItemType.NONE)
            return ItemType.NONE;

        currentTime += Time.deltaTime;
        
        if (timer != null)
        {
            timer.UpdateClock(currentTime, cookTime);
            // Debug: Uncomment để xem timer update
            // Debug.Log($"⏱️ Cooking {currentCookingFood}: {currentTime:F1}/{cookTime}s, fillAmount: {currentTime/cookTime:F2}");
        }
        else
        {
            Debug.LogWarning("⚠️ Timer is null in Process()!");
        }

        // Khi nấu xong
        if (currentTime >= cookTime)
        {
            Debug.Log($"✅ Finished cooking {currentCookingFood}!");
            currentTime = 0f;
            if (timer != null)
            {
                timer.UpdateClock(0f, cookTime);
                timer.gameObject.SetActive(false);
            }
            
            ItemType finishedFood = FinishCooking();
            
            // Nấu món tiếp theo nếu có trong queue
            if (cookingQueue.Count > 0)
            {
                StartCooking();
            }
            else
            {
                processStarted = false;
                currentCookingFood = ItemType.NONE;
                if (cookingBubbleUI != null)
                    cookingBubbleUI.SetActive(false);
            }

            return finishedFood;
        }

        return ItemType.NONE;
    }

    /// <summary>
    /// Kết thúc nấu ăn - đặt vào ItemBox
    /// </summary>
    private ItemType FinishCooking()
    {
        ItemType finishedFood = currentCookingFood;

        if (kitchenDesk != null)
        {
            kitchenDesk.SetType(finishedFood);
            Debug.Log($"✅ Cooking finished: {finishedFood} → Added to ItemBox");
        }
        else
        {
            Debug.LogWarning("❌ ItemBox not found! Cannot add cooked food");
        }

        return finishedFood;
    }

    /// <summary>
    /// Xóa dữ liệu khi cần thiết
    /// </summary>
    public override void ClearObject()
    {
        base.ClearObject();
        currentCookingFood = ItemType.NONE;
        currentTime = 0f;
        processStarted = false;
        cookingQueue.Clear();
        
        if (timer != null)
        {
            timer.gameObject.SetActive(false);
        }
        
        if (cookingBubbleUI != null)
            cookingBubbleUI.SetActive(false);
    }

    /// <summary>
    /// Lấy vị trí oven để Chef đi tới
    /// </summary>
    public Transform GetOvenPosition()
    {
        return ovenPosition;
    }

    /// <summary>
    /// Lấy số lượng món đang chờ nấu
    /// </summary>
    public int GetCookingQueueCount()
    {
        return cookingQueue.Count;
    }

    /// <summary>
    /// Lấy số lượng tổng (đang nấu + đang chờ)
    /// </summary>
    public int GetTotalCookingTasks()
    {
        int total = cookingQueue.Count;
        if (currentCookingFood != ItemType.NONE)
            total++;
        return total;
    }

    /// <summary>
    /// Lấy thực phẩm đang nấu
    /// </summary>
    public ItemType GetCurrentCookingFood()
    {
        return currentCookingFood;
    }
}
