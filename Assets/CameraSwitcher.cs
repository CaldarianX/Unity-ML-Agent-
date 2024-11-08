using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    public Camera camera1;  // Drag your first camera here in the Inspector
    public Camera camera2;  // Drag your second camera here in the Inspector
    private Camera activeCamera;

    private void Start()
    {
        // Set the initial active camera
        activeCamera = camera1;
        UpdateCameraStates();
    }

    public void SwitchCamera()
    {
        // Toggle between camera1 and camera2
        activeCamera = activeCamera == camera1 ? camera2 : camera1;
        UpdateCameraStates();
    }

    private void UpdateCameraStates()
    {
        // Enable the active camera and disable the others
        camera1.gameObject.SetActive(activeCamera == camera1);
        camera2.gameObject.SetActive(activeCamera == camera2);
    }
}
