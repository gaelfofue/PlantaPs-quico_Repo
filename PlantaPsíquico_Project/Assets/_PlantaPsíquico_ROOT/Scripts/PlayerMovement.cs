using UnityEngine;
using unityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public Rigidbody2D rb;
    public Transform groundCheck;
    public LayerMask groundLayer;

    float horizontal;
    float speed = 8f;
    float jumpingPower = 16f;
    bool isFacingRight = true;

    void Start()
    {

    }


    void Update()
    {
        rb.velocity = new Vector2(horizontalMovement * moveSpeed, rb.velocity.y);
    }

    public void Move(InputAction.CallbackContent context)
    {
        horizontalMovement = content.ReadValue<Vector2>().x;
    }
}
