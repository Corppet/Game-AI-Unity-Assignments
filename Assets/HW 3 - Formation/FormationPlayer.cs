using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FormationPlayer : MonoBehaviour
{
    [HideInInspector] public static FormationPlayer instance { get; private set; }

    [HideInInspector] public List<Vector3> trail;

    [Header("Trail Settings")]
    [Range(0f, 10f)]
    [SerializeField] private float trailPointInterval = .5f;
    [Range(0, 100)]
    [SerializeField] private int maxTrailPoints = 10;

    /// <summary>
    /// Adds an invisible "breadcrumb" to the player's trail after a set delay.
    /// </summary>
    /// <param name="delay">
    /// The seconds to wait before leaving a breadcrumb.
    /// </param>
    /// <returns></returns>
    public IEnumerator AddTrailPoint(float delay)
    {
        yield return new WaitForSeconds(delay);

        // get the player's position without height
        Vector3 flatPos = transform.position;
        flatPos.y = 0f;

        // remove the oldest trail point from the trail
        trail.RemoveAt(0);
        trail.Add(flatPos);

        StartCoroutine(AddTrailPoint(trailPointInterval));
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        StartCoroutine(AddTrailPoint(trailPointInterval));
    }
}
