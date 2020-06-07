using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectLauncher : MonoBehaviour
{
    public GameObject launchedObjectPrefab;

    public List<GameObject> instancedObjects = new List<GameObject>();
    public int maxInstancedObjects;
    public float launchTime;
    private float currentLaunchTime;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
