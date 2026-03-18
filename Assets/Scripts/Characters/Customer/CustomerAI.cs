using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class CustomerAI : MonoBehaviour
{
    [Header("Menu & Order (Mới thêm)")]
    public System.Collections.Generic.List<ItemType> availableMenu;
    private ItemType currentOrder;
    private bool hasOrdered = false;

    [Header("Cài đặt Đồ ăn & Trả tiền")]
    [SerializeField] private GameObject burgerModel;
    [SerializeField] private int foodPrice = 50;
    [SerializeField] private float eatTime = 10f;

    [Header("Kết nối Hệ thống")]
    public Transform exitPoint;
    public ItemBox kitchenDesk;

    private GameObject mySeat;
    private NavMeshAgent agent;
    private Animator animator;
    private CapsuleCollider capsuleCollider;
    private CustomerUI customerUI;

    public enum CustomerState { WalkingToSeat, WaitingForFood, Eating, Leaving }
    public CustomerState currentState = CustomerState.WalkingToSeat;

    // Biến phụ để CustomerUI kiểm tra xem khách có đang đợi đồ không
    public bool IsWaitingForFood => currentState == CustomerState.WaitingForFood;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        customerUI = GetComponent<CustomerUI>();

        if (burgerModel != null) burgerModel.SetActive(false);
        if (animator != null) animator.SetBool("Isate", false);

        if (exitPoint == null)
        {
            GameObject exitObj = GameObject.Find("ExitPoint");
            if (exitObj != null) exitPoint = exitObj.transform;
        }

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

        mySeat = FindClosestSeat();

        if (mySeat != null && agent != null)
        {
            mySeat.tag = "Untagged";
            agent.SetDestination(mySeat.transform.position);

            if (animator != null) animator.SetBool("IsWalking", true);
        }
        else
        {
            LeaveRestaurant();
        }
    }

    void Update()
    {
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

        if (availableMenu != null && availableMenu.Count > 0)
        {
            int randomIndex = Random.Range(0, availableMenu.Count);
            currentOrder = availableMenu[randomIndex];
            hasOrdered = false;

            if (customerUI != null)
            {
                customerUI.ShowOrderBubble(currentOrder);
            }
            Debug.Log("Khách muốn gọi món: " + currentOrder);
        }
    }

    public void ConfirmOrder()
    {
        if (currentState == CustomerState.WaitingForFood && !hasOrdered)
        {
            hasOrdered = true;
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

        LeaveRestaurant();
    }

    public void LeaveRestaurant()
    {
        currentState = CustomerState.Leaving;

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