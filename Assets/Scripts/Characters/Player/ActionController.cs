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
            // Vẫn giữ lại phần ĐẶT ĐỒ XUỐNG bằng chuột trái
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
            // Đã xóa lệnh nhận đồ (ItemBox) ở đây vì giờ ta dùng Trigger bên dưới
        }
    }

    // --- ĐÂY LÀ PHẦN THÊM MỚI ĐỂ TỰ ĐỘNG NHẬN ĐỒ ---
    private void OnTriggerEnter(Collider other)
    {
        // Kiểm tra xem vật vừa chạm có phải là bàn chứa đồ không
        if (other.TryGetComponent<ItemBox>(out ItemBox itemBox))
        {
            // Chỉ lấy đồ khi tay đang trống
            if (inventory.CurrentType == ItemType.NONE)
            {
                inventory.TakeItem(itemBox.GetItem());
                Debug.Log("Đã tự động lấy: " + itemBox.GetItem());
                StartCoroutine(canPutCoolDown());
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        // Nếu không cầm HAMBURGER thì thoát ra
        if (inventory.CurrentType != ItemType.HAMBURGER) return;

        // Nếu chạm vào Customer
        if (other.gameObject.CompareTag("Customer"))
        {
            CustomerAI currentCustomer = other.GetComponent<CustomerAI>();

            if (currentCustomer != null)
            {
                currentCustomer.ReceiveFood(); // Báo cho khách biết đã nhận đồ
                inventory.ClearHand();         // Ẩn Burger trên tay Player
                Debug.Log("Đã giao HAMBURGER cho khách!");
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