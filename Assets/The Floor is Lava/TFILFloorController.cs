using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TFILFloorController : MonoBehaviour
{
    [SerializeField] private GameObject respawnPoint;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.transform.position = respawnPoint.transform.position;
        }
    }
}
