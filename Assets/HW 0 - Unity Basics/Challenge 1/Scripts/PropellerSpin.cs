using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropellerSpin : MonoBehaviour
{
    [SerializeField] private float rotationSpeed;

    // Update is called once per frame
    void Update()
    {
        // make the game object spin
        transform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);
    }
}
