using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ChefAI : MonoBehaviour
{
    [Header("Chef References")]
    [SerializeField] private OvenStation ovenStation;
    [SerializeField] private float checkInterval = 1f; // Kiểm tra hàng chờ mỗi 1 giây
    [SerializeField] private float distanceThreshold = 0.5f; // Khoảng cách để xem đã tới oven

    private NavMeshAgent agent;
    private Animator animator;
    private Queue<ItemType> cookingQueue = new Queue<ItemType>();
    private float checkTimer = 0f;

    public enum ChefState { Idle, GoingToOven, Cooking, Done }
    public ChefState currentState = ChefState.Idle;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        // Tìm OvenStation
        if (ovenStation == null)
        {
            ovenStation = FindFirstObjectByType<OvenStation>();
            if (ovenStation == null)
            {
                Debug.LogError("❌ OvenStation not found! Chef won't work properly");
            }
        }

        // ✅ Set animation về Idle khi start
        SetAnimationWalking(false);
        SetAnimationCooking(false);

        Debug.Log("👨‍🍳 Chef initialized and ready to cook!");
    }

    private void Update()
    {
        // Kiểm tra hàng chờ mỗi 1 giây
        checkTimer += Time.deltaTime;
        if (checkTimer >= checkInterval)
        {
            CheckCookingQueue();
            checkTimer = 0f;
        }

        // Xử lý trạng thái đi tới oven
        if (currentState == ChefState.GoingToOven && agent != null && ovenStation != null)
        {
            if (agent.isActiveAndEnabled)
            {
                if (!agent.pathPending && agent.remainingDistance <= distanceThreshold)
                {
                    if (!agent.hasPath || agent.velocity.sqrMagnitude < 0.2f)
                    {
                        agent.isStopped = true;
                        SetAnimationWalking(false);
                        
                        currentState = ChefState.Cooking;
                        StartCooking();
                    }
                }
            }
        }

        // Xử lý trạng thái nấu ăn
        if (currentState == ChefState.Cooking && ovenStation != null)
        {
            // OvenStation xử lý cooking, chef chỉ cần đợi
            ItemType finishedFood = ovenStation.Process();
            
            if (finishedFood != ItemType.NONE)
            {
                Debug.Log($"✅ Chef finished cooking: {finishedFood}");
                
                // Kiểm tra xem còn món nào chờ nấu không
                if (ovenStation.GetCookingQueueCount() > 0)
                {
                    // Tiếp tục nấu món tiếp theo
                    ovenStation.Process(); // Sẽ tự động bắt đầu nấu tiếp
                }
                else
                {
                    // Xong hết, quay lại Idle
                    currentState = ChefState.Idle;
                    SetAnimationCooking(false);
                    Debug.Log("👨‍🍳 Chef finished all orders, back to Idle");
                }
            }
        }
    }

    /// <summary>
    /// Thêm đơn nấu ăn vào hàng chờ
    /// </summary>
    public void AddOrderToQueue(ItemType foodType)
    {
        if (foodType == ItemType.NONE)
        {
            Debug.LogWarning("❌ Cannot add NONE food to cooking queue");
            return;
        }

        cookingQueue.Enqueue(foodType);
        ovenStation.AddFoodToCookingQueue(foodType);
        Debug.Log($"📝 Chef received order: {foodType}. Queue size: {cookingQueue.Count}");

        // Nếu đang idle, bắt đầu đi tới oven
        if (currentState == ChefState.Idle)
        {
            GoToOven();
        }
    }

    /// <summary>
    /// Kiểm tra hàng chờ và bắt đầu nấu nếu cần
    /// </summary>
    private void CheckCookingQueue()
    {
        if (currentState == ChefState.Idle && ovenStation.GetTotalCookingTasks() > 0)
        {
            GoToOven();
        }
    }

    /// <summary>
    /// Chef đi tới oven
    /// </summary>
    private void GoToOven()
    {
        if (agent == null || ovenStation == null)
        {
            if (agent == null) Debug.LogError("❌ NavMeshAgent not found on Chef!");
            if (ovenStation == null) Debug.LogError("❌ OvenStation not found!");
            return;
        }

        currentState = ChefState.GoingToOven;
        
        // ✅ Chuyển animation: Walk
        SetAnimationWalking(true);
        SetAnimationCooking(false);

        // Lấy vị trí oven
        Transform ovenPos = ovenStation.GetOvenPosition();
        agent.isStopped = false;
        agent.SetDestination(ovenPos.position);

        Debug.Log("👨‍🍳 Chef is walking to oven...");
    }

    /// <summary>
    /// Chef bắt đầu nấu ăn
    /// </summary>
    private void StartCooking()
    {
        if (ovenStation == null)
            return;

        // ✅ Chuyển animation: Cooking
        SetAnimationWalking(false);
        SetAnimationCooking(true);

        agent.isStopped = true;
        
        // ✨ Hiển thị cooking bubble KHI Chef đã tới oven
        ovenStation.ShowCookingBubble();
        
        Debug.Log("👨‍🍳 Chef started cooking and bubble is now visible...");
    }

    private void SetAnimationWalking(bool isWalking)
    {
        if (animator != null)
        {
            animator.SetBool("IsWalking", isWalking);
            Debug.Log($"🎬 Chef Animation: IsWalking = {isWalking}");
        }
        else
        {
            Debug.LogWarning("⚠️ Animator not found on Chef!");
        }
    }

    /// <summary>
    /// Set animation Cooking
    /// </summary>
    private void SetAnimationCooking(bool isCooking)
    {
        if (animator != null)
        {
            animator.SetBool("IsCooking", isCooking);
            Debug.Log($"🎬 Chef Animation: IsCooking = {isCooking}");
        }
        else
        {
            Debug.LogWarning("⚠️ Animator not found on Chef!");
        }
    }

    /// <summary>
    /// Lấy trạng thái hiện tại
    /// </summary>
    public ChefState GetCurrentState()
    {
        return currentState;
    }

    /// <summary>
    /// Lấy số lượng đơn đang chờ nấu
    /// </summary>
    public int GetQueueCount()
    {
        return cookingQueue.Count;
    }
}
