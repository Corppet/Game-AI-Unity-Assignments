using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerBallController : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private float jumpForce = 20f;
    
    private Rigidbody ballRigidbody;

    private bool isGrounded;
    
    private void Start()
    {
        ballRigidbody = GetComponent<Rigidbody>();
    }
    
    private void FixedUpdate()
    {
        // find the input vector
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 movement = new Vector3(horizontal, 0, vertical);

        // move the ball according to the input vector
        ballRigidbody.AddForce(movement * speed);

        // jump if the player presses the space bar
        if (Input.GetKey(KeyCode.Space) && isGrounded)
        {
            ballRigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.collider.CompareTag("Platform"))
        {
            isGrounded = true;
        }
    }
}
