using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
    public class CameraController : MonoBehaviour
    {
        [Range(0f, 100f)]
        [SerializeField] private float speed = 10f;

        private void Update()
        {
            // if the spacebar is pressed, go to the player
            if (Input.GetKey(KeyCode.Space))
            {
                Vector3 playerPos = GameManager.instance.player.transform.position;
                transform.position = new Vector3(playerPos.x, transform.position.y, playerPos.z - 20f);
            }
            else
            {
                float horizontal = Input.GetAxis("Horizontal");
                float vertical = Input.GetAxis("Vertical");

                Vector3 direction = new Vector3(horizontal, 0f, vertical);
                transform.position += direction * speed * Time.deltaTime;
            }
        }
    }
}
