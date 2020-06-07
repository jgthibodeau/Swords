using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[RequireComponent(typeof(AudioSource))]
public class ComplexBreakableParent : MonoBehaviour
{
    public bool breakable = false;
    public float minBreakForce;
    public PlayAudio breakAudio;
    //public AudioSource audioSource;

    // Start is called before the first frame update
    void Start()
    {
        //audioSource = GetComponent<AudioSource>();
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
        float breakForce = CalculateBreakForce(collision);

        if (CanBreak(collision.gameObject, breakForce))
        {
            foreach (ContactPoint cp in collision.contacts)
            {
                ComplexBreakableChild cbc = cp.thisCollider.GetComponent<ComplexBreakableChild>();
                BreakChild(cbc, collision.transform.position);
            }
        }
    }

    public float CalculateBreakForce(Collision collision)
    {
        float breakForce = collision.relativeVelocity.magnitude;
        Rigidbody collisionRb = collision.gameObject.GetComponentInParent<Rigidbody>();
        if (collisionRb != null)
        {
            breakForce *= collisionRb.mass;
        }
        return breakForce;
    }

    public float CalculateBreakForce(Collider collider)
    {
        Rigidbody collisionRb = collider.attachedRigidbody;
        return collisionRb.velocity.magnitude * collisionRb.mass;
    }

    public bool CanBreak(GameObject go, float breakForce)
    {
        if (!breakable)
        {
            return false;
        }

        SwordObject swordObject = go.GetComponentInParent<SwordObject>();
        bool swordSwing = swordObject != null && swordObject.sword.swinging;

        return swordSwing || (breakForce > minBreakForce);
    }

    private void BreakChild(ComplexBreakableChild cbc, Vector3 position)
    {
        if (cbc != null)
        {
            cbc.TryToBreak(breakAudio, position);
        }
    }
}
