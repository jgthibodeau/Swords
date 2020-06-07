using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SliceableObject : MonoBehaviour
{
    public class SliceResult
    {
        public List<Collider> colliders = new List<Collider>();
        public Collider biggerSlicedObject, smallerSlicedObject;
        public bool sliced = false;
    }

    public bool freezeBiggerMesh;
    public bool sliceable = true;

    private float minSizeToKill = 0.5f;//  0.75f;//0.5f;//0.075f;
    private float minSizeToSlice = 0.15f;//0.5f;//0.075f;
    private float minSizeToExist = 0.05f;

    public Material capMaterial;

    public bool resetRigidbody;

    void Start()
    {
        if (capMaterial == null)
        {
            //Debug.LogError("Missing cap material");
            capMaterial = new Material(Shader.Find("Diffuse"));
        }
    }

    void Update()
    {
        if (resetRigidbody)
        {
            resetRigidbody = false;
            StartResetRigidbody(null, GetComponent<Rigidbody>(), false, Vector3.zero, 0);
        }
    }

    public void SetSliceable(Vector3 extents)
    {
        if (sliceable)
        {
            if (extents.magnitude < minSizeToExist)
            {
                Destroy(gameObject);
            }
            else if (extents.magnitude < minSizeToKill)
            {
                SetKillable();

                if (extents.magnitude < minSizeToSlice)
                {
                    SetUnsliceable();
                } else
                {
                    SetSliceable();
                }
            } else
            {
                SetSliceable();
            }
        }
    }

    private void SetKillable()
    {
        Kill kill = gameObject.AddComponent<Kill>();
        kill.minLife = 5;
        kill.maxLife = 10;
    }

    private void SetUnsliceable()
    {
        sliceable = false;
        Util.SetLayerRecursively(gameObject, LayerMask.NameToLayer("PhysicsObject"));
        Util.SetTagRecursively(gameObject, "Untagged");
        Destroy(this);
    }

    private void SetSliceable()
    {
        sliceable = true;
        Util.SetLayerRecursively(gameObject, LayerMask.NameToLayer("Sliceable"));
        Util.SetTagRecursively(gameObject, "Sliceable");
    }

    public virtual SliceResult SliceV2(Vector3 position, Vector3 normal, float separation, float separationForce)
    {
        Debug.Log("slice v2 " + gameObject);
        GameObject sliceableChild = sliceableChild = transform.GetChild(0).gameObject;
        GameObject[] pieces = BLINDED_AM_ME.MeshCut.Cut(sliceableChild, position, normal, capMaterial);

        SliceResult sliceResult = new SliceResult();

        if (pieces != null && pieces.Length > 1)
        {
            GameObject newParent = Util.InstantianteParent(gameObject, transform.parent);
            pieces[1].transform.parent = newParent.transform;

            float size1 = pieces[0].GetComponent<MeshFilter>().mesh.bounds.extents.sqrMagnitude;
            float size2 = pieces[1].GetComponent<MeshFilter>().mesh.bounds.extents.sqrMagnitude;
            bool firstPieceBigger = size1 > size2;
            
            if (firstPieceBigger)
            {
                sliceResult.biggerSlicedObject = ResetCollider(pieces[0], true, normal, separationForce, pieces[0].GetComponent<Collider>(), true);
                sliceResult.smallerSlicedObject = ResetCollider(pieces[1], false, -normal, separationForce, pieces[0].GetComponent<Collider>(), false);
            } else
            {
                sliceResult.smallerSlicedObject = ResetCollider(pieces[0], false, -normal, separationForce, pieces[0].GetComponent<Collider>(), true);
                sliceResult.biggerSlicedObject = ResetCollider(pieces[1], true, normal, separationForce, pieces[0].GetComponent<Collider>(), false);
            }
            sliceResult.colliders.Add(sliceResult.biggerSlicedObject);
            sliceResult.colliders.Add(sliceResult.smallerSlicedObject);
            sliceResult.sliced = true;
        }

        return sliceResult;
    }






    public virtual SliceResult Slice(Collider collider, MeshCutter meshCutter, Vector3 point, Vector3 normal, float separation, float separationForce)
    {
        return Slice(new Collision(), meshCutter, point, normal, separation, separationForce);
    }

    public virtual SliceResult Slice(Collision collision, MeshCutter meshCutter, Vector3 point, Vector3 normal, float separation, float separationForce)
    {
        if (!sliceable)
        {
            return new SliceResult();
        }

        // Put results in positive and negative array so that we separate all meshes if there was a cut made
        List<Transform> positive = new List<Transform>(),
                        negative = new List<Transform>();

        GameObject sliceableChild = sliceableChild = transform.GetChild(0).gameObject;
        SliceResult sliceResult = SliceObject(meshCutter, point, normal, sliceableChild, positive, negative, separationForce);

        // Separate meshes if a slice was made
        if (sliceResult.sliced)
        {
            SeparateMeshes(positive, negative, normal, separation);
        }

        return sliceResult;
    }

    SliceResult SliceObject(MeshCutter meshCutter, Vector3 point, Vector3 normal, GameObject obj, List<Transform> positiveObjects, List<Transform> negativeObjects, float separationForce)
    {
        SliceResult sliceResult = new SliceResult();

        var mesh = obj.GetComponent<MeshFilter>().mesh;

        //if (!meshCutter.SliceMesh(mesh, ref plane))
        //{
        //    Debug.Log("Cant slice");
        //    // Put object in the respective list
        //    if (plane.GetDistanceToPoint(meshCutter.GetFirstVertex()) >= 0)
        //    {
        //        positiveObjects.Add(obj.transform);
        //    }
        //    else
        //    {
        //        negativeObjects.Add(obj.transform);
        //    }

        //    return sliceResult;
        //}

        //// Silly condition that labels which mesh is bigger to keep the bigger mesh in the original gameobject
        //TempMesh biggerMesh, smallerMesh;
        //bool posBigger = meshCutter.PositiveMesh.surfacearea > meshCutter.NegativeMesh.surfacearea;
        //if (posBigger)
        //{
        //    biggerMesh = meshCutter.PositiveMesh;
        //    smallerMesh = meshCutter.NegativeMesh;
        //}
        //else
        //{
        //    biggerMesh = meshCutter.NegativeMesh;
        //    smallerMesh = meshCutter.PositiveMesh;
        //}




        //slice again
        bool posBigger;
        TempMesh biggerMesh, smallerMesh;
        //plane.Translate(normal * separationForce);

        // We multiply by the inverse transpose of the worldToLocal Matrix, a.k.a the transpose of the localToWorld Matrix
        // Since this is how normal are transformed
        var transformedNormal = ((Vector3)(obj.transform.localToWorldMatrix.transpose * normal)).normalized;
        //Convert plane in object's local frame
        Plane plane = new Plane(transformedNormal, obj.transform.InverseTransformPoint(point));

        if (!GenerateMeshes(mesh, meshCutter, ref plane, obj, positiveObjects, negativeObjects, out biggerMesh, out smallerMesh, out posBigger))
        {
            return sliceResult;
        }



        // Create new Sliced object with the other mesh
        //
        GameObject newObjParent = Util.InstantianteParent(gameObject, transform.parent);
        GameObject newObject = Instantiate(obj, newObjParent.transform);

        newObject.transform.SetPositionAndRotation(obj.transform.position, obj.transform.rotation);
        var newObjMesh = newObject.GetComponent<MeshFilter>().mesh;
        
        // Put the bigger mesh in the original object
        // TODO: Enable collider generation (either the exact mesh or compute smallest enclosing sphere)
        ReplaceMesh(mesh, biggerMesh);
        ReplaceMesh(newObjMesh, smallerMesh);

        Vector3 forceNormal = normal;
        if (posBigger)
        {
            positiveObjects.Add(obj.transform);
            negativeObjects.Add(newObject.transform);
        } else
        {
            forceNormal *= -1;
            negativeObjects.Add(obj.transform);
            positiveObjects.Add(newObject.transform);
        }

        sliceResult.biggerSlicedObject = ResetCollider(obj, true, forceNormal, separationForce, obj.GetComponent<Collider>(), true);
        sliceResult.smallerSlicedObject = ResetCollider(newObject, false, -forceNormal, separationForce, newObject.GetComponent<Collider>(), true);
        sliceResult.colliders.Add(sliceResult.biggerSlicedObject);
        sliceResult.colliders.Add(sliceResult.smallerSlicedObject);
        sliceResult.sliced = true;

        return sliceResult;
    }

    public bool GenerateMeshes(Mesh mesh, MeshCutter meshCutter, ref Plane plane, GameObject obj, List<Transform> positiveObjects, List<Transform> negativeObjects, out TempMesh biggerMesh, out TempMesh smallerMesh, out bool posBigger)
    {
        if (!meshCutter.SliceMesh(mesh, ref plane))
        {
            Debug.Log("Cant slice");
            //// Put object in the respective list
            //if (plane.GetDistanceToPoint(meshCutter.GetFirstVertex()) >= 0)
            //{
            //    positiveObjects.Add(obj.transform);
            //}
            //else
            //{
            //    negativeObjects.Add(obj.transform);
            //}
            biggerMesh = null;
            smallerMesh = null;
            posBigger = false;
            //Debug.Break();
            return false;
        }

        // Silly condition that labels which mesh is bigger to keep the bigger mesh in the original gameobject
        //TempMesh biggerMesh, smallerMesh;
        posBigger = meshCutter.PositiveMesh.surfacearea > meshCutter.NegativeMesh.surfacearea;
        if (posBigger)
        {
            biggerMesh = meshCutter.PositiveMesh;
            smallerMesh = meshCutter.NegativeMesh;
        }
        else
        {
            biggerMesh = meshCutter.NegativeMesh;
            smallerMesh = meshCutter.PositiveMesh;
        }

        return true;
    }

    private Collider ResetCollider(GameObject obj, bool biggerMesh, Vector3 forceNormal, float separationForce, Collider originalCollider, bool destroyOriginalCollider)
    {
        //Collider originalCollider = obj.GetComponent<Collider>();
        PhysicMaterial originalPhysicsMaterial = originalCollider.material;
        if (destroyOriginalCollider)
        {
            Destroy(originalCollider);
        }

        Collider newCollider = (Collider)obj.AddComponent(originalCollider.GetType());
        newCollider.material = originalPhysicsMaterial;

        if (originalCollider.GetType() == typeof(MeshCollider))
        {
            ((MeshCollider)newCollider).convex = true;
        }

        
        obj.GetComponentInParent<SliceableObject>().SetSliceable(newCollider.bounds.extents);
        Util.DestroySiblings(obj);

        ////reset position
        ////store vector from parent position to collider center
        //Vector3 meshCenter = newCollider.bounds.center;
        ////Vector3 meshCenter = obj.transform.TransformPoint(newCollider.bounds.center);
        ////Vector3 meshCenter = obj.transform.TransformPoint(((MeshCollider)newCollider).sharedMesh.bounds.center);
        //Vector3 offset = obj.transform.parent.position - meshCenter;
        ////move parent position to collider center
        //obj.transform.parent.position = meshCenter;
        ////move object position reverse of stored vector
        //obj.transform.position -= offset;



        Rigidbody originalRigidbody = obj.transform.parent.GetComponent<Rigidbody>();
        if (originalRigidbody != null)
        {
            originalRigidbody.ResetCenterOfMass();
            originalRigidbody.ResetInertiaTensor();

            //originalRigidbody.centerOfMass = Vector3.zero;

            //StartCoroutine(ResetRigidbody(originalRigidbody, biggerMesh));

            //Vector3 originalVelocity = originalRigidbody.velocity;
            //Destroy(originalRigidbody);
            //Rigidbody newRigidBody = obj.AddComponent<Rigidbody>();
            //newRigidBody.velocity = originalVelocity;


            /*StartCoroutine(*/ResetPosition(obj, originalRigidbody)/*)*/;
            StartCoroutine(ResetRigidbody(obj, originalRigidbody, !biggerMesh, forceNormal, separationForce));


            ////reset position
            ////store vector from parent position to collider center
            //Vector3 center = obj.transform.TransformPoint(originalRigidbody.centerOfMass);
            ////Vector3 center = obj.transform.TransformPoint(newCollider.bounds.center);
            ////Vector3 center = obj.transform.TransformPoint(((MeshCollider)newCollider).sharedMesh.bounds.center);
            //Vector3 offset = originalRigidbody.position - center;
            ////move parent position to collider center
            //originalRigidbody.position = center;
            ////move object position reverse of stored vector
            //obj.transform.position -= offset;
        } else
        {
            /*StartCoroutine(*/ResetPosition(obj, newCollider)/*)*/;
        }





        ////make a copy of the collider, but slightly bigger and with the slicedPhysicsMaterial
        //Util.DestroyChildren(obj);
        //GameObject child = new GameObject();
        //child.transform.parent = obj.transform;
        //child.transform.localPosition = Vector3.zero;
        //child.transform.rotation = Quaternion.identity;
        //Collider childCollider = (Collider)child.AddComponent(originalCollider.GetType());
        //childCollider.sharedMaterial = slicedPhysicsMaterial;
        //if (originalCollider.GetType() == typeof(MeshCollider))
        //{
        //    ((MeshCollider)childCollider).convex = true;
        //    ((MeshCollider)childCollider).sharedMesh = ((MeshCollider)newCollider).sharedMesh;
        //}
        //child.transform.localScale = new Vector3(1.01f, 1.01f, 1.01f);
        //child.layer = LayerMask.NameToLayer("Slice");


        return newCollider;
    }

    /*IEnumerator*/ void ResetPosition(GameObject obj, Rigidbody originalRigidbody)
    {
        //yield return null;
        //reset position
        //store vector from parent position to collider center
        Vector3 meshCenter = originalRigidbody.worldCenterOfMass;
        Vector3 offset = meshCenter - obj.transform.parent.position;
        Vector3 originalPosition = obj.transform.position;
        //move parent position to collider center
        originalRigidbody.position = meshCenter;
        //move object position reverse of stored vector
        obj.transform.position -= offset;
    }

    /*IEnumerator */ void ResetPosition(GameObject obj, Collider newCollider)
    {
        //yield return null;
        //reset position
        //store vector from parent position to collider center
        Vector3 meshCenter = newCollider.bounds.center;
        //Vector3 meshCenter = obj.transform.TransformPoint(((MeshCollider)newCollider).sharedMesh.bounds.center);
        Vector3 offset = meshCenter - obj.transform.parent.position;
        Vector3 originalPosition = obj.transform.position;// + offset;
        //move parent position to collider center
        obj.transform.parent.position = meshCenter;
        //move object position reverse of stored vector
        //obj.transform.position += offset;
        obj.transform.position = originalPosition;
    }

    public void StartResetRigidbody(GameObject obj, Rigidbody originalRigidbody, bool applyForce, Vector3 forceNormal, float separationForce)
    {
        StartCoroutine(ResetRigidbody(obj, originalRigidbody, applyForce, forceNormal, separationForce));
    }

    public IEnumerator ResetRigidbody(GameObject obj, Rigidbody originalRigidbody, bool applyForce, Vector3 forceNormal, float separationForce)
    {
        ////if (biggerMesh && freezeBiggerMesh)
        ////{
        //originalRigidbody.constraints = RigidbodyConstraints.FreezeAll;
        ////}

        //Vector3 velocity = originalRigidbody.velocity;
        //Vector3 angularVelocity = originalRigidbody.angularVelocity;

        yield return null;

        yield return new WaitForEndOfFrame();
        if (originalRigidbody == null)
        {
            Debug.LogWarning("no original rigidbody...");
            yield break;
        }

        originalRigidbody.isKinematic = true;

        yield return new WaitForEndOfFrame();
        if (originalRigidbody == null)
        {
            Debug.LogWarning("no original rigidbody...");
            yield break;
        }

        originalRigidbody.ResetCenterOfMass();
        originalRigidbody.ResetInertiaTensor();
        originalRigidbody.isKinematic = false;

        //if (biggerMesh && freezeBiggerMesh)
        //{
        //originalRigidbody.constraints = RigidbodyConstraints.None;
        //}
        //else
        //{
        //    //originalRigidbody.velocity = velocity;
        //    //originalRigidbody.angularVelocity = angularVelocity;
        //}

        if (applyForce)
        {
            Vector3 separationVector = (forceNormal + Util.RandomPerpendicularVector(forceNormal)) * separationForce;
            //positives[i].transform.position += Quaternion.AngleAxis(Random.Range(0, 360), worldPlaneNormal) * worldPlaneNormal * separation;
            originalRigidbody.AddForce(separationVector, ForceMode.Impulse);
        }

        if (obj != null)
        {
            ResetPosition(obj, originalRigidbody);
        }
    }


    /// <summary>
    /// Replace the mesh with tempMesh.
    /// </summary>
    void ReplaceMesh(Mesh mesh, TempMesh tempMesh, MeshCollider collider = null)
    {
        mesh.Clear();
        mesh.SetVertices(tempMesh.vertices);
        mesh.SetTriangles(tempMesh.triangles, 0);
        mesh.SetNormals(tempMesh.normals);
        if (mesh.uv != null && mesh.uv.Length > 0)
            mesh.SetUVs(0, tempMesh.uvs);

        //mesh.RecalculateNormals();
        mesh.RecalculateTangents();

        if (collider != null && collider.enabled)
        {
            collider.sharedMesh = mesh;
            collider.convex = true;
        }
    }

    void SeparateMeshes(Transform posTransform, Transform negTransform, Vector3 localPlaneNormal, float separation)
    {
        // Bring back normal in world space
        Vector3 worldNormal = ((Vector3)(posTransform.worldToLocalMatrix.transpose * localPlaneNormal)).normalized;

        Vector3 separationVec = worldNormal * separation;
        // Transform direction in world coordinates
        posTransform.position += separationVec;
        negTransform.position -= separationVec;
    }

    void SeparateMeshes(List<Transform> positives, List<Transform> negatives, Vector3 worldPlaneNormal, float separation)
    {
        int i;
        var separationVector = worldPlaneNormal * separation;
        
        for (i = 0; i < positives.Count; ++i)
        {
            positives[i].transform.position += separationVector;
            positives[i].transform.position += Util.RandomPerpendicularVector(worldPlaneNormal) * separation;
        }

        for (i = 0; i < negatives.Count; ++i)
        {
            negatives[i].transform.position -= separationVector;
            positives[i].transform.position += Util.RandomPerpendicularVector(worldPlaneNormal) * separation;
        }
    }
}
