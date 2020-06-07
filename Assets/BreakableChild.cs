using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableChild : MonoBehaviour
{
    private bool breakable = false;

    public Breakable breakableParent;

    void Start()
    {
        StartCoroutine(EnableBreakability());
    }

    IEnumerator EnableBreakability()
    {
        yield return null;
        yield return null;
        breakable = true;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (breakable)
        {
            //float force = collision.relativeVelocity.magnitude;
            //float force = collision.impulse.magnitude;
            float force = collision.relativeVelocity.magnitude;
            Rigidbody collisionRb = collision.gameObject.GetComponentInParent<Rigidbody>();
            if (collisionRb != null)
            {
                force *= collisionRb.mass;
            }

            breakableParent.Break(force, collision.gameObject);
        }
    }
}
