using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class CustomerAI : MonoBehaviour
{
    [Header("Order Information")]
    public ItemType currentOrder; // Public để Staff truy cập
    private bool hasOrdered = false;

    [Header("Cài đặt Đồ ăn & Trả tiền")]
    [SerializeField] private GameObject burgerModel;
    [SerializeField] private int foodPrice = 50; // Default giá, sẽ được override bởi FoodData
    [SerializeField] private float eatTime = 10f;
    
    [Header("Dữ liệu Thực phẩm")]
    [SerializeField] private FoodDataList foodDataList; // Reference tới FoodDataList (auto-load từ Resources nếu null)

    [Header("Kết nối Hệ thống")]
    public Transform exitPoint;
    public Transform waitingPoint;
    public ItemBox kitchenDesk;
    private QueueManager queueManager;

    private GameObject mySeat;
    private NavMeshAgent agent;
    private Animator animator;
    private CapsuleCollider capsuleCollider;
    private CustomerUI customerUI;
    private float waitingAtPointTimer = 0f;
    private Vector3 targetQueuePosition;
    private bool isInQueue = false;
    public bool hasBeenServedByStaff = false; // Tracking xem staff đã tới nhận đơn chưa
    public bool IsFirstInQueue = false; // Tracking xem có phải người đứng đầu hàng không
    private bool hasStartedFindingSeat = false; // Tracking xem đã bắt đầu tìm ghế chưa
    private Coroutine seatCheckingCoroutine; // Coroutine để kiểm tra ghế mỗi 5 giây
    private float seatCheckInterval = 5f; // Kiểm tra ghế mỗi 5 giây

    public enum CustomerState { InQueue, WalkingToWaitingPoint, WaitingAtWaitingPoint, WalkingToSeat, WaitingForFood, Eating, Leaving }
    public CustomerState currentState = CustomerState.InQueue;

    // Biến phụ để CustomerUI kiểm tra xem khách có đang đợi đồ không
    public bool IsWaitingForFood => currentState == CustomerState.WaitingForFood;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        customerUI = GetComponent<CustomerUI>();
        queueManager = QueueManager.Instance;

        if (burgerModel != null) burgerModel.SetActive(false);
        if (animator != null) animator.SetBool("Isate", false);

        if (exitPoint == null)
        {
            GameObject exitObj = GameObject.Find("ExitPoint");
            if (exitObj != null) exitPoint = exitObj.transform;
        }

        // --- ĐOẠN CODE TỰ ĐỘNG TÌM WAITING POINT ---
        if (waitingPoint == null)
        {
            GameObject waitingPointObj = GameObject.Find("WaitingPoint");
            if (waitingPointObj != null)
            {
                waitingPoint = waitingPointObj.transform;
            }
        }
        // --- KẾT THÚC ĐOẠN TÌM WAITING POINT ---

        // --- ĐOẠN CODE TỰ ĐỘNG TÌM QUẦY BẾP (ĐÃ SỬA) ---
        if (kitchenDesk == null)
        {
            // Tìm chính xác Object có gắn Tag là "SellArea"
            GameObject sellAreaObj = GameObject.FindGameObjectWithTag("SellArea");

            if (sellAreaObj != null)
            {
                kitchenDesk = sellAreaObj.GetComponent<ItemBox>();
            }

            if (kitchenDesk == null)
            {
                Debug.LogWarning("Khách " + gameObject.name + " không tìm thấy Quầy Bếp (SellArea)!");
            }
        }
        // --- KẾT THÚC ĐOẠN TÌM BẾP ---

        // --- THAM GIA HÀNG CHỜ ---
        if (queueManager != null && agent != null)
        {
            queueManager.AddToQueue(gameObject);
            isInQueue = true;
            if (animator != null) animator.SetBool("IsWalking", true);
            Debug.Log("Khách vừa vào, tham gia vào hàng chờ");
        }
        else
        {
            // Backup: nếu không có QueueManager, đi thẳng tới ghế
            mySeat = FindClosestSeat();
            if (mySeat != null && agent != null)
            {
                mySeat.tag = "Untagged";
                agent.SetDestination(mySeat.transform.position);
                if (animator != null) animator.SetBool("IsWalking", true);
                currentState = CustomerState.WalkingToSeat;
            }
            else
            {
                LeaveRestaurant();
            }
        }
    }

    void Update()
    {
        // --- XỬ LÝ TRẠNG THÁI: TRONG HÀNG CHỜ ---
        if (currentState == CustomerState.InQueue && agent != null && isInQueue)
        {
            if (agent.isActiveAndEnabled)
            {
                // Di chuyển tới vị trí hàng chờ
                if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.3f)
                {
                    if (!agent.hasPath || agent.velocity.sqrMagnitude < 0.2f)
                    {
                        agent.isStopped = true;
                        if (animator != null)
                        {
                            animator.SetBool("IsWalking", false);
                            animator.SetBool("IsWaiting", true);
                        }

                        Debug.Log("Khách đã đến vị trí hàng chờ");
                        
                        // Nếu là người đứng đầu hàng và chưa bắt đầu tìm ghế, thì bắt đầu tìm
                        if (IsFirstInQueue && !hasStartedFindingSeat)
                        {
                            hasStartedFindingSeat = true;
                            StartFindingSeat();
                        }
                    }
                }
            }
        }

        // --- XỬ LÝ TRẠNG THÁI: ĐI TỚI WAITING POINT ---
        if (currentState == CustomerState.WalkingToWaitingPoint && agent != null && waitingPoint != null)
        {
            if (agent.isActiveAndEnabled)
            {
                if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.3f)
                {
                    if (!agent.hasPath || agent.velocity.sqrMagnitude < 0.2f)
                    {
                        agent.isStopped = true;
                        if (animator != null)
                        {
                            animator.SetBool("IsWalking", false);
                            animator.SetBool("IsWaiting", true);
                        }

                        currentState = CustomerState.WaitingAtWaitingPoint;
                        waitingAtPointTimer = 0f;
                        Debug.Log("Khách đã đến Waiting Point, chờ ghế...");
                    }
                }
            }
        }

        // --- XỰ LÝ TRẠNG THÁI: ĐỢI TẠI WAITING POINT ---
        if (currentState == CustomerState.WaitingAtWaitingPoint)
        {
            waitingAtPointTimer += Time.deltaTime;
            if (animator != null)
            {
                animator.SetBool("IsWaiting", true);
                animator.SetBool("IsWalking", false);
            }

          
        }

        // --- XỬ LÝ TRẠNG THÁI: ĐI TỚI GHẾ ---
        if (currentState == CustomerState.WalkingToSeat && agent != null && mySeat != null)
        {
            if (agent.isActiveAndEnabled)
            {
                if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.3f)
                {
                    if (!agent.hasPath || agent.velocity.sqrMagnitude < 0.2f)
                    {
                        agent.isStopped = true;
                        if (animator != null) animator.SetBool("IsWalking", true);

                        currentState = CustomerState.WaitingForFood;
                        Invoke("SitDownAndOrder", 0.5f);
                    }
                }
            }
        }

        if (currentState == CustomerState.Leaving && agent != null)
        {
            if (agent.isActiveAndEnabled && !agent.pathPending && agent.remainingDistance <= 1.0f)
            {
                Destroy(gameObject);
            }
        }
    }

    /// <summary>
    /// Gọi từ QueueManager để khách đi tới vị trí hàng chờ
    /// </summary>
    public void GoToQueuePosition(Vector3 queuePos)
    {
        if (agent != null && currentState == CustomerState.InQueue)
        {
            targetQueuePosition = queuePos;
            agent.SetDestination(queuePos);
            agent.isStopped = false;
            if (animator != null) animator.SetBool("IsWalking", true);
            
            // Chỉ reset hasStartedFindingSeat nếu khách không phải người đứng đầu
            // Nếu trở thành người đứng đầu, flag sẽ được set trong Update khi khách tới vị trí
            if (!IsFirstInQueue)
            {
                hasStartedFindingSeat = false;
            }
            
            Debug.Log($"Khách {gameObject.name} di chuyển tới vị trí hàng chờ tại {queuePos}. IsFirstInQueue: {IsFirstInQueue}");
        }
    }

    /// <summary>
    /// Reset cờ tìm kiếm ghế (được gọi bởi QueueManager nếu cần)
    /// </summary>
    public void ResetFindingSeatFlag()
    {
        hasStartedFindingSeat = false;

        // Dừng coroutine kiểm tra ghế nếu đang chạy
        if (seatCheckingCoroutine != null)
        {
            StopCoroutine(seatCheckingCoroutine);
            seatCheckingCoroutine = null;
            Debug.Log("Dừng coroutine kiểm tra ghế của khách " + gameObject.name);
        }
    }

    /// <summary>
    /// Được gọi khi hàng chờ đầy, khách sẽ rời đi
    /// </summary>
    public void QueueIsFull()
    {
        Debug.Log("Hàng chờ đầy! Khách sẽ rời đi");
        
        if (queueManager != null && isInQueue)
        {
            queueManager.CustomerFinished(gameObject);
            isInQueue = false;
        }

        if (agent != null)
        {
            agent.enabled = true;
        }

        LeaveRestaurant();
    }

    /// <summary>
    /// Khách đầu tiên trong hàng bắt đầu tìm ghế
    /// Sẽ kiểm tra ghế trống mỗi 5 giây
    /// </summary>
    private void StartFindingSeat()
    {
        if (currentState != CustomerState.InQueue || !IsFirstInQueue) return;

        Debug.Log("Khách đứng đầu hàng bắt đầu tìm ghế...");

        // Dừng coroutine cũ nếu đang chạy
        if (seatCheckingCoroutine != null)
        {
            StopCoroutine(seatCheckingCoroutine);
        }

        // Bắt đầu coroutine kiểm tra ghế mỗi 5 giây
        seatCheckingCoroutine = StartCoroutine(CheckForSeatsCoroutine());
    }

    /// <summary>
    /// Coroutine kiểm tra ghế trống mỗi 5 giây
    /// Nếu tìm thấy ghế trống, sẽ đi tới đó
    /// </summary>
    private IEnumerator CheckForSeatsCoroutine()
    {
        while (IsFirstInQueue && currentState == CustomerState.InQueue)
        {
            // Chờ 5 giây trước khi kiểm tra
            yield return new WaitForSeconds(seatCheckInterval);

            Debug.Log("Khách " + gameObject.name + " đang kiểm tra ghế có trống không...");

            // Tìm ghế trống gần nhất
            mySeat = FindClosestSeat();

            if (mySeat != null && agent != null)
            {
                Debug.Log("Tìm thấy ghế trống! Khách " + gameObject.name + " sẽ đi tới ghế");
                
                // Đánh dấu ghế là đã được sử dụng
                mySeat.tag = "Untagged";

                // Bắt đầu đi tới ghế
                if (animator != null)
                {
                    animator.SetBool("IsWaiting", false);
                    animator.SetBool("IsWalking", true);
                }

                agent.isStopped = false;
                agent.SetDestination(mySeat.transform.position);
                currentState = CustomerState.WalkingToSeat;
                
                Debug.Log("Khách " + gameObject.name + " bắt đầu đi tới ghế");
                
                // Dừng coroutine vì đã tìm thấy ghế
                break;
            }
            else
            {
                Debug.Log("Khách " + gameObject.name + " chưa tìm thấy ghế trống, sẽ kiểm tra lại sau 5 giây");
            }
        }

        // Nếu không còn là người đầu tiên hoặc đã thay đổi trạng thái, dừng coroutine
        Debug.Log("Coroutine kiểm tra ghế của khách " + gameObject.name + " đã dừng");
    }

    private void SitDownAndOrder()
    {
        if (capsuleCollider != null) capsuleCollider.isTrigger = true;
        if (agent != null) agent.enabled = false;

        if (mySeat != null)
        {
            if (agent != null && agent.isActiveAndEnabled) agent.updateRotation = false;

            transform.position = mySeat.transform.position;
            transform.rotation = mySeat.transform.rotation;
            transform.position += transform.forward * -0.5f;
        }

        if (animator != null)
        {
            animator.SetBool("IsWalking", false);
            animator.SetBool("IsSitting", true);
        }

        Debug.Log("Khách đã ngồi xuống...");

        // ✅ XÓA KHÁCH KHỎI QUEUE MANAGER KHI NGỒI VÀO GHẾ
        if (QueueManager.Instance != null)
        {
            QueueManager.Instance.CustomerSeated(gameObject);
        }

        // Reset flag để staff biết cần phục vụ
        hasBeenServedByStaff = false;

        // ✅ LẤY DANH SÁCH FOOD ĐÃ MUA TỪ DATABASE
        List<string> purchasedFoodIDs = GetPurchasedFoodsMenu();
        
        // ✅ KIỂM SOÁT CHẶT: CHỈ GỌI MÓN NẾU ĐÃ MUA
        if (purchasedFoodIDs == null || purchasedFoodIDs.Count == 0)
        {
            // ❌ KHÔNG CÓ MÓN NÀO ĐÃ MUA - KHÁCH PHẢI ĐI VỀ
            Debug.LogError("❌ Khách không thể gọi món! Vì chưa có food được mua trong database");
            
            // Khách sẽ rời khỏi vì không có menu
            if (agent != null) agent.enabled = true;
            if (capsuleCollider != null) capsuleCollider.isTrigger = false;
            
            LeaveRestaurant();
            return;
        }

        // ✅ CÓ FOOD ĐÃ MUA - CHỌN NGẪU NHIÊN TỪ DANH SÁCH
        int randomIndex = Random.Range(0, purchasedFoodIDs.Count);
        string selectedFoodID = purchasedFoodIDs[randomIndex];
        
        Debug.Log($"📋 Danh sách food đã mua: {string.Join(", ", purchasedFoodIDs)}");
        Debug.Log($"🎲 Khách chọn: {selectedFoodID}");
        
        // Chuyển đổi food ID sang ItemType
        currentOrder = ConvertFoodIDToItemType(selectedFoodID);
        
        if (currentOrder == ItemType.NONE)
        {
            Debug.LogError($"❌ Lỗi: Không thể mapping Food ID '{selectedFoodID}' sang ItemType");
            if (agent != null) agent.enabled = true;
            if (capsuleCollider != null) capsuleCollider.isTrigger = false;
            LeaveRestaurant();
            return;
        }

        hasOrdered = false;
        
        // ✅ LẤY GIÁ TỪ FOODDATA
        FoodData foodData = GetFoodDataByID(selectedFoodID);
        if (foodData != null)
        {
            foodPrice = foodData.Price;
            Debug.Log($"✅ Khách gọi món: {selectedFoodID} (ItemType: {currentOrder}, Price: {foodPrice} VND)");
        }
        else
        {
            Debug.LogWarning($"⚠️ Không tìm thấy FoodData cho ID: {selectedFoodID}, dùng default price: {foodPrice}");
        }

        // ✅ HIỂN THỊ BUBBLE
        if (customerUI != null)
        {
            customerUI.ShowOrderBubble(currentOrder);
            Debug.Log("✨ Bubble gọi món được hiển thị");
        }
        else
        {
            Debug.LogWarning("CustomerUI chưa được gán");
        }
    }

    /// <summary>
    /// Lấy danh sách Food đã mua từ DataManager
    /// </summary>
    private List<string> GetPurchasedFoodsMenu()
    {
        if (DataManager.Instance != null)
        {
            return DataManager.Instance.GetPurchasedFoodIDs();
        }
        
        Debug.LogWarning("DataManager không tìm thấy!");
        return new List<string>();
    }

    /// <summary>
    /// Tìm FoodData theo Food ID
    /// </summary>
    private FoodData GetFoodDataByID(string foodID)
    {
        // 🔥 Nếu chưa có foodDataList, tìm bằng cách khác
        if (foodDataList == null)
        {
            // Cách 1: Tìm trong toàn bộ project (nếu FoodDataList là ScriptableObject duy nhất)
            FoodDataList[] allLists = Resources.FindObjectsOfTypeAll<FoodDataList>();
            if (allLists.Length > 0)
            {
                foodDataList = allLists[0];
                Debug.Log($"🔍 Tìm thấy FoodDataList: {foodDataList.name}");
            }
            else
            {
                Debug.LogError("❌ FoodDataList không tìm thấy! Hãy assign nó vào Customer Prefab!");
                return null;
            }
        }
        
        if (foodDataList != null)
        {
            FoodData food = foodDataList.GetFoodByID(foodID);
            if (food != null)
            {
                Debug.Log($"✅ Tìm thấy FoodData: ID={foodID}, Name={food.FoodName}, Price={food.Price}");
                return food;
            }
            else
            {
                Debug.LogWarning($"⚠️ Không tìm thấy Food ID '{foodID}' trong FoodDataList!");
            }
        }
        
        return null;
    }

    /// <summary>
    /// Chuyển đổi Food ID sang ItemType
    /// Mapping: Food ID → ItemType
    /// Hỗ trợ: "BIGBURGER", "1", "HAMBURGER", "MEATCOOKED", "2", "COOKEDMEAT"
    /// </summary>
    private ItemType ConvertFoodIDToItemType(string foodID)
    {
        if (string.IsNullOrEmpty(foodID))
        {
            Debug.LogError("❌ FoodID is null/empty!");
            return ItemType.NONE;
        }

        // Chuẩn hóa food ID (bỏ dấu cách, chuyển sang uppercase)
        string normalizedID = foodID.ToUpper().Replace(" ", "");
        
        // Mapping từ Food ID sang ItemType (hỗ trợ cả ID string và numeric)
        switch (normalizedID)
        {
            // Big Burger / Hamburger
            case "BIGBURGER":
            case "BURGERFOOD":
            case "BIGBURGERFOOD":
            case "HAMBURGER":
            case "1":  // 🔥 Thêm hỗ trợ numeric ID
                return ItemType.HAMBURGER;
                
            // Meat Cooked / Cooked Meat
            case "MEATCOOKED":
            case "COOKEDMEAT":
            case "MEETCOOKED":
            case "MEETCOOKEDFOOD":
            case "2":  // 🔥 Thêm hỗ trợ numeric ID
                return ItemType.COOKEDMEAT;
                
            // ✅ Thêm các mapping khác nếu cần
            // case "3":
            //     return ItemType.PIZZA;
            
            default:
                Debug.LogWarning($"⚠️ Không có mapping cho Food ID: '{foodID}' (normalized: '{normalizedID}')");
                return ItemType.NONE;
        }
    }

    public void ConfirmOrder()
    {
        if (currentState == CustomerState.WaitingForFood && !hasOrdered)
        {
            hasOrdered = true;
            hasBeenServedByStaff = true; // Đánh dấu rằng staff đã tới
            Debug.Log("Player đã chốt đơn món: " + currentOrder);
        }
    }

    public bool ReceiveFood(ItemType foodBroughtByPlayer)
    {
        if (currentState != CustomerState.WaitingForFood || !hasOrdered) return false;

        if (foodBroughtByPlayer != currentOrder)
        {
            Debug.Log("Sai món rồi! Tôi gọi " + currentOrder + " cơ!");
            return false;
        }

        Debug.Log("Khách đã nhận đúng món " + currentOrder + "!");

        // ✅ Hiển thị eating bubble khi customer nhận đúng đồ ăn
        if (customerUI != null) customerUI.ShowEatingBubble();

        if (burgerModel != null) burgerModel.SetActive(true);

        currentState = CustomerState.Eating;
        StartCoroutine(EatAndPayRoutine());

        return true;
    }

    private IEnumerator EatAndPayRoutine()
    {
        yield return new WaitForSeconds(eatTime);

        if (animator != null)
        {
            animator.SetBool("Isate", true);
            animator.SetBool("IsSitting", false);
        }

        if (DataManager.Instance != null)
        {
            DataManager.Instance.totalMoney += foodPrice;
            DataManager.Instance.SaveData();
        }

        if (burgerModel != null) burgerModel.SetActive(false);
        if (mySeat != null) mySeat.tag = "Seat";

        // Thông báo cho QueueManager rằng khách đã hoàn thành
        if (queueManager != null && isInQueue)
        {
            queueManager.CustomerFinished(gameObject);
            isInQueue = false;
        }

        LeaveRestaurant();
    }

    public void LeaveRestaurant()
    {
        currentState = CustomerState.Leaving;

        // ✅ Ẩn eating bubble khi customer rời khỏi
        if (customerUI != null) customerUI.HideEatingBubble();

        // Dừng coroutine kiểm tra ghế nếu đang chạy
        if (seatCheckingCoroutine != null)
        {
            StopCoroutine(seatCheckingCoroutine);
            seatCheckingCoroutine = null;
        }

        // ✅ QUAN TRỌNG: Reset ghế tag từ "Untagged" → "Seat" để customer khác có thể ngồi
        if (mySeat != null)
        {
            mySeat.tag = "Seat";
            Debug.Log($"✅ Customer {gameObject.name} rời khỏi ghế. Reset tag ghế về 'Seat'");
        }

        // Thông báo QueueManager nếu đang trong hàng
        if (queueManager != null && isInQueue)
        {
            queueManager.CustomerFinished(gameObject);
            isInQueue = false;
        }

        if (capsuleCollider != null) capsuleCollider.isTrigger = false;
        if (agent != null) agent.enabled = true;

        if (agent != null && exitPoint != null)
        {
            if (agent.isActiveAndEnabled)
            {
                agent.isStopped = false;
                agent.updateRotation = true;
                agent.SetDestination(exitPoint.position);
            }

            if (animator != null)
            {
                animator.SetBool("IsWalking", true);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Redirect khách từ exit point sang waiting point nếu waiting point còn trống
    /// Được gọi bởi QueueManager khi điều kiện phù hợp
    /// </summary>
    public void RedirectToWaitingPoint(Transform newWaitingPoint)
    {
        if (currentState != CustomerState.Leaving || agent == null)
            return;

        if (newWaitingPoint == null)
        {
            Debug.LogWarning("Khách " + gameObject.name + " không thể redirect, waitingPoint null!");
            return;
        }

        // Cập nhật waitingPoint
        waitingPoint = newWaitingPoint;

        // Chuyển sang trạng thái WalkingToWaitingPoint
        currentState = CustomerState.WalkingToWaitingPoint;

        // Bật animation walk
        if (animator != null)
        {
            animator.SetBool("IsWalking", true);
        }

        // Set destination tới waiting point
        agent.isStopped = false;
        agent.SetDestination(waitingPoint.position);

        Debug.Log("Khách " + gameObject.name + " đã được redirect từ exit sang waiting point");
    }

    private GameObject FindClosestSeat()
    {
        GameObject[] seats = GameObject.FindGameObjectsWithTag("Seat");
        GameObject closest = null;
        float distance = Mathf.Infinity;
        Vector3 position = transform.position;

        foreach (GameObject seat in seats)
        {
            float curDistance = (seat.transform.position - position).sqrMagnitude;
            if (curDistance < distance)
            {
                closest = seat;
                distance = curDistance;
            }
        }
        return closest;
    }
}