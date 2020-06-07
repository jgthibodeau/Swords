using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableAnchoredChild : MonoBehaviour
{
    //public BreakableAnchor breakableAnchor;

    //public bool broken = false;
    //public Rigidbody parentRb;
    //public float brokenMass;
    //public float brokenDrag;
    //public float brokenAngularDrag;

    //public AudioSource audioSource;
    
    //public BreakableAnchoredParent parent;

    //// Start is called before the first frame update
    //void Start()
    //{
    //    audioSource = GetComponent<AudioSource>();
    //    parentRb = transform.parent.GetComponent<Rigidbody>();
    //    parent = GetComponentInParent<BreakableAnchoredParent>();
    //}

    //public void TryToBreak(PlayAudio breakAudio)
    //{
    //    if (broken)
    //    {
    //        return;
    //    }

    //    Break(breakAudio);
    //}

    //private void Break(PlayAudio breakAudio)
    //{
    //    breakAudio.Play(audioSource);

    //    //for each sibling group

    //    foreach (BreakableSiblings group in siblingGroups)
    //    {
    //        //get all the non-null breakables
    //        group.siblingGroup.RemoveAll(item => item == null || !item.enabled);

    //        //if 1
    //        if (group.siblingGroup.Count == 1)
    //        {
    //            group.siblingGroup[0].Split();
    //        }
    //        else if (group.siblingGroup.Count > 1)
    //        {
    //            //create a new parent object with a copy of the parentRb
    //            GameObject newParent = new GameObject("new sibling group");
    //            newParent.transform.position = transform.position;
    //            newParent.transform.rotation = transform.rotation;

    //            //add complex breakable parent
    //            ComplexBreakableParent newBreakableParent = newParent.AddComponent<ComplexBreakableParent>();
    //            newBreakableParent.minBreakForce = complexBreakableParent.minBreakForce;
    //            newBreakableParent.breakAudio = complexBreakableParent.breakAudio;

    //            //add a new rigidbody based off the old parent
    //            Rigidbody newRb = newParent.AddComponent<Rigidbody>();
    //            newRb.mass = parentRb.mass;
    //            newRb.drag = parentRb.drag;
    //            newRb.angularDrag = parentRb.angularDrag;

    //            //put all the non-null breakables as children
    //            foreach (ComplexBreakableChild cbc in group.siblingGroup)
    //            {
    //                cbc.transform.parent = newParent.transform;
    //                cbc.parentRb = newRb;
    //                cbc.complexBreakableParent = newBreakableParent;
    //            }
    //        }
    //    }

    //    Transform oldParent = transform.parent;
    //    Split();
    //    if (oldParent != null)
    //    {
    //        GameObject.Destroy(oldParent.gameObject);
    //    }
    //}

    //public void Split()
    //{
    //    //remove this objects parent
    //    transform.parent = null;

    //    //create rigidbody
    //    Rigidbody rb = GetComponent<Rigidbody>();
    //    if (rb == null)
    //    {
    //        rb = gameObject.AddComponent<Rigidbody>();
    //        rb.mass = brokenMass;
    //        rb.drag = brokenDrag;
    //        rb.angularDrag = brokenAngularDrag;
    //        rb.velocity = parentRb.velocity;
    //        rb.angularVelocity = parentRb.angularVelocity;
    //    }

    //    //disable this script
    //    this.enabled = false;
    //    broken = true;
    //}
}
