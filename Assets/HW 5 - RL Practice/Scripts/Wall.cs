using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlaneRL
{
    public class Wall : MonoBehaviour
    {
        public bool isQLearning = true;

        [Space(5)]

        public float terminalReward = -1f;
        public float goalReward = 10f;
        [Range(-50f, 0f)]
        public float minHeight = -40f;
        [Range(0f, 50f)]
        public float maxHeight = 40f;

        public void RandomizeHeight()
        {
            float height = Random.Range(minHeight, maxHeight);
            transform.position = new Vector3(transform.position.x, height, transform.position.z);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                if (isQLearning)
                {
                    collision.gameObject.GetComponent<QLearningAgent>().AddReward(terminalReward);
                }
                else
                {
                    collision.gameObject.GetComponent<PlaneAgent>().hasTouchedWall = true;
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                if (isQLearning)
                {
                    other.GetComponent<QLearningAgent>().AddReward(goalReward);
                }
                else
                {
                    other.GetComponent<PlaneAgent>().AddReward(goalReward);
                }
            }
        }
    }
}
