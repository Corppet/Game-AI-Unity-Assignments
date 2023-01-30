using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flock : MonoBehaviour
{
    private float speed;
    private bool turning = false;

    private FlockManager flockManager;
    
    private void Start()
    {
        speed = Random.Range(FlockManager.instance.minSpeed, FlockManager.instance.maxSpeed);
        flockManager = FlockManager.instance;
    }

    private void Update()
    {
        // check if the object is out of bounds
        Bounds bounds = new Bounds(flockManager.transform.position, flockManager.swimLimits * 2);
        if (!bounds.Contains(transform.position))
        {
            turning = true;
        }
        else
        {
            turning = false;
        }

        // if out of bounds, turn around
        if (turning)
        {
            Vector3 direction = flockManager.transform.position - transform.position;
            transform.rotation = Quaternion.Slerp(transform.rotation,
                Quaternion.LookRotation(direction),
                flockManager.rotationSpeed * Time.deltaTime);
            speed = Random.Range(flockManager.minSpeed, flockManager.maxSpeed);
        }
        // otherwise, behave normally (as a flock)
        else
        {
            if (Random.Range(0, 100) < 10)
            {
                speed = Random.Range(flockManager.minSpeed, flockManager.maxSpeed);
            }

            if (Random.Range(0, 100) < 10)
            {
                ApplyRules();
            }

            transform.Translate(0, 0, Time.deltaTime * speed);
        }
    }

    private void ApplyRules()
    {
        GameObject[] fishes = flockManager.fishes;

        Vector3 vCenter = Vector3.zero;
        Vector3 vAvoid = Vector3.zero;
        float gSpeed = 0.01f;
        float nDistance;
        int groupSize = 0;
        foreach (GameObject fish in fishes)
        {
            if (fish == gameObject)
                continue;
            
            nDistance = Vector3.Distance(fish.transform.position, transform.position);
            if (nDistance <= flockManager.neighbourDistance)
            {
                vCenter += fish.transform.position;
                groupSize++;

                // avoid other object if they are close
                if (nDistance < 1.0f)
                {
                    vAvoid += transform.position - fish.transform.position;
                }

                // "collect" speed from object
                Flock anotherFlock = fish.GetComponent<Flock>();
                gSpeed += anotherFlock.speed;
            }
        }

        if (groupSize > 0)
        {
            vCenter /= groupSize;
            vCenter += flockManager.goalPos - transform.position;

            // average the speed of the group
            speed = gSpeed / groupSize;
            if (speed > flockManager.maxSpeed)
            {
                speed = flockManager.maxSpeed;
            }

            // align the direction with other fish and towards the goal
            Vector3 direction = vCenter + vAvoid - transform.position;
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation,
                    Quaternion.LookRotation(direction),
                    flockManager.rotationSpeed * Time.deltaTime);
            }
        }
    }
}
