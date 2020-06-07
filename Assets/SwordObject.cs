using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SwordObject : MonoBehaviour
{
    [System.Serializable]
    public class Waypoint
    {
        public Vector3 position;
        public Quaternion rotation;
        public bool swinging;
    }

    public float minWaypointDistance;
    public int maxWayPoints;
    public List<Waypoint> waypoints = new List<Waypoint>();
    
    public bool smoothWaypoints;
    [Range(0, 1)]
    public float waypointSmoothness = 0.5f;

    public Transform modelHolder;
    public Rigidbody rb;
    public Sword sword;

    public bool disableWhenNotSwinging, disableSliceColliders;
    public Collider triggerCollider, physicsCollider;

    public List<Collider> disabledColliders = new List<Collider>();

    public bool freezeBiggerMesh;

    private bool slicedObject;
    public AudioSource audioSource;

    public SwordToggle swordToggle;
    private SwordInfo swordInfo;

    // How far away from the slice do we separate resulting objects
    public float separation, separationForce;

    // Do we draw a plane object associated with the slice
    //private Plane slicePlane = new Plane();
    public bool drawPlane;

    private MeshCutter meshCutter;
    private TempMesh biggerMesh, smallerMesh;

    public Vector3 previousForward;
    public Plane slicePlane = new Plane();

    public bool sliceV2;

    private class Slice
    {
        public Vector3 start, startForward;
    }

    Dictionary<Collider, Slice> slices = new Dictionary<Collider, Slice>();


    void Start()
    {
        rb = GetComponent<Rigidbody>();

        meshCutter = new MeshCutter(256);

        previousForward = transform.forward;
    }

    void Update()
    {
        swordInfo = swordToggle.GetCurrentSword();

        if (!sword.swinging)
        {
            slicedObject = false;
            ReEnableCollisions();
        }

        if (disableWhenNotSwinging)
        {
            triggerCollider.enabled = sword.swinging;
            physicsCollider.enabled = sword.swinging;
        } else
        {
            triggerCollider.enabled = true;
            physicsCollider.enabled = true;
        }

        slicePlane.Set3Points(Vector3.zero, transform.forward, previousForward);

        if (Time.frameCount % 2 == 0)
        {
            previousForward = transform.forward;
        }

        Debug.DrawRay(transform.position, slicePlane.normal, Color.cyan);
    }

    public void AddWayPoint(Vector3 position, Quaternion rotation, bool swinging)
    {
        bool canAdd = true;
        if (waypoints.Count > 0)
        {
            Waypoint last = waypoints[waypoints.Count - 1];
            canAdd = last.position != position;
        }
        if (canAdd)
        {
            Waypoint waypoint = new Waypoint();
            waypoint.position = position;
            waypoint.rotation = rotation;
            waypoint.swinging = swinging;

            waypoints.Add(waypoint);

            if (waypoints.Count > maxWayPoints)
            {
                waypoints.RemoveAt(0);
            }

            if (smoothWaypoints)
            {
                SmoothWaypoints();
            }
        }
    }

    private void SmoothWaypoints()
    {
        List<Waypoint> points;
        List<Waypoint> curvedPoints;
        int pointsLength = 0;
        int curvedLength = 0;
        
        pointsLength = waypoints.Count;

        curvedLength = (pointsLength * Mathf.RoundToInt(waypointSmoothness)) - 1;
        if (curvedLength <= 0)
        {
            return;
        }
        curvedPoints = new List<Waypoint>(curvedLength);

        float t = 0.0f;
        for (int pointInTimeOnCurve = 0; pointInTimeOnCurve < curvedLength + 1; pointInTimeOnCurve++)
        {
            t = Mathf.InverseLerp(0, curvedLength, pointInTimeOnCurve);

            points = new List<Waypoint>(waypoints);

            for (int j = pointsLength - 1; j > 0; j--)
            {
                for (int i = 0; i < j; i++)
                {
                    points[i].position = (1 - t) * points[i].position + t * points[i + 1].position;
                    //points[i].rotation = (1 - t) * points[i].rotation + t * points[i + 1].rotation;
                }
            }

            curvedPoints.Add(points[0]);
        }

        waypoints = curvedPoints;
    }




    void FixedUpdate()
    {
        if (waypoints.Count > 0)
        {
            ////move to next waypoint
            //Waypoint currentWaypoint = waypoints[0];
            //while (waypoints.Count > 0 && Vector3.Distance(transform.position, currentWaypoint.position) < minWaypointDistance)
            //{
            //    waypoints.RemoveAt(0);
            //    if (waypoints.Count > 0)
            //    {
            //        currentWaypoint = waypoints[0];
            //    }
            //}

            //Vector3 desiredPosition = currentWaypoint.position;
            //Quaternion desiredRotation = currentWaypoint.rotation;
            //bool swinging = currentWaypoint.swinging;

            //if (swinging && Vector3.Distance(transform.position, desiredPosition) >= minWaypointDistance)
            //{
            //    desiredPosition = Vector3.MoveTowards(rb.position, desiredPosition, Time.fixedDeltaTime * swordInfo.moveSpeed);
            //    desiredRotation = Quaternion.RotateTowards(rb.rotation, desiredRotation, Time.fixedDeltaTime * swordInfo.rotateSpeed);

            //    rb.MovePosition(desiredPosition);
            //    rb.MoveRotation(desiredRotation);
            //} else
            //{
            //    rb.position = desiredPosition;
            //    rb.rotation = desiredRotation;

            //    rb.velocity = Vector3.zero;
            //    rb.angularVelocity = Vector3.zero;
            //}

            //move to next waypoint
            Waypoint currentWaypoint = waypoints[0];
            while (waypoints.Count > 0 && Vector3.Distance(transform.localPosition, currentWaypoint.position) < minWaypointDistance)
            {
                waypoints.RemoveAt(0);
                if (waypoints.Count > 0)
                {
                    currentWaypoint = waypoints[0];
                }
            }

            Vector3 desiredPosition = currentWaypoint.position;
            Quaternion desiredRotation = currentWaypoint.rotation;
            bool swinging = currentWaypoint.swinging;

            Vector3 worldPos = transform.parent.TransformPoint(desiredPosition);
            Quaternion worldRot = transform.parent.rotation * desiredRotation;

            if (Vector3.Distance(transform.localPosition, desiredPosition) >= minWaypointDistance)
            {
                float speed = swinging ? swordInfo.swingSpeed : swordInfo.moveSpeed;
                rb.MovePosition(Vector3.MoveTowards(rb.position, worldPos, Time.fixedDeltaTime * speed));
                rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, worldRot, Time.fixedDeltaTime * speed));
            }
            else
            {
                rb.position = worldPos;
                rb.rotation = worldRot;

                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if ((disableWhenNotSwinging || sword.swinging) && !disabledColliders.Contains(other))
        {
            if (swordInfo.canSlice && other.gameObject.tag == "Sliceable")
            {
                Debug.Log("slicing " + other.gameObject);
                Vector3 sliceNormal = Vector3.Cross(transform.forward, rb.velocity.normalized);

                SliceObjects(other, transform.position, slicePlane.normal, other.gameObject);
            }
            else
            {
                Breakable breakable = other.gameObject.GetComponentInParent<Breakable>();
                if (breakable != null)
                {
                    breakable.Break(100, gameObject);
                }
            }
        }

        //ignore more collisions with this object

        if (disableSliceColliders)
        {
            disabledColliders.Add(other);
            if (disableWhenNotSwinging)
            {
                Physics.IgnoreCollision(triggerCollider, other, true);
            }
        }
    }

    //void OnCollisionEnter(Collision collision)
    //{
    //    if ((disableWhenNotSwinging || sword.swinging) && collision.gameObject.tag == "Sliceable" && !disabledColliders.Contains(collision.collider))
    //    {
    //        Debug.Log("slicing " + collision.gameObject);
    //        Vector3 sliceNormal = Vector3.Cross(transform.forward, collision.GetContact(0).normal);

    //        SliceObjects(collision, transform.position, slicePlane.normal, collision.gameObject);
    //    }

    //    //ignore more collisions with this object
    //    disabledColliders.Add(collision.collider);
    //    if (disableWhenNotSwinging)
    //    {
    //        Physics.IgnoreCollision(physicsCollider, collision.collider, true);
    //    }
    //}

    void ReEnableCollisions()
    {
        if (disableWhenNotSwinging)
        {
            foreach (Collider c in disabledColliders)
            {
                if (c != null)
                {
                    Physics.IgnoreCollision(triggerCollider, c, false);
                }
            }
        }
        disabledColliders.Clear();

        //foreach(Collider c in slices.Keys)
        //{
        //    if (slices.TryGetValue(c, out Slice slice))
        //    {
        //        Vector3 normal = Vector3.Cross(slice.startForward, transform.forward);
        //        SliceObjects(slice.start, normal, c.gameObject);
        //    }
        //}
        //slices.Clear();
    }

    public Material capMaterial;

    void SliceObjects(Collision collision, Vector3 point, Vector3 normal, GameObject obj)
    {
        SliceableObject sliceableObject = collision.gameObject.GetComponent<SliceableObject>();
        if (sliceableObject != null)
        {
            SliceableObject.SliceResult sliceResult;
            if (sliceV2)
            {
                sliceResult = sliceableObject.SliceV2(transform.position, slicePlane.normal, separation, separationForce);
            }
            else
            {
                sliceResult = sliceableObject.Slice(collision, meshCutter, transform.position, slicePlane.normal, separation, separationForce);
            }
            if (sliceResult.sliced && disableWhenNotSwinging && disableSliceColliders)
            {
                if (!slicedObject)
                {
                    Debug.Log("Playing audio");
                    slicedObject = true;
                    swordInfo.sliceAudio.Play(audioSource);
                }

                foreach (Collider c in sliceResult.colliders)
                {
                    disabledColliders.Add(c);
                    Physics.IgnoreCollision(triggerCollider, c);
                }
            }
        }
    }

    void SliceObjects(Collider collider, Vector3 point, Vector3 normal, GameObject obj)
    {
        SliceableObject sliceableObject = obj.GetComponentInParent<ComplexSliceableParent>();
        if (sliceableObject == null)
        {
            sliceableObject = obj.GetComponentInParent<SliceableObject>();
        }

        Debug.Log("slicing " + sliceableObject);

        if (sliceableObject != null)
        {
            SliceableObject.SliceResult sliceResult;
            if (sliceV2)
            {
                sliceResult = sliceableObject.SliceV2(transform.position, slicePlane.normal, separation, separationForce);
            }
            else
            {
                sliceResult = sliceableObject.Slice(collider, meshCutter, transform.position, slicePlane.normal, separation, separationForce);
            }

            if (sliceResult.sliced && disableWhenNotSwinging && disableSliceColliders)
            {
                if (!slicedObject)
                {
                    Debug.Log("Playing audio");
                    slicedObject = true;
                    swordInfo.sliceAudio.Play(audioSource);
                }

                foreach (Collider c in sliceResult.colliders)
                {
                    disabledColliders.Add(c);
                    Physics.IgnoreCollision(collider, c);
                }
            }
        }
    }

    //void SliceObjects(Vector3 point, Vector3 normal, GameObject obj)
    //{
    //    // Put results in positive and negative array so that we separate all meshes if there was a cut made
    //    List<Transform> positive = new List<Transform>(),
    //                    negative = new List<Transform>();

    //    bool slicedAny = false;

    //    // We multiply by the inverse transpose of the worldToLocal Matrix, a.k.a the transpose of the localToWorld Matrix
    //    // Since this is how normal are transformed
    //    var transformedNormal = ((Vector3)(obj.transform.localToWorldMatrix.transpose * normal)).normalized;

    //    //Convert plane in object's local frame
    //    Plane plane = new Plane(transformedNormal, obj.transform.InverseTransformPoint(point));

    //    slicedAny = SliceObject(ref plane, obj, positive, negative) || slicedAny;

    //    // Separate meshes if a slice was made
    //    if (slicedAny)
    //    {
    //        SeparateMeshes(positive, negative, normal);
    //    }
    //}

    //bool SliceObject(ref Plane plane, GameObject obj, List<Transform> positiveObjects, List<Transform> negativeObjects)
    //{
    //    var mesh = obj.GetComponent<MeshFilter>().mesh;

    //    if (!meshCutter.SliceMesh(mesh, ref plane))
    //    {
    //        Debug.Log("Cant slice");
    //        // Put object in the respective list
    //        if (plane.GetDistanceToPoint(meshCutter.GetFirstVertex()) >= 0)
    //        {
    //            positiveObjects.Add(obj.transform);
    //        } else
    //        {
    //            negativeObjects.Add(obj.transform);
    //        }

    //        return false;
    //    }

    //    // TODO: Update center of mass

    //    // Silly condition that labels which mesh is bigger to keep the bigger mesh in the original gameobject
    //    bool posBigger = meshCutter.PositiveMesh.surfacearea > meshCutter.NegativeMesh.surfacearea;
    //    if (posBigger)
    //    {
    //        biggerMesh = meshCutter.PositiveMesh;
    //        smallerMesh = meshCutter.NegativeMesh;
    //    }
    //    else
    //    {
    //        biggerMesh = meshCutter.NegativeMesh;
    //        smallerMesh = meshCutter.PositiveMesh;
    //    }

    //    // Create new Sliced object with the other mesh
    //    GameObject newObject;
    //    if (obj.transform.parent != null)
    //    {
    //        newObject = Instantiate(obj, obj.transform.parent);
    //    } else
    //    {
    //        newObject = Instantiate(obj);
    //    }
    //    newObject.transform.SetPositionAndRotation(obj.transform.position, obj.transform.rotation);
    //    var newObjMesh = newObject.GetComponent<MeshFilter>().mesh;

    //    // Put the bigger mesh in the original object
    //    // TODO: Enable collider generation (either the exact mesh or compute smallest enclosing sphere)
    //    ReplaceMesh(mesh, biggerMesh);
    //    ReplaceMesh(newObjMesh, smallerMesh);

    //    (posBigger ? positiveObjects : negativeObjects).Add(obj.transform);
    //    (posBigger ? negativeObjects : positiveObjects).Add(newObject.transform);

    //    ResetCollider(obj, true);
    //    ResetCollider(newObject, false);

    //    return true;
    //}

    //private void ResetCollider(GameObject obj, bool biggerMesh)
    //{
    //    Collider originalCollider = obj.GetComponent<Collider>();
    //    PhysicMaterial originalPhysicsMaterial = originalCollider.material;
    //    Destroy(originalCollider);

    //    Collider newCollider = (Collider)obj.AddComponent(originalCollider.GetType());
    //    newCollider.material = originalPhysicsMaterial;

    //    disabledColliders.Add(newCollider);
    //    if (disableWhenNotSwinging)
    //    {
    //        Physics.IgnoreCollision(collider, newCollider, true);
    //    }

    //    if (originalCollider.GetType() == typeof(MeshCollider))
    //    {
    //        ((MeshCollider)newCollider).convex = true;
    //    }


    //    Rigidbody originalRigidbody = obj.GetComponent<Rigidbody>();
    //    originalRigidbody.ResetCenterOfMass();
    //    originalRigidbody.ResetInertiaTensor();
    //    StartCoroutine(ResetRigidbody(originalRigidbody, biggerMesh));
    //    //Vector3 originalVelocity = originalRigidbody.velocity;
    //    //Destroy(originalRigidbody);
    //    //Rigidbody newRigidBody = obj.AddComponent<Rigidbody>();
    //    //newRigidBody.velocity = originalVelocity;
    //}

    //IEnumerator ResetRigidbody(Rigidbody originalRigidbody, bool biggerMesh)
    //{
    //    if (biggerMesh && freezeBiggerMesh)
    //    {
    //        originalRigidbody.constraints = RigidbodyConstraints.FreezeAll;
    //    }

    //    Vector3 velocity = originalRigidbody.velocity;
    //    Vector3 angularVelocity = originalRigidbody.angularVelocity;

    //    originalRigidbody.isKinematic = true;
    //    yield return new WaitForEndOfFrame();
    //    yield return new WaitForEndOfFrame();
    //    originalRigidbody.ResetCenterOfMass();
    //    originalRigidbody.ResetInertiaTensor();
    //    originalRigidbody.isKinematic = false;

    //    if (biggerMesh && freezeBiggerMesh)
    //    {
    //        originalRigidbody.constraints = RigidbodyConstraints.None;
    //    } else
    //    {
    //        //originalRigidbody.velocity = velocity;
    //        //originalRigidbody.angularVelocity = angularVelocity;
    //    }
    //} 


    ///// <summary>
    ///// Replace the mesh with tempMesh.
    ///// </summary>
    //void ReplaceMesh(Mesh mesh, TempMesh tempMesh, MeshCollider collider = null)
    //{
    //    mesh.Clear();
    //    mesh.SetVertices(tempMesh.vertices);
    //    mesh.SetTriangles(tempMesh.triangles, 0);
    //    mesh.SetNormals(tempMesh.normals);
    //    mesh.SetUVs(0, tempMesh.uvs);

    //    //mesh.RecalculateNormals();
    //    mesh.RecalculateTangents();

    //    if (collider != null && collider.enabled)
    //    {
    //        collider.sharedMesh = mesh;
    //        collider.convex = true;
    //    }
    //}

    //void SeparateMeshes(Transform posTransform, Transform negTransform, Vector3 localPlaneNormal)
    //{
    //    // Bring back normal in world space
    //    Vector3 worldNormal = ((Vector3)(posTransform.worldToLocalMatrix.transpose * localPlaneNormal)).normalized;

    //    Vector3 separationVec = worldNormal * separation;
    //    // Transform direction in world coordinates
    //    posTransform.position += separationVec;
    //    negTransform.position -= separationVec;
    //}

    //void SeparateMeshes(List<Transform> positives, List<Transform> negatives, Vector3 worldPlaneNormal)
    //{
    //    int i;
    //    var separationVector = worldPlaneNormal * separation;

    //    for (i = 0; i < positives.Count; ++i)
    //    {
    //        positives[i].transform.position += separationVector;
    //    }

    //    for (i = 0; i < negatives.Count; ++i)
    //    {
    //        negatives[i].transform.position -= separationVector;
    //    }
    //}
}
