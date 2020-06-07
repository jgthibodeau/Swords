using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComplexSliceableAnchor : MonoBehaviour
{
    public ComplexSliceableParent parent;

    void Start()
    {
        parent = GetComponentInParent<ComplexSliceableParent>();
    }

    [SerializeField]
    private List<ComplexSliceableAnchor> connectedAnchors = new List<ComplexSliceableAnchor>();
    public List<ComplexSliceableAnchor> GetConnectedAnchors()
    {
        connectedAnchors.RemoveAll(item => item == null);
        return connectedAnchors;
    }
    public void AddConnectedAnchor(ComplexSliceableAnchor anchor)
    {
        if (!connectedAnchors.Contains(anchor))
        {
            connectedAnchors.Add(anchor);
        }
    }
    public void RemoveConnectedAnchor(ComplexSliceableAnchor anchor)
    {
        connectedAnchors.Remove(anchor);
    }
    public void ClearConnectedAnchors()
    {
        connectedAnchors.Clear();
    }


    [SerializeField]
    private List<ComplexSliceableChild> connectedChildren = new List<ComplexSliceableChild>();
    public List<ComplexSliceableChild> GetConnectedChildren()
    {
        connectedChildren.RemoveAll(item => item == null);
        return connectedChildren;
    }
    public void AddConnectedChild(ComplexSliceableChild child)
    {
        if (!connectedChildren.Contains(child))
        {
            connectedChildren.Add(child);
        }
    }
    public void RemoveConnectedChild(ComplexSliceableChild child)
    {
        connectedChildren.Remove(child);
    }
    public void ClearConnectedChildren()
    {
        connectedChildren.Clear();
    }


    public float DistanceToCollider(Collider collider)
    {
        return collider.bounds.SqrDistance(transform.position);
    }

    public bool CloseEnoughToObject(Collider collider)
    {
        return DistanceToCollider(collider) < parent.minAnchorDistance * parent.minAnchorDistance;
    }

    public Vector3 GetAveragePosition()
    {
        int childCount = 0;
        Vector3 averagePosition = Vector3.zero;

        foreach (ComplexSliceableChild child in GetConnectedChildren())
        {
            childCount++;
            averagePosition += child.transform.position;
        }

        if (childCount > 0)
        {
            averagePosition /= childCount;
        }

        return averagePosition;
    }

    public void SetParent(Transform newParent)
    {
        transform.parent = newParent;

        foreach (ComplexSliceableChild child in GetConnectedChildren())
        {
            child.transform.parent = newParent;
        }
    }

    public void ResetConnectedAnchors()
    {
        ClearConnectedAnchors();

        foreach (ComplexSliceableChild child in GetConnectedChildren())
        {
            foreach(ComplexSliceableAnchor anchor in child.GetAnchors())
            {
                connectedAnchors.Add(anchor);
            }
        }
    }
}
