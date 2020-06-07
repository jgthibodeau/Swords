using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComplexSliceableChild : SliceableObject
{
    public int slicedCount = 0;

    [SerializeField]
    private List<ComplexSliceableAnchor> anchors = new List<ComplexSliceableAnchor>();
    public List<ComplexSliceableAnchor> GetAnchors()
    {
        anchors.RemoveAll(item => item == null);
        return anchors;
    }
    public void AddAnchor(ComplexSliceableAnchor anchor)
    {
        if (!anchors.Contains(anchor))
        {
            anchors.Add(anchor);
        }
    }
    public void RemoveAnchor(ComplexSliceableAnchor anchor)
    {
        anchors.Remove(anchor);
    }
    public void ClearAnchors()
    {
        anchors.Clear();
    }

    public ComplexSliceableParent parent;

    void Awake()
    {
        parent = GetComponentInParent<ComplexSliceableParent>();
    }
    
    public void SetupConnectedAnchors(ICollection<ComplexSliceableAnchor> newAnchors)
    {
        Collider collider = GetComponentInChildren<Collider>();
        anchors.Clear();
        if (collider != null)
        {
            foreach (ComplexSliceableAnchor anchor in newAnchors)
            {
                if (anchor.CloseEnoughToObject(collider))
                {
                    anchors.Add(anchor);
                }
                else
                {
                    anchor.RemoveConnectedChild(this);
                }
            }
        }

        foreach (ComplexSliceableAnchor anchor in anchors)
        {
            anchor.AddConnectedChild(this);
        }
    }

    public override SliceResult Slice(Collision collision, MeshCutter meshCutter, Vector3 point, Vector3 normal, float separation, float separationForce)
    {
        SliceResult sliceResult = base.Slice(collision, meshCutter, point, normal, 0, separationForce);

        if (sliceResult.sliced)
        {
            UpdateAnchors(sliceResult, separation, separationForce);
        }

        return sliceResult;
    }
    
    public void UpdateAnchors(SliceResult sliceResult, float separation, float separationForce)
    {
        List<ComplexSliceableAnchor> originalAnchors = new List<ComplexSliceableAnchor>();
        originalAnchors.AddRange(GetAnchors());

        ComplexSliceableChild side1 = sliceResult.biggerSlicedObject.GetComponentInParent<ComplexSliceableChild>();
        ComplexSliceableChild side2 = sliceResult.smallerSlicedObject.GetComponentInParent<ComplexSliceableChild>();

        List<ComplexSliceableAnchor> side1Anchors = new List<ComplexSliceableAnchor>();
        List<ComplexSliceableAnchor> side2Anchors = new List<ComplexSliceableAnchor>();
        
        foreach (ComplexSliceableAnchor anchor in originalAnchors)
        {
            //determine which sliced object is closer to the anchor and attach it
            float distanceFromAnchorTo1 = anchor.DistanceToCollider(sliceResult.biggerSlicedObject);
            float distanceFromAnchorTo2 = anchor.DistanceToCollider(sliceResult.smallerSlicedObject);

            if (distanceFromAnchorTo1 < distanceFromAnchorTo2)
            {
                side1Anchors.Add(anchor);
            }
            else
            {
                side2Anchors.Add(anchor);
            }

            anchor.RemoveConnectedChild(this);
        }

        side1.ClearAnchors();
        foreach (ComplexSliceableAnchor anchor in side1Anchors)
        {
            side1.AddAnchor(anchor);
            anchor.AddConnectedChild(side1);
        }
        side2.ClearAnchors();
        foreach (ComplexSliceableAnchor anchor in side2Anchors)
        {
            side2.AddAnchor(anchor);
            anchor.AddConnectedChild(side2);
        }

        //if either anchor list is empty, split it out into it's own object with a basic SliceableObject
        if (side1.anchors.Count == 0)
        {
            Split(side1, (side2.transform.position - side1.transform.position), separationForce);
        }
        if (side2.anchors.Count == 0)
        {
            Split(side2, (side1.transform.position - side2.transform.position), separationForce);
        }

        foreach (ComplexSliceableAnchor anchor in originalAnchors)
        {
            anchor.ResetConnectedAnchors();
        }
        parent.TraverseAnchorsAndSplitObject(separation, separationForce);
    }
    
    void Split(ComplexSliceableChild child, Vector3 normal, float separationForce)
    {
        child.transform.parent = parent.transform.parent;
        Rigidbody rb = child.gameObject.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = child.gameObject.AddComponent<Rigidbody>();
        }

        rb.mass = parent.childMass;
        rb.drag = parent.childDrag;
        rb.angularDrag = parent.childAngularDrag;
        rb.ResetCenterOfMass();
        rb.ResetInertiaTensor();

        rb.isKinematic = true;
        rb.isKinematic = false;
        
        SliceableObject sliceableObject = child.gameObject.AddComponent<SliceableObject>();
        sliceableObject.StartResetRigidbody(null, rb, true, normal, separationForce);
        Destroy(child);
    }
}
