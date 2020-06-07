using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollideEffects : MonoBehaviour
{
    public float playWaitTime, minVelocity, maxVelocity;

    public GameObject objectToSpawn;
    public bool parentToObject;

    public bool playOnce;
    private bool played;

    void OnCollisionEnter(Collision collision)
    {
        if (playOnce && played)
        {
            return;
        }

        played = true;

        float velocity = collision.relativeVelocity.magnitude;
        if (velocity > minVelocity)
        {
            GameObject instance = GameObject.Instantiate(objectToSpawn);
            
            if (parentToObject)
            {
                GameObject obj = new GameObject();
                obj.transform.parent = transform;
                instance.transform.parent = obj.transform;
            }

            instance.transform.position = transform.position;
            instance.transform.rotation = transform.rotation;
        }
    }
}
