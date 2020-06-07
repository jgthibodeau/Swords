using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class HingedBreakableObject : MonoBehaviour
{
    public float minBreakForce;
    public PlayAudio breakAudio;
    public Rigidbody rb;
    List<HingedBreakableChild> children = new List<HingedBreakableChild>();

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();

        foreach (Transform t in transform)
        {
            if (t != transform)
            {
                children.Add(t.gameObject.AddComponent<HingedBreakableChild>());
            }
        }
    }

    public void RemoveChild(HingedBreakableChild child)
    {
        children.Remove(child);
        if (children.Count == 1)
        {
            children[0].BreakHinges();
        }
    }
}
