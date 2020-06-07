using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class HingedBreakableChild : MonoBehaviour
{
    private bool broken = false;
    private bool breakable = false;
    private HingedBreakableObject hingedBreakableObject;
    private FixedJoint[] joints;
    private HingeJoint[] hinges;
    private AudioSource audioSource;

    // Start is called before the first frame update
    void Start()
    {
        hingedBreakableObject = GetComponentInParent<HingedBreakableObject>();
        audioSource = GetComponent<AudioSource>();
        joints = GetComponents<FixedJoint>();
        hinges = GetComponents<HingeJoint>();

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
        //float breakForce = CalculateBreakForce(collision);

        //if (joints != null && joints.Length > 0 && CanBreak(collision, breakForce))
        //{
        //    hingedBreakableObject.breakAudio.Play(audioSource);
        //    BreakJoints();
        //}

        SwordObject swordObject = collision.gameObject.GetComponentInParent<SwordObject>();
        bool swordSwing = swordObject != null && swordObject.sword.swinging;
        if (swordSwing)
        {
            BreakHinges();
        }
    }

    void OnJointBreak(float breakForce)
    {
        broken = true;
        hingedBreakableObject.breakAudio.Play(audioSource);
    }

    public void BreakJoints()
    {
        foreach (FixedJoint joint in joints)
        {
            joint.connectedBody = null;
            joint.breakForce = 0;
            hingedBreakableObject.RemoveChild(this);
        }
    }

    public void BreakHinges()
    {
        foreach (HingeJoint hinge in hinges)
        {
            hinge.connectedBody = null;
            hinge.breakForce = 0;
        }
    }

    private float CalculateBreakForce(Collision collision)
    {
        float breakForce = collision.relativeVelocity.magnitude;
        Rigidbody collisionRb = collision.gameObject.GetComponentInParent<Rigidbody>();
        if (collisionRb != null)
        {
            breakForce *= collisionRb.mass;
        }
        return breakForce;
    }

    private bool CanBreak(Collision collision, float breakForce)
    {
        if (!breakable)
        {
            return false;
        }

        SwordObject swordObject = collision.gameObject.GetComponentInParent<SwordObject>();
        bool swordSwing = swordObject != null && swordObject.sword.swinging;

        return swordSwing || (breakForce > hingedBreakableObject.minBreakForce);
    }
}
