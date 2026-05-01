using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class StaffAI : MonoBehaviour
{
    [Header("Thiết lập Staff")]
    [SerializeField] private float checkInterval = 1f; // Kiểm tra khách hàng mỗi 1 giây
    [SerializeField] private float distanceThreshold = 0.5f; // Khoảng cách để xem đã đến khách
    [SerializeField] private List<ObjectnType> foodToCarry = new List<ObjectnType>(); // List đồ ăn để mang
    [SerializeField] private Transform staffRestPoint; // Vị trí Staff đi nghỉ

    private NavMeshAgent agent;
    private Animator animator;
    private CustomerAI targetCustomer;
    private ItemBox kitchenDesk;
    private ChefAI chef; // ✅ Chef reference
    private float checkTimer = 0f;
    private ItemType currentFoodType; // Loại đồ ăn đang mang
    
    // ✅ ItemBox periodic check fields
    [SerializeField] private float itemBoxCheckInterval = 3f; // Kiểm tra ItemBox mỗi 3 giây
    private float itemBoxCheckTimer = 0f; // Timer cho ItemBox check

    public enum StaffState { Idle, Walking, Serving, GoingToKitchen, TakingFood, ReturningToCustomer, GoingToRest }
    public StaffState currentState = StaffState.Idle;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        // Tìm ItemBox (Kitchen Desk)
        GameObject sellAreaObj = GameObject.FindGameObjectWithTag("SellArea");
        if (sellAreaObj != null)
        {
            kitchenDesk = sellAreaObj.GetComponent<ItemBox>();
        }

        // ✅ Tìm Chef
        chef = FindFirstObjectByType<ChefAI>();
        if (chef == null)
        {
            Debug.LogWarning("⚠️ Chef not found in scene!");
        }

        // ✅ Set animation về Idle khi start
        SetAnimationWalking(false);
        SetAnimationOrdering(false);

        // ✅ MỚI: Apply tốc độ nâng cấp từ StaffUpgradeManager
        ApplyServiceSpeed();
    }

    /// <summary>
    /// ✅ MỚI: Apply tốc độ nâng cấp từ StaffUpgradeManager
    /// </summary>
    public void ApplyServiceSpeed()
    {
        if (agent == null)
            return;

        if (StaffUpgradeManager.Instance == null)
            return;

        float speedMultiplier = StaffUpgradeManager.Instance.GetCurrentServiceSpeedValue();
        float baseSpeed = 7.0f;  // Tốc độ base từ prefab
        agent.speed = baseSpeed * speedMultiplier;

        Debug.Log($"[StaffAI] ✅ Apply Service Speed: {speedMultiplier}x, Final Speed: {agent.speed}");
    }

    /// <summary>
    /// ✅ Set animation Walking (walk/idle transition)
    /// </summary>
    private void SetAnimationWalking(bool isWalking)
    {
        if (animator != null)
        {
            animator.SetBool("IsWalking", isWalking);
            Debug.Log($"Animation: IsWalking = {isWalking}");
        }
    }

    /// <summary>
    /// ✅ Set animation Ordering (nhận đơn)
    /// </summary>
    private void SetAnimationOrdering(bool isOrdering)
    {
        if (animator != null)
        {
            animator.SetBool("IsOrder", isOrdering);
            Debug.Log($"Animation: IsOrder = {isOrdering}");
        }
    }

    void Update()
    {
        // Kiểm tra xem có khách hàng nào chọn món rồi không
        checkTimer += Time.deltaTime;

        if (checkTimer >= checkInterval)
        {
            FindCustomerNeedingService();
            checkTimer = 0f;
        }

        // ✅ MỚI: Kiểm tra ItemBox mỗi 3 giây (chỉ khi Staff Idle hoặc Going to Rest)
        itemBoxCheckTimer += Time.deltaTime;
        if (itemBoxCheckTimer >= itemBoxCheckInterval)
        {
            CheckItemBoxAndPickup();
            itemBoxCheckTimer = 0f;
        }

        // --- CẮT NGẮN VIỆC ĐI NGHỈ NẾU CÓ KHÁCH GỌIMÓN ---
        if (currentState == StaffState.GoingToRest && agent != null && staffRestPoint != null)
        {
            // Kiểm tra liên tục xem có khách cần service không
            CustomerAI[] allCustomers = FindObjectsByType<CustomerAI>(FindObjectsSortMode.None);
            foreach (CustomerAI customer in allCustomers)
            {
                if (customer.IsWaitingForFood && !customer.hasBeenServedByStaff)
                {
                    // Có khách cần service, dừng đi nghỉ
                    Debug.Log("Staff nhận ra có khách gọi món, hủy việc đi nghỉ và đi phục vụ");
                    targetCustomer = customer;
                    WalkToCustomer();
                    return;
                }
            }
        }

        // Xử lý trạng thái đi lại để nhận đơn
        if (currentState == StaffState.Walking && agent != null && targetCustomer != null)
        {
            if (agent.isActiveAndEnabled)
            {
                if (!agent.pathPending && agent.remainingDistance <= distanceThreshold)
                {
                    if (!agent.hasPath || agent.velocity.sqrMagnitude < 0.2f)
                    {
                        agent.isStopped = true;
                        // ✅ Chuyển animation thành Idle (IsWalking=false, IsOrder=false) trước gọi ReceiveOrder
                        SetAnimationWalking(false);
                        SetAnimationOrdering(true);

                        currentState = StaffState.Serving;
                        ReceiveOrder();
                    }
                }
            }
        }

        // Xử lý trạng thái đi tới kitchen
        if (currentState == StaffState.GoingToKitchen && agent != null && kitchenDesk != null)
        {
            if (agent.isActiveAndEnabled)
            {
                if (!agent.pathPending && agent.remainingDistance <= distanceThreshold)
                {
                    if (!agent.hasPath || agent.velocity.sqrMagnitude < 0.2f)
                    {
                        agent.isStopped = true;
                        // ✅ Chuyển animation thành Idle (IsWalking=false, IsOrder=false) trước gọi TakeFoodFromKitchen
                        SetAnimationWalking(false);
                        SetAnimationOrdering(true);

                        currentState = StaffState.TakingFood;
                        TakeFoodFromKitchen();
                    }
                }
            }
        }

        // Xử lý trạng thái quay lại khách
        if (currentState == StaffState.ReturningToCustomer && agent != null && targetCustomer != null)
        {
            if (agent.isActiveAndEnabled)
            {
                if (!agent.pathPending && agent.remainingDistance <= distanceThreshold)
                {
                    if (!agent.hasPath || agent.velocity.sqrMagnitude < 0.2f)
                    {
                        agent.isStopped = true;
                        // ✅ Chuyển animation thành Idle (IsWalking=false, IsOrder=false)
                        SetAnimationWalking(false);
                        SetAnimationOrdering(true);

                        ServeFood();
                    }
                }
            }
        }

        // Xử lý trạng thái đi tới vị trí nghỉ
        if (currentState == StaffState.GoingToRest && agent != null && staffRestPoint != null)
        {
            if (agent.isActiveAndEnabled)
            {
                if (!agent.pathPending && agent.remainingDistance <= distanceThreshold)
                {
                    if (!agent.hasPath || agent.velocity.sqrMagnitude < 0.2f)
                    {
                        agent.isStopped = true;
                        // ✅ Chuyển animation thành Idle (IsWalking=false, IsOrder=false)
                        SetAnimationWalking(false);
                        SetAnimationOrdering(true);

                        // ✅ QUAN TRỌNG: Set state ngay trước gọi coroutine
                        currentState = StaffState.Idle;
                        Debug.Log("Staff đã tới vị trí nghỉ, chuyển về Idle");
                        // ⏳ Sau 1 giây, về trạng thái Idle để có thể tìm khách tiếp theo
                        StartCoroutine(ReturnToIdleAfterDelay(1f));
                    }
                }
            }
        }
    }

    /// <summary>
    /// Tìm khách hàng cần service (đã chọn món nhưng chưa được nhân viên tới)
    /// Nếu không có khách nào cần service, staff sẽ đi nghỉ tại Staff_RestPoint
    /// </summary>
    private void FindCustomerNeedingService()
    {
        // ✅ Nếu đang phục vụ khách (ở bất kỳ stage nào), bỏ qua
        if (currentState == StaffState.Serving || 
            currentState == StaffState.Walking ||
            currentState == StaffState.GoingToKitchen ||
            currentState == StaffState.TakingFood ||
            currentState == StaffState.ReturningToCustomer ||
            currentState == StaffState.GoingToRest)
            return;

        // Tìm tất cả khách hàng trong scene
        CustomerAI[] allCustomers = FindObjectsByType<CustomerAI>(FindObjectsSortMode.None);
        bool foundCustomerNeedingService = false;

        foreach (CustomerAI customer in allCustomers)
        {
            // Kiểm tra xem khách có đang chờ đơn (đã chọn món) không
            if (customer.IsWaitingForFood && !customer.hasBeenServedByStaff)
            {
                targetCustomer = customer;
                WalkToCustomer();
                foundCustomerNeedingService = true;
                break;
            }
        }

        // Nếu không tìm thấy khách nào cần service và chưa đi nghỉ, thì đi tới Staff_RestPoint
        if (!foundCustomerNeedingService && currentState == StaffState.Idle && staffRestPoint != null)
        {
            WalkToRestPoint();
        }
    }

    /// <summary>
    /// ✅ Kiểm tra ItemBox mỗi 3 giây và lấy đồ ăn nếu có sẵn
    /// </summary>
    private void CheckItemBoxAndPickup()
    {
        // ✅ Điều kiện DISABLED: Không kiểm tra trong những trường hợp này
        if (currentState == StaffState.Walking ||        // Đang đi tới khách để nhận đơn
            currentState == StaffState.Serving ||        // Đang chốt đơn tại khách
            currentState == StaffState.GoingToKitchen || // Đang đi tới kitchen
            currentState == StaffState.TakingFood ||     // Đang nhận đồ ăn tại kitchen
            currentState == StaffState.ReturningToCustomer) // Đang mang đồ ăn quay lại khách
        {
            return;
        }

        // ✅ Nếu staff đang mang đồ ăn từ lệnh đơn cũ, bỏ qua
        if (currentFoodType != ItemType.NONE)
        {
            return;
        }

        // ✅ Kiểm tra ItemBox có đồ ăn không
        if (kitchenDesk == null)
            return;

        ItemType foodInBox = kitchenDesk.GetCurrentType();
        
        // Nếu không có đồ ăn trong ItemBox, bỏ qua
        if (foodInBox == ItemType.NONE)
        {
            return;
        }

        // ✅ Có đồ ăn trong ItemBox, tìm customer cần loại đồ ăn này
        CustomerAI targetForThisFood = FindCustomerWaitingForFood(foodInBox);
        
        if (targetForThisFood != null)
        {
            // Có customer chờ loại đồ ăn này
            targetCustomer = targetForThisFood;
            currentFoodType = foodInBox;
            WalkToKitchen();
            Debug.Log($"✅ Staff nhận ra có đồ ăn {foodInBox} trong ItemBox, đi nhận");
        }
        else
        {
            Debug.Log($"⚠️ Có đồ ăn {foodInBox} trong ItemBox nhưng không tìm thấy customer chờ");
        }
    }

    /// <summary>
    /// Tìm customer đang chờ loại đồ ăn cụ thể
    /// </summary>
    private CustomerAI FindCustomerWaitingForFood(ItemType foodType)
    {
        CustomerAI[] allCustomers = FindObjectsByType<CustomerAI>(FindObjectsSortMode.None);
        
        foreach (CustomerAI customer in allCustomers)
        {
            // Tìm customer có currentOrder khớp với foodType và đang chờ (ConfirmOrder đã được gọi)
            if (customer.currentOrder == foodType && customer.IsWaitingForFood)
            {
                return customer;
            }
        }
        
        return null;
    }

    /// <summary>
    /// Staff đi tới vị trí khách hàng
    /// </summary>
    private void WalkToCustomer()
    {
        if (agent == null || targetCustomer == null)
            return;

        currentState = StaffState.Walking;

        // ✅ Chuyển animation: Walk (IsWalking=true, IsOrder=false)
        SetAnimationWalking(true);
        SetAnimationOrdering(false);

        // Đặt đích đến
        agent.isStopped = false;
        agent.SetDestination(targetCustomer.transform.position);

        Debug.Log("Staff bắt đầu đi tới khách hàng để nhận đơn");
    }

    /// <summary>
    /// Staff đi tới vị trí nghỉ (Staff_RestPoint)
    /// </summary>
    private void WalkToRestPoint()
    {
        if (agent == null || staffRestPoint == null)
            return;

        currentState = StaffState.GoingToRest;

        // ✅ Chuyển animation: Walk (IsWalking=true, IsOrder=false)
        SetAnimationWalking(true);
        SetAnimationOrdering(false);

        // Đặt đích đến
        agent.isStopped = false;
        agent.SetDestination(staffRestPoint.position);

        Debug.Log("Staff không có khách để phục vụ, đi tới vị trí nghỉ");
    }

    /// <summary>
    /// Staff đã đến khách hàng, nhận đơn
    /// </summary>
    private void ReceiveOrder()
    {
        if (targetCustomer == null)
            return;

        // ✅ Dừng lại và chuyển animation thành Idle + Ordering (IsWalking=false, IsOrder=true)
        agent.isStopped = true;
        SetAnimationWalking(false);
        SetAnimationOrdering(true);

        Debug.Log("Staff đã đến khách hàng, bắt đầu chốt đơn...");

        // 🎯 Bắt đầu countdown ngay khi Staff đến (vòng tròn bắt đầu quay)
        StartCoroutine(StartCountdownWhenStaffArrive());
        
        // Chốt đơn trong 3 giây
        StartCoroutine(ConfirmOrderAfterDelay(3f));
    }

    /// <summary>
    /// Bắt đầu countdown ngay khi Staff đến khách hàng
    /// </summary>
    private IEnumerator StartCountdownWhenStaffArrive()
    {
        // Tìm CountDown script từ Customer
        CountDown countDown = targetCustomer.GetComponentInChildren<CountDown>();
        if (countDown != null)
        {
            yield return null;  // Đợi một frame để activation xử lý
            
            // Bắt đầu countdown với duration là 3 giây
            countDown.SetDurationAndStartCountdown(3f);
            Debug.Log("🎯 Vòng tròn bắt đầu quay ngay khi Staff đến!");
        }
        else
        {
            Debug.LogWarning("⚠️ CountDown không tìm thấy!");
        }
    }

    /// <summary>
    /// Chốt đơn sau 3 giây
    /// </summary>
    private IEnumerator ConfirmOrderAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (targetCustomer == null)
            yield break;

        // Lưu loại đồ ăn để lấy từ kitchen
        currentFoodType = targetCustomer.currentOrder;

        // Chuyển customer sang trạng thái chờ đồ ăn
        targetCustomer.ConfirmOrder();
        Debug.Log("Staff đã chốt đơn xong, customer chuyển sang waiting. Chef sẽ nấu: " + currentFoodType);

        // ✅ ĐƠN GIẢN: Ẩn Order_bubble_Menu ngay khi staff chốt đơn
        // CountDown đã được kích hoạt ở StartCountdownWhenStaffArrive(), nên vẫn chạy ở background
        CustomerUI customerUI = targetCustomer.GetComponent<CustomerUI>();
        if (customerUI != null)
        {
            customerUI.HideOrderBubbleMenuContainer();
            Debug.Log("✅ Gọi HideOrderBubbleMenuContainer() từ CustomUI");
        }
        else
        {
            Debug.LogWarning("⚠️ CustomerUI component không tìm thấy!");
        }
        
        // Đợi một chút để CountDown ở background kịp xử lý
        yield return null;

        // ✅ TÔI MỚI - Thông báo Chef để nấu ăn
        if (chef != null)
        {
            chef.AddOrderToQueue(currentFoodType);
            Debug.Log("👨‍🍳 Chef đã nhận lệnh nấu: " + currentFoodType);
        }
        else
        {
            Debug.LogWarning("⚠️ Chef not found! Cannot send order to chef");
        }

        // ✅ Chuyển animation về Walk (IsWalking=true, IsOrder=false)
        SetAnimationWalking(true);
        SetAnimationOrdering(false);
        agent.isStopped = false;

        // ✅ CHÍNH: Bây giờ Staff chờ đến khi Chef nấu xong, rồi mới đi kitchen
        // Sử dụng coroutine để kiểm tra mỗi 0.5 giây xem food có sẵn trong ItemBox không
        yield return StartCoroutine(WaitForFoodToBeReady());
    }

    /// <summary>
    /// ✅ SỬALO: Kiểm tra các lần một xem ItemBox có đồ ăn cần giao không
    /// Nếu chưa sẵn sàng, staff quay về Idle để nhận đơn khác
    /// </summary>
    private IEnumerator WaitForFoodToBeReady()
    {
        // ✅ CHỈ KIỂM TRA MỘT LẦN khi nhận đơn thành công
        yield return new WaitForSeconds(0.5f);

        // Kiểm tra xem ItemBox có chứa món cần lấy không
        if (kitchenDesk != null && kitchenDesk.GetCurrentType() == currentFoodType)
        {
            Debug.Log("✅ Food is ready! Staff will go take it from kitchen");
            WalkToKitchen();
        }
        else
        {
            // ✅ SỬALO: Nếu chưa sẵn sàng, quay về Idle để nhận đơn khác
            Debug.Log("⚠️ Food not ready yet! Staff returns to Idle to accept other orders");
            targetCustomer = null;
            StartCoroutine(ReturnToIdleAfterDelay(0f));
        }
    }

    /// <summary>
    /// Staff đi tới kitchen để lấy đồ ăn
    /// </summary>
    private void WalkToKitchen()
    {

        currentState = StaffState.GoingToKitchen;

        // ✅ QUAN TRỌNG: Bắt đầu di chuyển (agent.isStopped = false)
        agent.isStopped = false;
        agent.SetDestination(kitchenDesk.transform.position);

        Debug.Log("Staff bắt đầu đi tới kitchen để lấy đồ ăn");
    }

    /// <summary>
    /// Staff nhận hàng từ kitchen
    /// </summary>
    private void TakeFoodFromKitchen()
    {
        if (kitchenDesk == null)
        {
            Debug.LogError("Staff không có Kitchen Desk!");
            ReturnToCustomerWithoutFood();
            return;
        }

        // ✅ Dừng lại và chuyển animation thành Idle (IsWalking=false, IsOrder=true - Khi tại itembox)
        agent.isStopped = true;
        SetAnimationWalking(false);
        SetAnimationOrdering(true);

        // ✅✅✅ QUAN TRỌNG: Thực sự LẤY food từ queue (Dequeue)
        ItemType takenFood = kitchenDesk.GetItem();
        
        if (takenFood == ItemType.NONE)
        {
            Debug.LogWarning("⚠️ Queue rỗng! Staff không lấy được đồ ăn. Quay lại customer");
            ReturnToCustomerWithoutFood();
            return;
        }
        
        if (takenFood != currentFoodType)
        {
            Debug.LogWarning("⚠️ Mismatch: Queue có " + takenFood + " nhưng expect " + currentFoodType);
            currentFoodType = takenFood;
        }

        Debug.Log("✅ Staff lấy thành công từ queue: " + takenFood);

        // Hiển thị model đồ ăn trên staff
        ShowFoodModel(currentFoodType);

        // ✅ Chuyển animation về Walk (IsWalking=true, IsOrder=false)
        SetAnimationWalking(true);
        SetAnimationOrdering(false);
        agent.isStopped = false;

        // Sau 1 giây, quay lại khách
        StartCoroutine(ReturnToCustomerAfterDelay(1f));
    }

    /// <summary>
    /// Fallback: Quay lại customer nếu không lấy được food
    /// </summary>
    private void ReturnToCustomerWithoutFood()
    {
        if (targetCustomer == null)
        {
            Debug.LogError("Staff không có targetCustomer!");
            StartCoroutine(ReturnToIdleAfterDelay(0f));
            return;
        }

        Debug.Log("Staff quay lại customer (không có food)");
        currentState = StaffState.ReturningToCustomer;
        
        // ✅ Chuyển animation về Walk (IsWalking=true, IsOrder=false)
        SetAnimationWalking(true);
        SetAnimationOrdering(false);

        agent.isStopped = false;
        agent.SetDestination(targetCustomer.transform.position);
    }


    /// <summary>
    /// Quay lại khách sau delay
    /// </summary>
    private IEnumerator ReturnToCustomerAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (targetCustomer == null)
        {
            // ✅ Nếu không có targetCustomer (autonomous pickup không tìm đúng customer)
            // Reset food type và quay lại Idle
            Debug.Log("⚠️ targetCustomer is null, resetting food and returning to Idle");
            currentFoodType = ItemType.NONE;
            StartCoroutine(ReturnToIdleAfterDelay(0f));
            yield break;
        }

        currentState = StaffState.ReturningToCustomer;

        // ✅ Bắt đầu di chuyển (agent.isStopped = false)
        agent.isStopped = false;
        agent.SetDestination(targetCustomer.transform.position);

        Debug.Log("Staff mang đồ ăn quay lại khách hàng");
    }

    /// <summary>
    /// Staff đã đến khách để phục vụ đồ ăn
    /// </summary>
    private void ServeFood()
    {
        if (targetCustomer == null)
            return;

        // ✅ Dừng lại và chuyển animation thành Idle (IsWalking=false, IsOrder=false)
        agent.isStopped = true;
        SetAnimationWalking(false);
        SetAnimationOrdering(true);

        Debug.Log("Staff đã phục vụ đồ ăn cho khách");

        // Gọi ReceiveFood trên customer
        bool success = targetCustomer.ReceiveFood(currentFoodType);

        if (success)
        {
            // ✅ Tắt model đồ ăn khỏi staff (không destroy)
            HideAllFoodModels();
            // ✅ Reset currentFoodType sau khi giao đồ ăn thành công
            currentFoodType = ItemType.NONE;
            Debug.Log("Khách đã nhận đúng món ăn!");
        }
        else
        {
            Debug.Log("Khách từ chối, quay lại kitchen");
        }

        // Quay lại idle
        StartCoroutine(ReturnToIdleAfterDelay(1f));
    }

    /// <summary>
    /// Quay lại trạng thái Idle sau 1 chút thời gian
    /// </summary>
    private IEnumerator ReturnToIdleAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (agent != null)
        {
            agent.isStopped = true;
        }

        // ✅ Đặt animation thành Idle (IsWalking=false, IsOrder=false)
        SetAnimationWalking(false);
        SetAnimationOrdering(false);

        // ✅ Reset food type khi quay lại Idle
        currentFoodType = ItemType.NONE;
        currentState = StaffState.Idle;
        targetCustomer = null;

        Debug.Log("Staff quay lại trạng thái Idle");
    }

    /// <summary>
    /// Lấy trạng thái hiện tại
    /// </summary>
    public StaffState GetCurrentState()
    {
        return currentState;
    }

    /// <summary>
    /// Hiển thị model đồ ăn trên staff (bật/tắt giống Inventory)
    /// </summary>
    private void ShowFoodModel(ItemType foodType)
    {
        bool foundModel = false;

        foreach (ObjectnType foodItem in foodToCarry)
        {
            if (foodItem.type != foodType)
            {
                if (foodItem.item != null) foodItem.item.SetActive(false);
            }
            else
            {
                if (foodItem.item != null)
                {
                    foodItem.item.SetActive(true);
                    foundModel = true;
                    Debug.Log("Bật model cho món: " + foodType);
                }
            }
        }

        if (!foundModel)
        {
            Debug.LogWarning("Không tìm thấy model cho món: " + foodType);
        }
    }

    /// <summary>
    /// Tắt tất cả model đồ ăn trên staff
    /// </summary>
    private void HideAllFoodModels()
    {
        foreach (ObjectnType foodItem in foodToCarry)
        {
            if (foodItem.item != null) foodItem.item.SetActive(false);
        }
    }

    /// <summary>
    /// Helper: Tìm Transform theo tên (tìm sâu/recursive)
    /// </summary>
    private Transform FindTransformRecursive(Transform parent, string name)
    {
        if (parent.name == name)
            return parent;

        foreach (Transform child in parent)
        {
            Transform result = FindTransformRecursive(child, name);
            if (result != null)
                return result;
        }

        return null;
    }

    /// <summary>
    /// Helper: Lấy đường dẫn đầy đủ của object (ví dụ: "Customer1/root/OderClockCircle")
    /// </summary>
    private string GetPathToObject(Transform transform)
    {
        string path = transform.name;
        Transform parent = transform.parent;
        while (parent != null && parent != transform.root)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }
        return path;
    }
}
