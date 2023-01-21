using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SAMCameraController : MonoBehaviour
{
    [SerializeField] private Camera[] cameras;
    [SerializeField] private TMP_Dropdown cameraDropdown;

    public void UpdateCamera()
    {
        int cameraIndex = cameraDropdown.value;
        for (int i = 0; i < cameras.Length; i++)
            cameras[i].enabled = i == cameraIndex;
    }
}
