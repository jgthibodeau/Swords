using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrapplePoint : MonoBehaviour
{
    public enum GrappleType { TOP, BOTTOM }
    public GrappleType grappleType;

    void Start()
    {
        gameObject.layer = LayerMask.NameToLayer("Grapple");
    }
}
