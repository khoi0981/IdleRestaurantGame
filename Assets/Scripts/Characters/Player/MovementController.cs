using UnityEngine;
using Terresquall;

public class TopDownPlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float rotationSpeed = 720f;

    private Rigidbody rb;
    private Vector3 movement;
    private Animator anim; // Bước 1: Khai báo biến Animator

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>(); // Lấy component Animator từ nhân vật
        rb.freezeRotation = true;
    }

    void Update()
    {
        float moveX = VirtualJoystick.GetAxis("Horizontal");
        float moveZ = VirtualJoystick.GetAxis("Vertical");

        movement = new Vector3(moveX, 0f, moveZ).normalized;

        // --- Bước 2: THAY ĐỔI GIÁ TRỊ ĐIỀU KIỆN ANIMATION ---
        // movement.magnitude sẽ trả về giá trị từ 0 (đứng yên) đến 1 (di chuyển tối đa)
        float currentSpeed = movement.magnitude;

        // Gửi giá trị này vào tham số "Speed" (kiểu Float) trong Animator của Dog
        // Hoặc dùng anim.SetBool("IsMoving", currentSpeed > 0); nếu bạn dùng kiểu Bool
        anim.SetFloat("Speed", currentSpeed);
        // ----------------------------------------------------

        if (movement != Vector3.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(movement, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
        }
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }
}   