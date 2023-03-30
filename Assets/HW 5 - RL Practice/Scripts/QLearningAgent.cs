using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlaneRL
{
    public class QLearningAgent : MonoBehaviour
    {
        [Range(0f, 100f)]
        [SerializeField] float speed = 10f;

        [Space(10)]

        [SerializeField] private Transform target;

        new private Rigidbody rigidbody;
        private Vector3 startingPosition;
        private Vector3 startingRotation;

        private float totalReward;

        private State prevState;
        private int prevAction;
        private float extraReward;

        public void AddReward(float amount)
        {
            extraReward += amount;
        }

        private void Awake()
        {
            rigidbody = GetComponent<Rigidbody>();
        }

        private void Start()
        {
            startingPosition = transform.position;
            startingRotation = transform.rotation.eulerAngles;
            totalReward = 0;

            Debug.Log(transform.rotation.eulerAngles);
        }

        private void FixedUpdate()
        {
            // move the plane
            rigidbody.velocity = transform.forward * speed;

            // get agent's current state
            Vector2Int pos = new(Mathf.RoundToInt(transform.position.z), 
                Mathf.RoundToInt(transform.position.y));
            int rotation = Mathf.RoundToInt(transform.rotation.eulerAngles.x);
            State state = new(pos, rotation);

            // find and perform an action
            int action = QLearningManager.Instance.GetAction(state);
            PerformAction(action);

            // get reward
            float reward = extraReward;
            extraReward = 0;
            // terminal state
            if (reward < 0f)
            {
                NewEpisode();
            }
            else if (transform.rotation.eulerAngles.x > 90f || transform.rotation.eulerAngles.x < -90f)
            {
                reward = -10f;
                NewEpisode();
            }
            else if (transform.position.z > target.position.z)
            {
                reward += 100f;
                NewEpisode();
            }
            else
            {
                reward += .1f;
                totalReward += reward;
            }

            // update the Q-value and stats
            QLearningManager.Instance.UpdateQValue(prevState, prevAction, reward, state);
            QLearningManager.Instance.Steps++;
            prevState = state;
            prevAction = action;
        }

        private void NewEpisode()
        {
            QLearningManager.Instance.CompleteEpisode(totalReward);

            // reset position and rotation
            transform.SetPositionAndRotation(startingPosition, Quaternion.Euler(startingRotation));
            rigidbody.velocity = Vector3.zero;
            rigidbody.angularVelocity = Vector3.zero;

            totalReward = 0;
            Debug.Log("Starting new episode.");
        }

        private void PerformAction(int action)
        {
            transform.Rotate(Vector3.right * action);
        }
    }
}
