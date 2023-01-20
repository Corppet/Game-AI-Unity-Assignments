using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinishPlatform : MonoBehaviour
{
    [SerializeField] private GameObject finishText;

    private void Start()
    {
        finishText.SetActive(false);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            finishText.SetActive(true);
            collision.collider.GetComponent<PlayerBallController>().DisableMovement();
        }
    }
}
