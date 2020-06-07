using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordV2 : MonoBehaviour
{
    public float distanceToStartSwing = 0.1f;
    public float distanceToMaintainSwing = 0.1f;
    public float distanceToStopSwing = 1f;
    public float angleToMaintainSwing = 5f;

    public float trailTime = 0.1f;
    public TrailRenderer[] trails;
    private Coroutine[] disableTrails;

    private AudioSource audioSource;
    public SwordToggle swordToggle;
    private SwordInfo swordInfo;
    public Transform swordObject;

    public bool readyWhenLocked;
    [SerializeField]
    private bool readying, swinging;

    public float mouseSensitivity = 1;
    [SerializeField]
    private Vector2 lastMousePos, mousePos, swingDirection, swingStartPos;
    
    public Vector2 swingDistance;
    public float swingArc = 180;

    public LayerMask hittableLayers;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        swordInfo = swordToggle.GetCurrentSword();

        bool wasReadying = readying;
        readying = Util.GetButton("Ready") || (readyWhenLocked && Util.GetButton("Lock"));

        Vector2 mouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")) * mouseSensitivity;
        mousePos += mouseDelta;
        if (readying)
        {
            if (!swinging)
            {
                if (Vector2.Distance(lastMousePos, mousePos) > distanceToStartSwing)
                {
                    StartSwing();
                }
            }
            //else
            //{
            //    if (Vector2.Distance(mousePos, lastMousePos) < distanceToMaintainSwing ||
            //        Vector2.Distance(swingStartPos, mousePos) > distanceToStopSwing ||
            //        Vector2.Angle(swingDirection, (mousePos - swingStartPos)) > angleToMaintainSwing)
            //    {
            //        StopSwing();
            //    }
            //}

            lastMousePos = mousePos;
        } else
        {
            StopSwing();
            lastMousePos = mousePos;
        }
    }

    private void StartSwing()
    {
        swinging = true;
        swingDirection = (mousePos - lastMousePos).normalized;
        swingStartPos = lastMousePos;
        
        StopAllCoroutines();
        StartCoroutine(MoveSword());

        DoSwordCollisions();
    }

    private void StopSwing()
    {
        if (swinging)
        {
            swinging = false;
            DoSwordCollisions();
        }
    }

    private void DoSwordCollisions()
    {
        Vector3 startPosition = transform.position
            - transform.right * swingDirection.x * swingDistance.x
            - transform.up * swingDirection.y * swingDistance.y;

        Vector3 startPositionExtended = startPosition
            + transform.forward * swordInfo.length;

        Vector3 endPosition = transform.position
            + transform.right * swingDirection.x * swingDistance.x
            + transform.up * swingDirection.y * swingDistance.y;

        Vector3 endPositionExtended = endPosition
            + transform.forward * swordInfo.length;

        Vector3 direction = endPosition - startPosition;

        //Debug.DrawLine(startPosition, startPositionExtended);
        //Debug.DrawLine(startPosition, endPosition);
        //Debug.DrawLine(endPosition, endPositionExtended);
        //Debug.DrawLine(startPositionExtended, startPositionExtended);
        //Debug.Break();


        RaycastHit[] hits = Physics.CapsuleCastAll(
            startPosition,
            startPositionExtended,
            swordInfo.radius,
            direction.normalized,
            direction.magnitude,
            hittableLayers
            );

        foreach(RaycastHit hit in hits)
        {

        }
    }
    
    private IEnumerator MoveSword()
    {
        Quaternion rotation = Quaternion.Euler(
            0,
            0,
            Vector2.SignedAngle(Vector2.up, swingDirection)
        );
        swordObject.localRotation = rotation;
        swordObject.RotateAround(swordObject.position, swordObject.right, swingArc / 2);

        Vector3 startPosition = new Vector3(
            -swingDirection.x * swingDistance.x,
            -swingDirection.y * swingDistance.y
        );

        //Vector3 movementDirection = new Vector3(
        //    swingDirection.x * swingDistance.x * 2,
        //    swingDirection.y * swingDistance.y * 2
        //    );

        //Vector3 endPosition = startPosition + movementDirection;

        Vector3 endPosition = new Vector3(
            swingDirection.x * swingDistance.x,
            swingDirection.y * swingDistance.y
        );

        //Debug.Log("moving sword from " + startPosition + " to " + endPosition);

        swordObject.localPosition = startPosition;

        //d = rt
        float time = 0;
        float distance = Vector3.Distance(startPosition, endPosition);
        float swingTime = distance / swordInfo.swingSpeed;
        while (time < swingTime)
        {
            //Debug.Log("moving sword " + lerped);
            //swordObject.localPosition += movementDirection * Time.deltaTime / swordInfo.swingTime;
            swordObject.localPosition = Vector3.MoveTowards(swordObject.localPosition, endPosition, swordInfo.swingSpeed * Time.deltaTime);

            float timeChunk = Time.deltaTime / swingTime;
            //swordObject.Rotate(swingArc * timeChunk, 0, 0, Space.Self);
            swordObject.RotateAround(swordObject.position, swordObject.right, -swingArc * timeChunk);

            time += Time.deltaTime;
            yield return null;
        }

        swordObject.localPosition = endPosition;
        //yield return null;
        //swordObject.localPosition = Vector3.zero;
        //swordObject.localRotation = Quaternion.Euler(0, 0, 180);

        swinging = false;
    }
}
