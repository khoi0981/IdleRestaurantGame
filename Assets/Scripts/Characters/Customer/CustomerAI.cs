using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class CustomerAI : MonoBehaviour
{
    [Header("Menu & Order (Mới thêm)")]
    public System.Collections.Generic.List<ItemType> availableMenu;
    public ItemType currentOrder; // Public để Staff truy cập
    private bool hasOrdered = false;

    [Header("Cài đặt Đồ ăn & Trả tiền")]
    [SerializeField] private GameObject burgerModel;
    [SerializeField] private int foodPrice = 50;
    [SerializeField] private float eatTime = 10f;

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
    private float waitingDuration = 2f;
    private Vector3 targetQueuePosition;
    private bool isInQueue = false;
    public bool hasBeenServedByStaff = false; // Tracking xem staff đã tới nhận đơn chưa
    public bool IsFirstInQueue = false; // Tracking xem có phải người đứng đầu hàng không
    private bool hasReachedQueuePosition = false; // Tracking xem khách đã tới vị trí hàng chờ chưa
    private bool hasStartedFindingSeat = false; // Tracking xem đã bắt đầu tìm ghế chưa

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

                        // Đánh dấu rằng khách đã tới vị trí hàng chờ
                        hasReachedQueuePosition = true;
                        
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

        // --- XỬ LÝ TRẠNG THÁI: ĐỢI TẠI WAITING POINT ---
        if (currentState == CustomerState.WaitingAtWaitingPoint)
        {
            waitingAtPointTimer += Time.deltaTime;

            if (waitingAtPointTimer >= waitingDuration)
            {
                // Sau khi chờ xong, tìm ghế gần nhất
                mySeat = FindClosestSeat();

                if (mySeat != null && agent != null)
                {
                    mySeat.tag = "Untagged";
                    if (animator != null)
                    {
                        animator.SetBool("IsWaiting", false);
                        animator.SetBool("IsWalking", true);
                    }

                    agent.isStopped = false;
                    agent.SetDestination(mySeat.transform.position);
                    currentState = CustomerState.WalkingToSeat;
                    Debug.Log("Khách bắt đầu đi tới ghế...");
                }
                else
                {
                    Debug.LogWarning("Không tìm thấy ghế trống!");
                    LeaveRestaurant();
                }
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
                        if (animator != null) animator.SetBool("IsWalking", false);

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

            // Reset flags khi cập nhật vị trí hàng chờ (khách di chuyển sang vị trí mới)
            hasReachedQueuePosition = false;
            
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
        hasReachedQueuePosition = false;
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
    /// </summary>
    private void StartFindingSeat()
    {
        if (currentState != CustomerState.InQueue || !IsFirstInQueue) return;

        Debug.Log("Khách đứng đầu hàng bắt đầu tìm ghế...");

        // Nếu có waiting point, đi tới đó trước
        if (waitingPoint != null && agent != null)
        {
            if (animator != null)
            {
                animator.SetBool("IsWaiting", false);
                animator.SetBool("IsWalking", true);
            }

            agent.isStopped = false;
            agent.SetDestination(waitingPoint.position);
            currentState = CustomerState.WalkingToWaitingPoint;
            Debug.Log("Khách đầu tiên bắt đầu đi tới Waiting Point từ vị trí hàng chờ");
        }
        else
        {
            // Nếu không có waiting point, thẳng đi tới ghế
            mySeat = FindClosestSeat();
            if (mySeat != null && agent != null)
            {
                mySeat.tag = "Untagged";
                if (animator != null)
                {
                    animator.SetBool("IsWaiting", false);
                    animator.SetBool("IsWalking", true);
                }

                agent.isStopped = false;
                agent.SetDestination(mySeat.transform.position);
                currentState = CustomerState.WalkingToSeat;
                Debug.Log("Khách đầu tiên bắt đầu đi thẳng tới ghế");
            }
        }
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

        // Reset flag để staff biết cần phục vụ
        hasBeenServedByStaff = false;

        // --- LỰA CHỌN NGẪU NHIÊN MỘT MÓN ĂN TỪ MENU ---
        if (availableMenu != null && availableMenu.Count > 0)
        {
            int randomIndex = Random.Range(0, availableMenu.Count);
            currentOrder = availableMenu[randomIndex];
            hasOrdered = false;

            Debug.Log("Khách muốn gọi món: " + currentOrder);

            // --- HIỂN THỊ BUBBLE HIỂN THỊ MÓN ĂN ---
            if (customerUI != null)
            {
                customerUI.ShowOrderBubble(currentOrder);
                Debug.Log("Hiển thị bubble đặt hàng cho: " + currentOrder);
            }
            else
            {
                Debug.LogWarning("CustomerUI chưa được gán cho khách " + gameObject.name);
            }
        }
        else
        {
            Debug.LogWarning("Menu trống! Khách không có lựa chọn món ăn");
        }
    }

    public void ConfirmOrder()
    {
        if (currentState == CustomerState.WaitingForFood && !hasOrdered)
        {
            hasOrdered = true;
            hasBeenServedByStaff = true; // Đánh dấu rằng staff đã tới
            Debug.Log("Player đã chốt đơn món: " + currentOrder);

            if (customerUI != null) customerUI.ShowWaitingBubble();

            if (kitchenDesk != null)
            {
                kitchenDesk.SetType(currentOrder);
                Debug.Log("Bếp đã lên món: " + currentOrder);
            }
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

        if (customerUI != null) customerUI.HideAllBubbles();

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