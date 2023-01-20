using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControllerX : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float verticalInput;

    // Update is called once per frame
    void FixedUpdate()
    {
        // get the user's vertical input
        verticalInput = Input.GetAxis("Vertical");

        // move the plane forward at a constant rate
        transform.Translate(Vector3.forward * speed);

        // tilt the plane up/down based on up/down arrow keys
        if (Input.GetKey(KeyCode.UpArrow))
            transform.Rotate(Vector3.left * rotationSpeed * Time.deltaTime);
        if (Input.GetKey(KeyCode.DownArrow))
            transform.Rotate(Vector3.right * rotationSpeed * Time.deltaTime);
    }
}
