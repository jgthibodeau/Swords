using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BreakableSiblings
{
    public List<ComplexBreakableChild> otherAnchors;
    public List<ComplexBreakableChild> siblingGroup;

    public Vector3 AveragePositon()
    {
        Vector3 position = Vector3.zero;
        int siblingCount = 0;
        foreach(ComplexBreakableChild cbc in siblingGroup)
        {
            if (cbc != null)
            {
                position += cbc.transform.position;
                siblingCount++;
            }
        }
        position /= siblingCount;
        return position;
    }
}

public class ComplexBreakableChild : MonoBehaviour
{
    public bool broken = false;
    public Rigidbody parentRb;
    public float brokenMass;
    public float brokenDrag;
    public float brokenAngularDrag;
    
    public AudioSource audioSource;
    public bool overrideParentAudio;
    public PlayAudio overrideAudio;

    public List<BreakableSiblings> siblingGroups;

    public ComplexBreakableParent complexBreakableParent;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        parentRb = transform.parent.GetComponent<Rigidbody>();
        complexBreakableParent = GetComponentInParent<ComplexBreakableParent>();
    }

    void OnTriggerEnter(Collider collider)
    {
        float breakForce = complexBreakableParent.CalculateBreakForce(collider);
        Debug.Log("Trigger " + breakForce);

        if (complexBreakableParent.CanBreak(collider.gameObject, breakForce))
        {
            ComplexBreakableChild cbc = collider.GetComponent<ComplexBreakableChild>();
            Debug.Log("Breaking");
            if (TryToBreak(complexBreakableParent.breakAudio, collider.transform.position))
            {
                GetComponent<Rigidbody>().velocity = collider.attachedRigidbody.velocity;
            }
        }
    }

    public bool TryToBreak(PlayAudio breakAudio, Vector3 position)
    {
        if (broken)
        {
            return false;
        }

        Break(breakAudio, position);
        return true;
    }

    private void Break(PlayAudio breakAudio, Vector3 position)
    {
        if (overrideParentAudio)
        {
            overrideAudio.Play(audioSource);
        }
        else
        {
            breakAudio.Play(audioSource);
        }

        //for each sibling group

        foreach (BreakableSiblings group in siblingGroups) {
            //get all the non-null breakables
            group.siblingGroup.RemoveAll(item => item == null || !item.enabled);

            //if 1
            if (group.siblingGroup.Count == 1)
            {
                group.siblingGroup[0].Split();
            }
            else if (group.siblingGroup.Count > 1) {
                //if there is at least 1 active anchor
                group.otherAnchors.RemoveAll(item => item == null || !item.enabled);
                if (group.otherAnchors.Count > 0)
                {
                    MoveSiblings(group);
                } else
                {
                    SplitSiblings(group);
                }
            }
        }

        Transform oldParent = transform.parent;
        Split();
        if (oldParent != null)
        {
            GameObject.Destroy(oldParent.gameObject);
        }
    }

    public void MoveSiblings(BreakableSiblings group)
    {
        ////create a new parent object with a copy of the parentRb
        //GameObject newParent = new GameObject("new sibling group");
        //newParent.transform.position = transform.position;
        //newParent.transform.rotation = transform.rotation;

        ////add complex breakable parent
        //ComplexBreakableParent newBreakableParent = newParent.AddComponent<ComplexBreakableParent>();
        //newBreakableParent.minBreakForce = complexBreakableParent.minBreakForce;
        //newBreakableParent.breakAudio = complexBreakableParent.breakAudio;

        ////add a new rigidbody based off the old parent
        //Rigidbody newRb = newParent.AddComponent<Rigidbody>();
        //newRb.mass = parentRb.mass;
        //newRb.drag = parentRb.drag;
        //newRb.angularDrag = parentRb.angularDrag;


        GameObject newParent = GameObject.Instantiate(complexBreakableParent.gameObject, new Vector3(0, -10000, 0), complexBreakableParent.transform.rotation);
        foreach (Transform child in newParent.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        newParent.transform.position = complexBreakableParent.transform.position;
        ComplexBreakableParent newBreakableParent = newParent.GetComponent<ComplexBreakableParent>();
        Rigidbody newRb = newParent.GetComponent<Rigidbody>();

        //put all the non-null breakables as children
        foreach (ComplexBreakableChild cbc in group.siblingGroup)
        {
            cbc.transform.parent = newParent.transform;
            cbc.parentRb = newRb;
            cbc.complexBreakableParent = newBreakableParent;
        }
    }

    public void SplitSiblings(BreakableSiblings group)
    {
        //put all the non-null breakables as children
        foreach (ComplexBreakableChild cbc in group.siblingGroup)
        {
            cbc.Split();
        }
    }

    public void Split()
    {
        //remove this objects parent
        transform.parent = null;
        
        //create rigidbody
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.velocity = parentRb.velocity;
            rb.angularVelocity = parentRb.angularVelocity;
        }
        rb.isKinematic = false;
        rb.useGravity = true;
        rb.mass = brokenMass;
        rb.drag = brokenDrag;
        rb.angularDrag = brokenAngularDrag;

        //disable this script
        this.enabled = false;
        broken = true;

        Instantiate(gameObject);
        Destroy(gameObject);
    }
}
