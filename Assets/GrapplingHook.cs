using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrapplingHook : MonoBehaviour
{
    public PlayerController playerController;

    public UnityStandardAssets.Characters.FirstPerson.FirstPersonController fpsController;
    public LineRenderer lineRenderer;
    public Transform hookObject;

    public CameraShake cameraShake;
    public float shakeAmount, shakeTime;
    
    public LockOnController.TargetInfo grappleTargetInfo;

    public Transform currentTarget, availableTarget;
    public GrapplePoint grapplePoint;
    public LayerMask grappleLayers;

    public float grappleGravity;

    public bool isGrappling;
    public float currentGrappleSpeed, grappleAcceleration, minGrappleSpeed, maxGrappleSpeed;
    public float minDistance;

    public float middlePointHeight, middlePointDepth;
    public float middlePointRopeHeight;
    public float parabolaPointDistance;

    public float grappleDrawSpeed;

    public bool isPullingRope, isThrowingRope, grappleHookMoving, isMoving;
    private Vector3 currentHookPoint, currentHookEnd;
    public Vector3 desiredGrapplePosition;
    
    private Coroutine throwCoroutine, moveCoroutine;

    public PlayAudio throwAudio;
    public PlayAudio ropeAudio;

    private AudioSource audioSource;

    private int ignorRaycastLayer;

    void Start()
    {
        ignorRaycastLayer = LayerMask.NameToLayer("Ignore Raycast");
        audioSource = GetComponent<AudioSource>();
        fpsController = GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>();
    }

    // Update is called once per frame
    void Update()
    {
        AqcuireTarget();

        if (Input.GetButtonDown("Grapple") && HasGrappleTarget())
        {
            currentGrappleSpeed = minGrappleSpeed;

            initialRopeOffset = Random.value;

            throwAudio.Play(audioSource);

            currentTarget = availableTarget;

            grapplePoint = availableTarget.GetComponent<GrapplePoint>();

            isGrappling = true;
            if (throwCoroutine != null)
            {
                Debug.Log("Halting existing throw");
                StopCoroutine(throwCoroutine);
            }
            Debug.Log("Throwing to " + currentTarget);
            throwCoroutine = StartCoroutine(ThrowRope(currentTarget));
        }

        //if (Input.GetButton("Jump"))
        //{
        //    if (grappleCoRoutine != null)
        //    {
        //        StopCoroutine(grappleCoRoutine);
        //    }
        //    StopGrapple();
        //}

        if (isGrappling) {

            if (grappleHookMoving) {
	        	hookObject.position = currentHookPoint;
	        	hookObject.rotation = Quaternion.LookRotation(currentHookPoint - transform.position, Vector3.up);
            } else {
	        	hookObject.position = Util.HiddenPoint;
	        }

            if (isMoving)
            {
                //fpsController.EnableWalk(false);
                //fpsController.EnableGravity(false);

                Vector3 direction = desiredGrapplePosition - transform.position;

                currentGrappleSpeed = Mathf.Min(currentGrappleSpeed + grappleAcceleration * Time.deltaTime, maxGrappleSpeed);
                Vector3 moveVector = direction.normalized * currentGrappleSpeed;

                //if (grapplePoint.grappleType == GrapplePoint.GrappleType.TOP)
                //{
                //    moveVector = Vector3.ClampMagnitude(moveVector, direction.magnitude);
                //}

                //fpsController.m_CharacterController.Move(moveVector);
                fpsController.m_MoveDir = moveVector;
            }

            //fpsController.EnableCollisions(!isPullingRope);

            if (moveCoroutine == null && throwCoroutine == null)
            {
                playerController.StateEnd(PlayerController.PlayerState.GRAPPLING);
                //Halt();
            }
        } else
        {
            //fpsController.EnableCollisions(true);
            //fpsController.EnableWalk(true);
            hookObject.position = Util.HiddenPoint;
            currentTarget = null;
        }
    }

    //void FixedUpdate()
    //{
    //    if (isGrappling && isMoving)
    //    {
    //        fpsController.m_CharacterController.Move(desiredGrappleDirection);
    //    }
    //}

    private void AqcuireTarget()
    {
        int originalLayer = 0;
        if (currentTarget != null)
        {
            originalLayer = currentTarget.gameObject.layer;
            currentTarget.gameObject.layer = ignorRaycastLayer;
        }

        availableTarget = grappleTargetInfo.FindTarget(fpsController.m_Camera.transform.position, fpsController.m_Camera.transform.forward, grappleLayers);

        if (currentTarget != null)
        {
            currentTarget.gameObject.layer = originalLayer;
        }
    }

    public int numberRopePoints = 100;
    public float sinScale = 100;
    public float ropeWidthScale = 5f;
    private float initialRopeOffset;
    void LateUpdate()
    {
        if (isThrowingRope || isPullingRope)
        {
            lineRenderer.enabled = true;

            Vector3[] linePositions;
            if (grappleHookMoving)
            {
                linePositions = new Vector3[numberRopePoints];

                Vector3 direction = currentHookPoint - transform.position;
                direction /= numberRopePoints;

                for(int i = 0; i < numberRopePoints; i++)
                {
                    float value = initialRopeOffset + (float)i / numberRopePoints;
                    value *= sinScale;

                    Vector3 distanceOffset = direction * i;
                    Vector3 sinOffsetX = hookObject.right * ropeWidthScale * Mathf.Sin(value);
                    Vector3 sinOffsetY = hookObject.up * ropeWidthScale * Mathf.Sin(value/2);
                    Vector3 point = transform.position + distanceOffset + sinOffsetX + sinOffsetY;

                    linePositions[i] = point;
                }
            }
            else
            {
                //linePositions = new Vector3[] { transform.position, currentHookPoint };

                linePositions = new Vector3[numberRopePoints];

                Vector3 direction = currentHookPoint - transform.position;
                direction /= numberRopePoints;

                for (int i = 0; i < numberRopePoints; i++)
                {
                    Vector3 distanceOffset = direction * i;
                    Vector3 point = transform.position + distanceOffset;

                    linePositions[i] = point;
                }
            }
            lineRenderer.positionCount = linePositions.Length;
            lineRenderer.SetPositions(linePositions);
        } else
        {
            lineRenderer.enabled = false;
        }
    }

    bool HasGrappleTarget()
    {
        return availableTarget != null && availableTarget != currentTarget;
    }

    IEnumerator ThrowRope(Transform target)
    {
        isThrowingRope = true;
        Vector3 startPosition, finalPosition, middlePosition;

        //send out grapple rope
		grappleHookMoving = true;
        currentHookPoint = transform.position;
        float passedTime = 0;
        while (Vector3.Distance(currentHookPoint, transform.position) < Vector3.Distance(target.position, transform.position))
        {
            Vector3 direction = (target.position - transform.position).normalized;
            passedTime += Time.deltaTime;
            currentHookPoint = transform.position + direction * grappleDrawSpeed * passedTime;
            yield return null;
        }
        currentHookPoint = target.position;
        grappleHookMoving = false;
        

        //create path
        startPosition = transform.position;
        finalPosition = target.position;

        middlePosition = (startPosition + finalPosition) / 2;
        switch (grapplePoint.grappleType)
        {
            case GrapplePoint.GrappleType.TOP:
                finalPosition += Vector3.up * fpsController.m_CharacterController.height / 2;

                if (startPosition.y - finalPosition.y < middlePointHeight)
                {
                    middlePosition.y = finalPosition.y;
                }
                middlePosition.y += middlePointHeight;
                break;
            case GrapplePoint.GrappleType.BOTTOM:
                finalPosition -= Vector3.up * fpsController.m_CharacterController.height / 2;

                if (finalPosition.y - startPosition.y < middlePointHeight)
                {
                    middlePosition.y = finalPosition.y;
                }
                middlePosition.y -= middlePointDepth;
                break;
        }
        
        List<Vector3> pathPositions = Util.GeneratePath(startPosition, middlePosition, finalPosition, parabolaPointDistance);
        DrawPath(pathPositions);

        cameraShake.Shake(shakeAmount, shakeTime);
        ropeAudio.Play(audioSource);
        
        Debug.Log("Done throwing");

        if (moveCoroutine != null)
        {
            Debug.Log("Halting existing move");
            StopCoroutine(moveCoroutine);
        }
        Debug.Log("Moving");
        moveCoroutine = StartCoroutine(MoveAlongPath(grapplePoint, pathPositions));

        isThrowingRope = false;
        throwCoroutine = null;
    }

    private IEnumerator MoveAlongPath(GrapplePoint grapplePoint, List<Vector3> pathPositions)
    {
        playerController.StateStart(PlayerController.PlayerState.GRAPPLING);

        fpsController.EnableCollisions(false);
        fpsController.EnableWalk(false);
        fpsController.EnableGravity(false);

        isPullingRope = true;
        isMoving = true;

        int numberPoints = pathPositions.Count;
        Vector3 targetPosition, nextPosition;
        Vector3 startPosition = pathPositions[0];
        for (int i=0; i < numberPoints - 1; i++)
        {
            targetPosition = pathPositions[i];
            nextPosition = pathPositions[i+1];
            desiredGrapplePosition = targetPosition;

            //while (Vector3.Distance(transform.position, desiredGrapplePosition) > minDistance && Vector3.Distance(transform.position, desiredGrapplePosition) < Vector3.Distance(transform.position, nextPosition))
            //while (Vector3.Distance(transform.position, desiredGrapplePosition) > minDistance && Vector3.Distance(transform.position, startPosition) < Vector3.Distance(startPosition, desiredGrapplePosition))
            while (Vector3.Distance(transform.position, desiredGrapplePosition) > minDistance && Vector3.Distance(transform.position, desiredGrapplePosition) < Vector3.Distance(transform.position, nextPosition) && Vector3.Distance(transform.position, startPosition) < Vector3.Distance(startPosition, desiredGrapplePosition))
            {
                Debug.DrawLine(transform.position, desiredGrapplePosition);
                DrawPath(pathPositions);
                yield return null;
            }
        }

        desiredGrapplePosition = pathPositions[numberPoints-1];
        while (Vector3.Distance(transform.position, desiredGrapplePosition) > minDistance && Vector3.Distance(transform.position, startPosition) < Vector3.Distance(startPosition, desiredGrapplePosition))
        {
            Debug.DrawLine(transform.position, desiredGrapplePosition);
            DrawPath(pathPositions);
            yield return null;
        }

        
        isPullingRope = false;
        if (grapplePoint.grappleType == GrapplePoint.GrappleType.BOTTOM)
        {
            //    Debug.Log("Continuing movement");
            Vector3 direction = (pathPositions[numberPoints - 1] - pathPositions[numberPoints - 2]);
            desiredGrapplePosition = transform.position + direction;
            //    moveCoroutine = StartCoroutine(MoveAfterPath(direction));
        } else
        {
            fpsController.m_MoveDir = Vector3.zero;
        }
        //{
        Debug.Log("Done moving");
        isMoving = false;
        moveCoroutine = null;
        //}
    }

    private IEnumerator MoveAfterPath(Vector3 initialDirection)
    {
        fpsController.EnableCollisions(true);
        fpsController.EnableGravity(true);

        hookObject.position = Util.HiddenPoint;

        Vector3 moveVector = initialDirection.normalized;

        while (!fpsController.m_CharacterController.isGrounded)
        {
            Debug.DrawRay(transform.position, moveVector, Color.red);

            desiredGrapplePosition = transform.position + moveVector;// * currentGrappleSpeed * Time.deltaTime;

            //moveVector += Vector3.down * grappleGravity * Time.deltaTime;
            moveVector = moveVector.normalized;

            yield return null;
        }

        Debug.Log("Done moving");
        fpsController.EnableWalk(true);
        isMoving = false;
        moveCoroutine = null;
    }

    public void Halt()
    {
        isPullingRope = false;
        isGrappling = false;
        isMoving = false;
        fpsController.EnableCollisions(true);
        fpsController.EnableWalk(true);
        fpsController.EnableGravity(true);

        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
            moveCoroutine = null;
        }

        if (throwCoroutine != null)
        {
            StopCoroutine(throwCoroutine);
            throwCoroutine = null;
        }
    }

    private void DrawPath(List<Vector3> positions)
    {
    	if (positions.Count < 2) {
    		return;
    	}
        for(int i = 0; i < positions.Count - 2; i++)
        {
            Debug.DrawLine(positions[i], positions[i + 1], Color.cyan);
        }
    }
}
