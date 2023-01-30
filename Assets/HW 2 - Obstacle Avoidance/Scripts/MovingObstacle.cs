using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingObstacle : MonoBehaviour
{
    [SerializeField] private Vector3 velocity;
    [SerializeField] private float duration;
    
    private Rigidbody myRigidbody;

    private IEnumerator ChangeDirection()
    {
        velocity *= -1;

        yield return new WaitForSeconds(duration);

        StartCoroutine(ChangeDirection());
    }

    private void Awake()
    {
        myRigidbody = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        StartCoroutine(ChangeDirection());
    }

    private void FixedUpdate()
    {
        myRigidbody.MovePosition(myRigidbody.position + velocity * Time.fixedDeltaTime);
    }
}
