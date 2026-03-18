using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionController : MonoBehaviour
{
    private Animator anim;
    private Inventory inventory;
    private Functionality currentFunction;
    private WaitForSeconds takeCooldown;
    private bool isWorking = false;
    private bool isProcessing = false;
    private bool canPut = true;

    private void Awake()
    {
        canPut = true;
        anim = GetComponent<Animator>();
        inventory = GetComponent<Inventory>();
        takeCooldown = new WaitForSeconds(0.5f);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            DoAction();
        }
        else if (Input.GetMouseButton(0))
        {
            isWorking = true;
            if (isProcessing == false)
            {
                StartProcessAction();
            }
            else
            {
                DoProcessAction();
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isWorking = false;
            if (isProcessing)
            {
                currentFunction?.ResetTimer();
                isProcessing = false;
            }
        }
    }

    private void DoAction()
    {
        if (anim != null)
        {
            // Tạm thời comment lại nếu Animator của Player chưa làm animation Take
            // anim.SetTrigger("Take"); 
        }

        DoTakeAction();
    }

    private void StartProcessAction()
    {
        Ray ray = new Ray(transform.position + Vector3.up / 2, transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, 1.5f))
        {
            if (hit.collider.TryGetComponent<Functionality>(out Functionality itemProcess))
            {
                isProcessing = true;
                currentFunction = itemProcess;
            }
        }
    }

    private void DoProcessAction()
    {
        if (!isProcessing) return;
        if (!isWorking) return;
        ItemType item = currentFunction.Process();
        if (item != ItemType.NONE)
        {
            currentFunction.ClearObject();
            inventory.TakeItem(item);
            isWorking = false;
        }
    }

    public void DoTakeAction()
    {
        Vector3 rayStartPoint = transform.position + (Vector3.up * 1f) + (transform.forward * 0.5f);
        Ray ray = new Ray(rayStartPoint, transform.forward);
        Debug.DrawRay(rayStartPoint, transform.forward * 1.5f, Color.red, 2f);

        if (Physics.Raycast(ray, out RaycastHit hit, 1.5f))
        {
            if (hit.collider.TryGetComponent<IPutItemFull>(out IPutItemFull itemPutBox))
            {
                if (canPut)
                {
                    bool status = itemPutBox.PutItem(inventory.GetItem());
                    if (status == true)
                    {
                        inventory.PutItem();
                        inventory.ClearHand();
                    }
                }
            }
        }
    }

    // --- PHẦN LOGIC TƯƠNG TÁC TỰ ĐỘNG BẰNG TRIGGER ---
    private void OnTriggerEnter(Collider other)
    {
        // 1. Tương tác với Bàn chứa đồ (ItemBox)
        if (other.TryGetComponent<ItemBox>(out ItemBox itemBox))
        {
            // Nếu bàn đang có đồ và tay player đang trống rỗng
            if (inventory.CurrentType == ItemType.NONE && itemBox.GetCurrentType() != ItemType.NONE)
            {
                inventory.TakeItem(itemBox.GetItem()); // Lấy đồ vào inventory
                Debug.Log("Đã tự động lấy: " + inventory.CurrentType);
                StartCoroutine(canPutCoolDown());
            }
        }

        // 2. Tương tác với Khách Hàng
        // Giữ nguyên dùng Tag "Customer" như bạn viết, hoặc dùng TryGetComponent cho an toàn
        if (other.TryGetComponent<CustomerAI>(out CustomerAI customerAI))
        {
            // Trường hợp 1: Tay người chơi trống rỗng -> Lại gần để chốt đơn
            if (inventory.CurrentType == ItemType.NONE)
            {
                customerAI.ConfirmOrder();
            }
            // Trường hợp 2: Tay người chơi có đồ ăn -> Thử giao đồ cho khách
            else
            {
                // Truyền món ăn Player đang cầm (inventory.CurrentType) vào cho khách kiểm tra
                bool isDelivered = customerAI.ReceiveFood(inventory.CurrentType);

                // Nếu khách nhận (nghĩa là đúng món và khách đang chờ đồ)
                if (isDelivered)
                {
                    inventory.ClearHand(); // Xóa đồ trên tay Player
                    Debug.Log("Đã giao thành công món " + inventory.CurrentType + " cho khách!");
                }
            }
        }
    }

    private IEnumerator canPutCoolDown()
    {
        canPut = false;
        yield return takeCooldown;
        canPut = true;
    }
}