using UnityEngine;

public class PlayerController : MonoBehaviour
{

    //Variables de Referencia
    private Rigidbody2D playerRb;
    private Animator anim;
    private float horizontalInput;

    //Variables de Estadisticas del Player
    public float speed;
    public float jumpForce;

    void Start()
    {
        playerRb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    
    void Update()
    {
        Movement();
        Jump();
    }

    void Movement ()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        playerRb.linearVelocity = new Vector2 (horizontalInput, 0);
    }

    void Jump ()
    {

    }
}
