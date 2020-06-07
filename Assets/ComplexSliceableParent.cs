using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComplexSliceableParent : SliceableObject
{
    public ComplexSliceableAnchor[] anchors;
    public ComplexSliceableChild[] children;
    public float childMass, childDrag, childAngularDrag, minAnchorDistance;

    public bool generateAnchors = true;

    void Start()
    {
        if (generateAnchors)
        {
            GenerateAnchors();
            generateAnchors = false;
        }
    }

    private void GenerateAnchors()
    {
        //get all anchors and children
        anchors = GetComponentsInChildren<ComplexSliceableAnchor>();
        children = GetComponentsInChildren<ComplexSliceableChild>();

        //clear out each anchor
        foreach (ComplexSliceableAnchor anchor in anchors)
        {
            anchor.ClearConnectedChildren();
            anchor.ClearConnectedAnchors();
        }

        //for each child, determine the anchors close enough to it
        foreach (ComplexSliceableChild child in children)
        {
            child.SetupConnectedAnchors(anchors);
        }

        //for each anchor, determine all connected anchors through children
        foreach (ComplexSliceableAnchor anchor in anchors)
        {
            anchor.ResetConnectedAnchors();
        }
    }

    public override SliceResult Slice(Collider collider, MeshCutter meshCutter, Vector3 point, Vector3 normal, float separation, float separationForce)
    {
        Debug.Log("slicing complex parent via collider");
        SliceResult sliceResult = new SliceResult();
        ComplexSliceableChild child = collider.gameObject.GetComponentInParent<ComplexSliceableChild>();
        if (child != null)
        {
            Debug.Log("slicing child " + child);
            SliceChild(sliceResult, child, collider, meshCutter, point, normal, separation, separationForce);
        }

        return sliceResult;
    }

    public override SliceResult Slice(Collision collision, MeshCutter meshCutter, Vector3 point, Vector3 normal, float separation, float separationForce)
    {
        Debug.Log("slicing complex parent via collision");
        SliceResult sliceResult = new SliceResult();

        ContactPoint[] contacts = new ContactPoint[collision.contactCount];
        collision.GetContacts(contacts);
        List<ComplexSliceableChild> slicedChildren = new List<ComplexSliceableChild>();
        foreach (ContactPoint cp in contacts)
        {
            ComplexSliceableChild child = cp.otherCollider.GetComponentInParent<ComplexSliceableChild>();
            if (child != null && !slicedChildren.Contains(child))
            {
                Debug.Log("slicing child " + child);
                SliceChild(sliceResult, child, collision, meshCutter, point, normal, separation, separationForce);
                slicedChildren.Add(child);
            }
        }

        return sliceResult;
    }

    private void SliceChild(SliceResult sliceResult, ComplexSliceableChild child, Collider collider, MeshCutter meshCutter, Vector3 point, Vector3 normal, float separation, float separationForce)
    {
        //perform the slice for each child
        SliceResult childResult = child.Slice(new Collision(), meshCutter, point, normal, separation, separationForce);
        if (childResult.sliced)
        {
            sliceResult.colliders.AddRange(childResult.colliders);
            sliceResult.sliced = true;
        }
    }

    private void SliceChild(SliceResult sliceResult, ComplexSliceableChild child, Collision collision, MeshCutter meshCutter, Vector3 point, Vector3 normal, float separation, float separationForce)
    {
        //perform the slice for each child
        SliceResult childResult = child.Slice(collision, meshCutter, point, normal, separation, separationForce);
        if (childResult.sliced)
        {
            sliceResult.colliders.AddRange(childResult.colliders);
            sliceResult.sliced = true;
        }
    }

    public void ResetAnchors(float separation, float separationForce)
    {
        //create a new list to store visited anchors
        List<ComplexSliceableAnchor> visitedAnchors = new List<ComplexSliceableAnchor>();
        List<List<ComplexSliceableAnchor>> visitedAnchorLists = new List<List<ComplexSliceableAnchor>>();

        //repeatedly traverse the anchor list to build up a list of graphs
        foreach (ComplexSliceableAnchor anchor in anchors)
        {
            if (!visitedAnchors.Contains(anchor))
            {
                visitedAnchorLists.Add(GenerateAnchorGraph(anchor, visitedAnchors));
            }
        }

        for(int i = transform.childCount - 1; i >= 0; i--)
        {
            transform.GetChild(i).parent = null;
        }

        Vector3 originalPosition = transform.position;

        SetAnchors(visitedAnchorLists[0].ToArray());

        List<ComplexSliceableParent> parentList = new List<ComplexSliceableParent>();
        parentList.Add(this);
        int smallestGroup = GetComponentsInChildren<ComplexSliceableChild>().Length;
        for (int i = 1; i < visitedAnchorLists.Count; i++) {
            GameObject newObj = Util.InstantianteParent(gameObject, transform.parent);

            ComplexSliceableParent parent = newObj.GetComponent<ComplexSliceableParent>();
            parent.SetAnchors(visitedAnchorLists[i].ToArray());

            int groupSize = parent.GetComponentsInChildren<ComplexSliceableChild>().Length;
            if (groupSize < smallestGroup)
            {
                smallestGroup = groupSize;
            }

            parentList.Add(parent);
        }

        foreach(ComplexSliceableParent parent in parentList)
        {
            Rigidbody rb = parent.gameObject.GetComponent<Rigidbody>();
            rb.ResetCenterOfMass();
            rb.ResetInertiaTensor();

            int groupSize = parent.GetComponentsInChildren<ComplexSliceableChild>().Length;

            Vector3 direction = (parent.transform.position - originalPosition);
            parent.transform.position += direction * separation;
            bool inSmallestGroup = (groupSize == smallestGroup);
            //if (inSmallestGroup)
            //{
            //    transform.rotation *= Quaternion.Euler(0, 0, Random.Range(-5f, 5f));
            //}
            //StartCoroutine(ResetRigidbody(rb, !inSmallestGroup, direction, separationForce));
            StartCoroutine(ResetRigidbody(null, rb, true, direction, separationForce));
        }
    }

    public void SetAnchors(ComplexSliceableAnchor[] newAnchors)
    {
        anchors = newAnchors;

        int anchorCount = 0;
        Vector3 averagePosition = Vector3.zero;
        foreach(ComplexSliceableAnchor anchor in anchors)
        {
            if (anchor != null)
            {
                anchorCount++;
                averagePosition += anchor.GetAveragePosition();
            }
        }

        if (anchorCount > 0)
        {
            transform.position = averagePosition / anchorCount;

            foreach (ComplexSliceableAnchor anchor in anchors)
            {
                if (anchor != null)
                {
                    anchor.SetParent(transform);
                }
            }
        } else
        {
            Destroy(gameObject);
        }
    }
    
    public List<ComplexSliceableAnchor> GenerateAnchorGraph(ComplexSliceableAnchor root, List<ComplexSliceableAnchor> visitedAnchors)
    {
        List<ComplexSliceableAnchor> newGraph = new List<ComplexSliceableAnchor>();
        newGraph.Add(root);
        visitedAnchors.Add(root);

        List<ComplexSliceableAnchor> connectedAnchors = root.GetConnectedAnchors();
        foreach (ComplexSliceableAnchor anchor in connectedAnchors)
        {
            if (!visitedAnchors.Contains(anchor))
            {
                newGraph.AddRange(GenerateAnchorGraph(anchor, visitedAnchors));
            }
        }

        return newGraph;
    }







    public void TraverseAnchorsAndSplitObject(float separation, float separationForce)
    {
        //treat all anchors as unvisited
        List<ComplexSliceableAnchor> unvisitedAnchors = new List<ComplexSliceableAnchor>();
        unvisitedAnchors.AddRange(anchors);

        List<List<ComplexSliceableAnchor>> anchorGraphs = new List<List<ComplexSliceableAnchor>>();

        //while there is an unvisited anchor
        while (unvisitedAnchors.Count > 0)
        {
            //traverse starting at that anchor and generate a graph of all connected anchors
            ComplexSliceableAnchor startingAnchor = unvisitedAnchors[0];
            unvisitedAnchors.RemoveAt(0);
            anchorGraphs.Add(GenerateAnchorGraphV2(startingAnchor, unvisitedAnchors));
        }

        //if there is >1 generated graph
        if (anchorGraphs.Count > 1)
        {
            //split the object into multiple objects
            foreach (List<ComplexSliceableAnchor> anchorGraph in anchorGraphs)
            {
                CreateNewParent(anchorGraph, separation, separationForce);
            }
            Destroy(gameObject);
        }
    }

    public List<ComplexSliceableAnchor> GenerateAnchorGraphV2(ComplexSliceableAnchor currentAnchor, List<ComplexSliceableAnchor> unvisitedAnchors)
    {
        List<ComplexSliceableAnchor> anchorGraph = new List<ComplexSliceableAnchor>();
        anchorGraph.Add(currentAnchor);

        foreach (ComplexSliceableAnchor anchor in currentAnchor.GetConnectedAnchors())
        {
            if (unvisitedAnchors.Contains(anchor))
            {
                unvisitedAnchors.Remove(anchor);
                anchorGraph.AddRange(GenerateAnchorGraphV2(anchor, unvisitedAnchors));
            }
        }

        return anchorGraph;
    }

    public void CreateNewParent(List<ComplexSliceableAnchor> newAnchors, float separation, float separationForce)
    {
        //create new object next to this one
        GameObject newObj = Util.InstantianteParent(gameObject, transform.parent);
        ComplexSliceableParent parent = newObj.GetComponent<ComplexSliceableParent>();
        parent.generateAnchors = false;

        //move all anchors to it
        parent.SetAnchors(newAnchors.ToArray());

        //move all children connected to anchors to it
        foreach (ComplexSliceableAnchor anchor in newAnchors)
        {
            if (anchor != null)
            {
                anchor.parent = parent;
                foreach (ComplexSliceableChild child in anchor.GetConnectedChildren())
                {
                    child.parent = parent;
                    child.transform.parent = parent.transform;
                }
            }
        }

        //reset rigidbody and position
        Rigidbody rb = parent.gameObject.GetComponent<Rigidbody>();
        rb.ResetCenterOfMass();
        rb.ResetInertiaTensor();

        Vector3 direction = (parent.transform.position - transform.position);
        parent.transform.position += direction * separation;
        StartCoroutine(ResetRigidbody(null, rb, true, direction, separationForce));
    }
}
