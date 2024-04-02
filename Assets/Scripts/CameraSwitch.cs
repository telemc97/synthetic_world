using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSwitch : MonoBehaviour
{
    public GameObject[] cameras;

    private int camIt = 0;

    private void Start()
    {
        foreach (GameObject camera in cameras)
        {
            camera.SetActive(false);
        }
        cameras[0].SetActive(true);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            int newCamera = camIt + 1;
            cameras[camIt].SetActive(false);
            if (newCamera >= cameras.Length) 
            {
                newCamera = 0;
            }
            camIt = newCamera;
            cameras[newCamera].SetActive(true);
        }
    }
}
