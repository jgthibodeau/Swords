using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowRotation : MonoBehaviour
{
    public Transform target;
    
    void LateUpdate()
    {
        Vector3 up = transform.parent.up * target.localPosition.y + transform.parent.right * target.localPosition.x;
        transform.rotation = Quaternion.LookRotation(transform.parent.forward, up.normalized);
    }
}
