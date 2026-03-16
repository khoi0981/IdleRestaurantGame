using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class CustomerAI : MonoBehaviour
{
    [Header("Cài đặt Đồ ăn & Trả tiền")]
    [SerializeField] private GameObject requestBubble;
    [SerializeField] private GameObject burgerModel;
    [SerializeField] private int foodPrice = 50;
    [SerializeField] private float eatTime = 10f;

    [Header("Kết nối Hệ thống")]
    public Transform exitPoint;

    private GameObject mySeat;
    private NavMeshAgent agent;
    private Animator animator;

    private enum CustomerState { WalkingToSeat, WaitingForFood, Eating, Leaving }
    private CustomerState currentState = CustomerState.WalkingToSeat;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        if (requestBubble != null) requestBubble.SetActive(false);
        if (burgerModel != null) burgerModel.SetActive(false);

        // Đảm bảo Isate là false ban đầu
        if (animator != null) animator.SetBool("Isate", false);

        if (exitPoint == null)
        {
            GameObject exitObj = GameObject.Find("ExitPoint");
            if (exitObj != null) exitPoint = exitObj.transform;
        }

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

        if (currentState == CustomerState.Leaving && agent != null)
        {
            if (!agent.pathPending && agent.remainingDistance <= 1.0f)
            {
                Destroy(gameObject);
            }
        }
    }

    private void SitDownAndOrder()
    {
        if (mySeat != null)
        {
            if (agent != null) agent.updateRotation = false;
            transform.position = mySeat.transform.position;

            Transform tableTransform = mySeat.transform.parent;
            if (tableTransform != null)
            {
                Vector3 directionToTable = tableTransform.position - transform.position;
                directionToTable.y = 0;
                transform.rotation = Quaternion.LookRotation(directionToTable);
            }

            transform.position += transform.forward * -0.5f;
        }

        if (animator != null)
        {
            animator.SetBool("IsWalking", false);
            animator.SetBool("IsSitting", true);
        }

        if (requestBubble != null) requestBubble.SetActive(true);
        Debug.Log("Khách đã ngồi xuống và đang Order món!");
    }

    public void ReceiveFood()
    {
        if (currentState != CustomerState.WaitingForFood) return;
        Debug.Log("Khách đã nhận được Burger!");

        if (requestBubble != null) requestBubble.SetActive(false);
        if (burgerModel != null) burgerModel.SetActive(true);

        currentState = CustomerState.Eating;
        StartCoroutine(EatAndPayRoutine());
    }

    private IEnumerator EatAndPayRoutine()
    {
        // Chờ thời gian ăn (10 giây)
        yield return new WaitForSeconds(eatTime);

        // --- SỬA Ở ĐÂY: Ăn xong thì chuyển Isate thành true ---
        if (animator != null)
        {
            animator.SetBool("Isate", true);
            animator.SetBool("IsSitting", false); // Tắt animation ngồi
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

        if (agent != null && exitPoint != null)
        {
            agent.isStopped = false;
            agent.updateRotation = true;
            agent.SetDestination(exitPoint.position);

            if (animator != null)
            {
                // Isate đã được bật ở trên, ở đây ta có thể bật lại IsWalking nếu cần cho các transition khác
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