using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{

    //Variables de Referencia
    private Rigidbody2D playerRb;
    private Animator anim;
    private PlayerInput input;
    private Vector2 moveInput;

    //Variables de Estadisticas del Player
    public float speed;
    public float jumpForce;
    public bool isGrounded; //Mira si esta tocando el suelo
    public Transform groundCheck; // Referencia a la posicion del detector de suelo
    public float groundCheckRadius; // Radio detector de suelo
    public LayerMask groundLayer; //Capa que puede tocar el detector de suelo

    private void Awake()
    {
         playerRb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        input = GetComponent<PlayerInput>();
    }

    void Start()
    {
       
    }

    
    void Update()
    {
       
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
        float horizontalMove = moveInput.x * speed;
        playerRb.linearVelocity = new Vector2(horizontalMove,playerRb.linearVelocity.y);
    }
    
    public void OnJump(InputAction.CallbackContext context)
    {


    }
}
