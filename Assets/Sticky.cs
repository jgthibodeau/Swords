using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sticky : MonoBehaviour
{
    private Collider[] colliders;
    private Rigidbody rb;
    public bool stuck = false;

    public bool destroyRigidbody;

    void Start()
    {
        colliders = GetComponentsInChildren<Collider>();
        rb = GetComponent<Rigidbody>();
    }

    void OnCollisionEnter(Collision c)
    {
        if (!stuck)
        {
            Quaternion rotation = transform.rotation;

            GameObject empty = new GameObject();
            empty.transform.parent = c.transform;
            transform.parent = empty.transform;
            //rb.constraints = RigidbodyConstraints.FreezeAll;
            //rb.isKinematic = true;

            //if (destroyRigidbody)
            //{
                Destroy(rb);
            //} else
            //{
            //    rb.isKinematic = true;
            //    foreach (Collider collider in colliders)
            //    {
            //        Physics.IgnoreCollision(c.collider, collider);
            //    }
            //}
            
            transform.position = Util.Average(c.contacts);
            transform.rotation = rotation;

            stuck = true;
        }
    }
}
