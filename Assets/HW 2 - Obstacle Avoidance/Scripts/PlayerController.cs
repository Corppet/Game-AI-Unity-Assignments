using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
	public float speed = 5f;

	[HideInInspector] public float currentSpeed;

    protected Rigidbody myRigidbody;
	protected Camera viewCamera;
	protected Vector3 velocity;

	void Awake() 
	{
		myRigidbody = GetComponent<Rigidbody>();
		viewCamera = Camera.main;
	}

	void Update() 
	{
		// mouse controls (controls direction of player)
		Vector3 mousePos = viewCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, 
			Input.mousePosition.y, viewCamera.transform.position.y));
		transform.LookAt(mousePos + Vector3.up * transform.position.y);
		
        // keyboard controls (controls movement of player)
        velocity = new Vector3(Input.GetAxisRaw("Horizontal"), 0, 
			Input.GetAxisRaw("Vertical")).normalized * speed;

        // update current speed
        currentSpeed = velocity.magnitude;
    }

    void FixedUpdate() 
	{
		// move character
		myRigidbody.MovePosition(myRigidbody.position + velocity * Time.fixedDeltaTime);
	}
}