using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraEffects : MonoBehaviour
{
    public GameObject normalCamera;
    public GameObject swordCamera;

    void Start()
    {
        normalCamera.SetActive(true);
        swordCamera.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButton("Ready") || Input.GetButton("Block"))
        {
            normalCamera.SetActive(false);
            swordCamera.SetActive(true);
        } else
        {
            normalCamera.SetActive(true);
            swordCamera.SetActive(false);
        }
    }
}
