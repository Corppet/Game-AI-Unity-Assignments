using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

namespace PlaneRL
{
    public class PlaneAgent : Agent
    {
        [HideInInspector] public bool hasTouchedWall;

        [Range(0f, 100f)]
        [SerializeField] float speed = 10f;
        [Range(0f, 720f)]
        [SerializeField] float rotationSpeed = 100f;

        [Space(10)]

        [SerializeField] private Transform target;

        new private Rigidbody rigidbody;

        private Vector3 startingPosition;
        private Vector3 startingRotation;

        private Wall goal;
        private int _current;
        private int currentWall
        {
            get { return _current; }

            set
            {
                Wall[] walls = GameManager.Instance.walls;

                if (value >= 0 && value < walls.Length)
                {
                    goal = walls[value];
                    _current = value;
                }
                else if (value < 0)
                {
                    _current = walls.Length - 1;
                    goal = walls[_current];
                }
                else
                {
                    _current = 0;
                    goal = walls[_current];
                }
            }
        }

        public override void Initialize()
        {
            rigidbody = GetComponent<Rigidbody>();
            startingPosition = transform.position;
            startingRotation = transform.eulerAngles;
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            sensor.AddObservation(transform.position);
            sensor.AddObservation(transform.rotation.eulerAngles);
            sensor.AddObservation(goal.transform.position);
        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            float verticalAction = Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f);

            if (transform.rotation.eulerAngles.x < 90f || transform.rotation.eulerAngles.x > -90f)
            {
                transform.Rotate(rotationSpeed * verticalAction * Vector3.right);
            }

            if (transform.position.z > goal.transform.position.z)
            {
                SetReward(10f);
                currentWall++;
            }
            if (hasTouchedWall)
            {
                SetReward(-1f);
                EndEpisode();
            }
            if (transform.position.z > target.position.z)
            {
                SetReward(100f);
                EndEpisode();
            }
            else
            {
                SetReward(-.1f);
            }

            rigidbody.velocity = transform.forward * speed;
        }

        public override void OnEpisodeBegin()
        {
            transform.SetPositionAndRotation(startingPosition, Quaternion.Euler(startingRotation));
            rigidbody.velocity = Vector3.zero;
            rigidbody.angularVelocity = Vector3.zero;
            currentWall = 0;
            hasTouchedWall = false;
        }

        public override void Heuristic(in ActionBuffers actionsOut)
        {
            var continuousActionsOut = actionsOut.ContinuousActions;
            continuousActionsOut[0] = Input.GetAxis("Vertical");
        }
    }
}
