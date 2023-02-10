using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OAObstacle : MonoBehaviour
{
    [SerializeField] private float detectedDuration = 1f;
    [SerializeField] private Material detectedMaterial;
    
    private Material originalMaterial;
    private Renderer myRenderer;
    private bool isDetected;
    

    private void OnEnable()
    {
        myRenderer = GetComponent<Renderer>();
        originalMaterial = myRenderer.material;
    }

    public void Detected()
    {
        if (isDetected)
            return;

        StartCoroutine(ToggleDetected());
    }

    private IEnumerator ToggleDetected()
    {
        isDetected = true;
        myRenderer.material = detectedMaterial;

        yield return new WaitForSeconds(detectedDuration);

        isDetected = false;
        myRenderer.material = originalMaterial;
    }
}
