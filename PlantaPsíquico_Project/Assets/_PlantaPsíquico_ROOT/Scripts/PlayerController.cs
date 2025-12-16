using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D playerRb;
    private float horizontalInput;

    public float speed;
    public float jumpForce;

    void Start()
    {
        playerRb = GetComponent<Rigidbody2D>();
    }


    void Update()
    {
        
    }

}
