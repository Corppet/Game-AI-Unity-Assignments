using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OAObstacle : MonoBehaviour
{
    [SerializeField] private float detectedDuration = 1f;
    [SerializeField] private Material detectedMaterial;
    
    private Material originalMaterial;
    private Renderer myRenderer;
    

    private void Awake()
    {
        myRenderer = GetComponent<Renderer>();
        originalMaterial = myRenderer.material;
    }

    public void Detected()
    {
        StartCoroutine(ToggleDetected());
    }

    private IEnumerator ToggleDetected()
    {
        myRenderer.material = detectedMaterial;

        yield return new WaitForSeconds(detectedDuration);

        myRenderer.material = originalMaterial;
    }
}
