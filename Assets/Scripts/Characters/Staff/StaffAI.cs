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
    private float checkTimer = 0f;
    private ItemType currentFoodType; // Loại đồ ăn đang mang

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

        // Tìm tự động Staff_RestPoint nếu chưa setup
        if (staffRestPoint == null)
        {
            GameObject restPointObj = GameObject.Find("Staff_RestPoint");
            if (restPointObj != null)
            {
                staffRestPoint = restPointObj.transform;
                Debug.Log("Staff tìm thấy Staff_RestPoint");
            }
            else
            {
                Debug.LogWarning("Staff không tìm thấy Staff_RestPoint!");
            }
        }

        if (agent == null)
            Debug.LogError("Staff " + gameObject.name + " không có NavMeshAgent!");
        if (animator == null)
            Debug.LogError("Staff " + gameObject.name + " không có Animator!");
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
                        if (animator != null) animator.SetBool("IsWalking", false);

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
                        if (animator != null) animator.SetBool("IsWalking", false);

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
                        if (animator != null) animator.SetBool("IsWalking", false);

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
                        if (animator != null) animator.SetBool("IsWalking", false);

                        currentState = StaffState.Idle;
                        Debug.Log("Staff đã tới vị trí nghỉ, chuyển về Idle");
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
        // Nếu đang phục vụ khách hoặc đang đi tới phục vụ, bỏ qua
        if (currentState == StaffState.Serving || currentState == StaffState.Walking)
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
    /// Staff đi tới vị trí khách hàng
    /// </summary>
    private void WalkToCustomer()
    {
        if (agent == null || targetCustomer == null)
            return;

        currentState = StaffState.Walking;

        // Bật animation walk
        if (animator != null)
            animator.SetBool("IsWalking", true);

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

        // Bật animation walk
        if (animator != null)
            animator.SetBool("IsWalking", true);

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

        Debug.Log("Staff đã đến khách hàng, bắt đầu chốt đơn...");

        // Chốt đơn trong 3 giây
        StartCoroutine(ConfirmOrderAfterDelay(3f));
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

        // Chuyển customer sang bubble waiting
        targetCustomer.ConfirmOrder();
        Debug.Log("Staff đã chốt đơn xong, customer chuyển sang waiting. Idào kitchen lấy: " + currentFoodType);

        // Đi tới kitchen lấy đồ ăn
        WalkToKitchen();
    }

    /// <summary>
    /// Staff đi tới kitchen để lấy đồ ăn
    /// </summary>
    private void WalkToKitchen()
    {
        if (agent == null || kitchenDesk == null)
        {
            Debug.LogError("Staff không có Kitchen Desk!");
            return;
        }

        currentState = StaffState.GoingToKitchen;

        if (animator != null)
            animator.SetBool("IsWalking", true);

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
            return;

        Debug.Log("Staff đã đến kitchen, nhận hàng: " + currentFoodType);

        // Hiển thị model đồ ăn trên staff (bật/tắt)
        ShowFoodModel(currentFoodType);

        // Sau 1 giây, quay lại khách
        StartCoroutine(ReturnToCustomerAfterDelay(1f));
    }

    /// <summary>
    /// Quay lại khách sau delay
    /// </summary>
    private IEnumerator ReturnToCustomerAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (targetCustomer == null)
            yield break;

        currentState = StaffState.ReturningToCustomer;

        if (animator != null)
            animator.SetBool("IsWalking", true);

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

        Debug.Log("Staff đã phục vụ đồ ăn cho khách");

        // Gọi ReceiveFood trên customer
        bool success = targetCustomer.ReceiveFood(currentFoodType);

        if (success)
        {
            // Tắt model đồ ăn khỏi staff (không destroy)
            HideAllFoodModels();
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

        if (animator != null)
        {
            animator.SetBool("IsWalking", false);
        }

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
}
