using System.Collections.Generic;
using UnityEngine;

public class QueueManager : MonoBehaviour
{
    public static QueueManager Instance { get; private set; }

    [Header("Cấu hình hàng chờ")]
    public List<Vector3> ticketQueuePos = new List<Vector3>();
    public List<GameObject> npcInQueue = new List<GameObject>();

    [Header("Tham chiếu")]
    [SerializeField] private Transform queueStartPoint;
    [SerializeField] private Transform waitingPoint;
    private float redirectCheckTimer = 0f;
    private float redirectCheckInterval = 0.5f; // Kiểm tra mỗi 0.5 giây
    private float redirectDistance = 5f; // Khoảng cách tối đa để xét redirect

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    void Start()
    {
        // Nếu không có queueStartPoint, tìm nó
        if (queueStartPoint == null)
        {
            GameObject queueObj = GameObject.Find("QueueStartPoint");
            if (queueObj != null)
                queueStartPoint = queueObj.transform;
        }

        // Nếu không có waitingPoint, tìm nó
        if (waitingPoint == null)
        {
            GameObject waitingPointObj = GameObject.Find("WaitingPoint");
            if (waitingPointObj != null)
                waitingPoint = waitingPointObj.transform;
        }

        // Khởi tạo các vị trí hàng chờ
        InitializeQueuePositions();
    }

    void Update()
    {
        // Kiểm tra định kỳ xem có khách chưa được phục vụ đang tìm exit point gần waiting point không
        redirectCheckTimer += Time.deltaTime;
        if (redirectCheckTimer >= redirectCheckInterval)
        {
            CheckAndRedirectLeavingCustomers();
            redirectCheckTimer = 0f;
        }
    }

    /// <summary>
    /// Khởi tạo danh sách vị trí chờ
    /// </summary>
    void InitializeQueuePositions()
    {
        if (queueStartPoint != null)
        {
            Vector3 startPos = queueStartPoint.position;
            // Mỗi vị trí cách nhau 3 đơn vị trên trục X (âm)
            for (int i = 0; i < 5; i++)
            {
                ticketQueuePos.Add(startPos + Vector3.left * (i * 3f));
            }
        }
        else
        {
            // Vị trí mặc định nếu không tìm thấy QueueStartPoint
            ticketQueuePos.Add(new Vector3(8, 0, 0));
            ticketQueuePos.Add(new Vector3(5, 0, 0));
            ticketQueuePos.Add(new Vector3(2, 0, 0));
            ticketQueuePos.Add(new Vector3(-1, 0, 0));
            ticketQueuePos.Add(new Vector3(-4, 0, 0));
        }

        Debug.Log("Queue Manager được khởi tạo với " + ticketQueuePos.Count + " vị trí chờ");
    }

    /// <summary>
    /// Thêm khách vào hàng chờ
    /// </summary>
    public void AddToQueue(GameObject customer)
    {
        if (customer != null)
        {
            // Kiểm tra xem hàng chờ có đầy không
            if (npcInQueue.Count >= ticketQueuePos.Count)
            {
                Debug.LogWarning("Hàng chờ đã đầy! Khách sẽ rời đi");
                
                // Thông báo cho khách rằng hàng đầy, hãy rời đi
                CustomerAI customerAI = customer.GetComponent<CustomerAI>();
                if (customerAI != null)
                {
                    customerAI.QueueIsFull();
                }
                return;
            }

            npcInQueue.Add(customer);
            
            // Nếu là khách đầu tiên, đánh dấu
            if (npcInQueue.Count == 1)
            {
                CustomerAI customerAI = customer.GetComponent<CustomerAI>();
                if (customerAI != null)
                {
                    customerAI.IsFirstInQueue = true;
                    Debug.Log("Khách đầu tiên vào hàng, sẽ bắt đầu tìm ghế sau chút");
                }
            }
            
            UpdateQueuePositions();
            Debug.Log("Khách được thêm vào hàng. Tổng trong hàng: " + npcInQueue.Count);
        }
    }

    /// <summary>
    /// Cập nhật vị trí tất cả khách trong hàng
    /// </summary>
    public void UpdateQueuePositions()
    {
        for (int i = 0; i < npcInQueue.Count; i++)
        {
            if (npcInQueue[i] != null && i < ticketQueuePos.Count)
            {
                // Di chuyển khách tới vị trí hàng tương ứng
                CustomerAI customerAI = npcInQueue[i].GetComponent<CustomerAI>();
                if (customerAI != null)
                {
                    // Nếu là người đứng đầu, đánh dấu và cập nhật vị trí
                    if (i == 0)
                    {
                        customerAI.IsFirstInQueue = true;
                        // KHÔNG reset flags ở đây, để cho Update loop của customer xử lý
                        customerAI.GoToQueuePosition(ticketQueuePos[i]);
                        Debug.Log($"Khách {customerAI.gameObject.name} trở thành người đứng đầu hàng (vị trí 0)");
                    }
                    else
                    {
                        // Khách không phải người đứng đầu
                        customerAI.IsFirstInQueue = false;
                        // Reset flag tìm kiếm ghế cho những khách không phải người đứng đầu
                        customerAI.ResetFindingSeatFlag();
                        customerAI.GoToQueuePosition(ticketQueuePos[i]);
                        Debug.Log($"Khách {customerAI.gameObject.name} cập nhật vị trí hàng chờ (vị trí {i})");
                    }
                }
            }
        }
    }

    /// <summary>
    /// Gọi khi khách ngồi vào ghế (rời khỏi hàng chờ)
    /// Xóa khách khỏi queue và dịch các khách còn lại lên
    /// </summary>
    public void CustomerSeated(GameObject customer)
    {
        if (npcInQueue.Count > 0 && npcInQueue[0] == customer)
        {
            npcInQueue.RemoveAt(0);
            Debug.Log($"✅ Khách {customer.name} đã ngồi vào ghế. Xóa khỏi hàng. Còn lại: {npcInQueue.Count}");

            // Cập nhật vị trí những khách còn lại (element 1 → element 0, v.v.)
            UpdateQueuePositions();

            // Thông báo khách mới đứng đầu (nếu có)
            if (npcInQueue.Count > 0)
            {
                CustomerAI newFirstCustomer = npcInQueue[0].GetComponent<CustomerAI>();
                if (newFirstCustomer != null)
                {
                    newFirstCustomer.IsFirstInQueue = true;
                    Debug.Log($"👤 Khách mới đứng đầu hàng: {newFirstCustomer.gameObject.name}");
                }
            }
        }
        else
        {
            Debug.LogWarning($"⚠️ Khách {customer.name} không phải người đứng đầu hàng!");
        }
    }

    /// <summary>
    /// Gọi khi khách hoàn thành dịch vụ (ăn xong, rời đi)
    /// </summary>
    public void CustomerFinished(GameObject customer)
    {
        if (npcInQueue.Count > 0 && npcInQueue[0] == customer)
        {
            npcInQueue.RemoveAt(0);
            Debug.Log("Khách đã hoàn thành. Còn lại trong hàng: " + npcInQueue.Count);

            // Cập nhật vị trí những khách còn lại
            UpdateQueuePositions();

            // Thông báo khách mới đứng đầu bắt đầu tìm ghế
            if (npcInQueue.Count > 0)
            {
                CustomerAI newFirstCustomer = npcInQueue[0].GetComponent<CustomerAI>();
                if (newFirstCustomer != null)
                {
                    Debug.Log("Khách mới đứng đầu hàng: " + newFirstCustomer.gameObject.name);
                }
            }
        }
        else
        {
            // Nếu không phải người đứng đầu, chỉ xóa khỏi danh sách
            if (npcInQueue.Contains(customer))
            {
                npcInQueue.Remove(customer);
                Debug.Log("Khách rời khỏi hàng. Còn lại: " + npcInQueue.Count);
                UpdateQueuePositions();
            }
        }
    }

    /// <summary>
    /// Lấy vị trí tiếp theo của hàng
    /// </summary>
    public Vector3 GetNextQueuePosition()
    {
        if (ticketQueuePos.Count > 0)
            return ticketQueuePos[0];
        return Vector3.zero;
    }

    /// <summary>
    /// Kiểm tra xem hàng có đầy không
    /// </summary>
    public bool IsQueueFull()
    {
        return npcInQueue.Count >= ticketQueuePos.Count;
    }

    /// <summary>
    /// Lấy số lượng khách trong hàng
    /// </summary>
    public int GetQueueCount()
    {
        return npcInQueue.Count;
    }

    /// <summary>
    /// Kiểm tra xem waiting point có trống không (không có khách đi tới hoặc đang ở)
    /// </summary>
    private bool IsWaitingPointEmpty()
    {
        if (waitingPoint == null)
            return false;

        // Kiểm tra xem có khách nào đang ở hoặc đi tới waiting point không
        CustomerAI[] allCustomers = FindObjectsByType<CustomerAI>(FindObjectsSortMode.None);
        
        foreach (CustomerAI customer in allCustomers)
        {
            // Kiểm tra nếu khách ở trạng thái WaitingAtWaitingPoint hoặc WalkingToWaitingPoint
            if (customer.currentState == CustomerAI.CustomerState.WaitingAtWaitingPoint || 
                customer.currentState == CustomerAI.CustomerState.WalkingToWaitingPoint)
            {
                return false; // Waiting point có khách
            }
        }

        return true; // Waiting point trống
    }

    /// <summary>
    /// Tìm khách đang tìm exit point gần waiting point nhất và chưa được phục vụ
    /// </summary>
    private CustomerAI FindClosestLeavingCustomerNotServed()
    {
        if (waitingPoint == null)
            return null;

        CustomerAI[] allCustomers = FindObjectsByType<CustomerAI>(FindObjectsSortMode.None);
        CustomerAI closestCustomer = null;
        float closestDistance = redirectDistance; // Chỉ xét trong khoảng cách này

        foreach (CustomerAI customer in allCustomers)
        {
            // Điều kiện: đang tìm exit point (Leaving state) và CHƯA được phục vụ
            if (customer.currentState == CustomerAI.CustomerState.Leaving && 
                !customer.hasBeenServedByStaff)
            {
                float distance = Vector3.Distance(customer.transform.position, waitingPoint.position);
                
                // Tìm khách gần nhất
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestCustomer = customer;
                }
            }
        }

        return closestCustomer;
    }

    /// <summary>
    /// Kiểm tra và redirect khách từ exit point sang waiting point nếu điều kiện phù hợp
    /// </summary>
    private void CheckAndRedirectLeavingCustomers()
    {
        // Chỉ thực hiện nếu có waiting point
        if (waitingPoint == null)
            return;

        // Kiểm tra xem waiting point có trống không
        if (!IsWaitingPointEmpty())
            return;

        // Tìm khách đang tìm exit point gần waiting point nhất chưa được phục vụ
        CustomerAI targetCustomer = FindClosestLeavingCustomerNotServed();

        if (targetCustomer != null)
        {
            Debug.Log($"Tìm thấy khách {targetCustomer.gameObject.name} đang tìm exit gần waiting point, redirect sang waiting point");
            RedirectCustomerToWaitingPoint(targetCustomer);
        }
    }

    /// <summary>
    /// Redirect khách từ exit point sang waiting point
    /// </summary>
    private void RedirectCustomerToWaitingPoint(CustomerAI customer)
    {
        if (customer == null || waitingPoint == null)
            return;

        // Chuyển khách sang trạng thái WalkingToWaitingPoint
        customer.RedirectToWaitingPoint(waitingPoint);
        Debug.Log($"Khách {customer.gameObject.name} đã được redirect từ exit sang waiting point");
    }
}
