using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClampVelocity : MonoBehaviour
{
    public bool continuallyUpdateChildren = false;
    public float maxVelocity = 20;
    public float maxAngularVelocity = 20;
    public bool cascadeToChildren = true;
    public float refreshRate = 0.1f;
    private Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (rb == null)
        {
            StartCoroutine(GetRigidBody());
        }

        if (cascadeToChildren)
        {
            CascadeToChildren();
            if (continuallyUpdateChildren)
            {
                StartCoroutine(UpdateChildren());
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (rb != null)
        {
            rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxVelocity);
            rb.angularVelocity = Vector3.ClampMagnitude(rb.angularVelocity, maxAngularVelocity);
        }
    }

    IEnumerator UpdateChildren()
    {
        while (true)
        {
            CascadeToChildren();
            yield return new WaitForSeconds(refreshRate);
        }
    }

    void CascadeToChildren() {
        Transform[] children = GetComponentsInChildren<Transform>();
        foreach (Transform child in children)
        {
            if (child != this && child.GetComponent<ClampVelocity>() == null)
            {
                ClampVelocity cv = child.gameObject.AddComponent<ClampVelocity>();
                cv.maxVelocity = maxVelocity;
                cv.cascadeToChildren = false;
            }
        }
    }

    IEnumerator GetRigidBody()
    {
        while (rb == null)
        {
            yield return new WaitForSeconds(refreshRate);
            rb = GetComponent<Rigidbody>();
        }
    }
}
