using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerBallController : MonoBehaviour
{
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float maxVelocity = 10f;
    [SerializeField] private float jumpForce = 20f;

    private Rigidbody ballRigidbody;

    private bool moveEnabled;
    private bool isGrounded;

    public void DisableMovement()
    {
        moveEnabled = false;
        ballRigidbody.velocity = Vector3.zero;
        ballRigidbody.angularVelocity = Vector3.zero;
    }

    public void EnableMovement()
    {
        moveEnabled = true;
    }
    
    private void Start()
    {
        ballRigidbody = GetComponent<Rigidbody>();
        moveEnabled = true;
    }
    
    private void FixedUpdate()
    {
        if (moveEnabled)
        {
            // find the input vector
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            Vector3 movement = new Vector3(horizontal, 0, vertical);

            // move the ball according to the input vector
            float currentVelocity = ballRigidbody.velocity.magnitude;
            ballRigidbody.AddForce(movement * (maxVelocity - currentVelocity), ForceMode.Acceleration);

            // jump if the player presses the space bar
            if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
            {
                ballRigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                isGrounded = false;
            }
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
