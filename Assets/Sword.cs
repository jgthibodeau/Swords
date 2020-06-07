using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sword : MonoBehaviour
{
    public float moveSpeed, rotateSpeed;
    public float minAttackAngle, maxAttackAngle;
    public float maxX, maxY, maxZ;
    public float blockOffset, blockDistanceX, blockDistanceY;
    public float stabOffset;
    public float mouseSensitivity, mouseMaxMagnitude;

    private Vector2 previousPosition;
    public Transform anchor;
    public SwordObject swordObject;
    public Transform swordRotateObject;
    public Transform swordSliceObject;
    public Transform swordDamagePoint;
    //private Vector3 previousSwordDamagePoint;
    private Vector3 previousSwordPosition;

    public float trailTime = 0.1f;
    public TrailRenderer[] trails;
    private Coroutine[] disableTrails;

    private Animator animator;
    //private Rigidbody rb;

    public Vector3 defaultPosition;
    public Vector3 defaultRotation;

    private Vector2 mouse;
    //private float mousePosX = 0;
    //private float mousePosY = 0;
    
    public float defaultMass, swingMass;
    public bool swinging = false;
    public bool freeSwing = true;
    public float swingAngleScale = 1;
    public float minAngleToSwing, maxAngleToSwing, minSwingDistance, minSwingSpeed, maxSwingSpeed, swingStopSpeed;
    private Vector2 swingStartPosition, swingLastPosition;
    private AudioSource audioSource;

    public SwordToggle swordToggle;
    private SwordInfo swordInfo;

    public bool readyWhenLocked = false;

    private bool readying, blocking, stabbing, swordLocked;
    public Vector2 input, previousInput;

    // Start is called before the first frame update
    void Start()
    {
        defaultPosition = transform.localPosition;
        defaultRotation = transform.localEulerAngles;
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        //rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        swordInfo = swordToggle.GetCurrentSword();

        GetInput();

        if (readying || blocking)
        {
            //if (!locked)
            //{
            //    AqcuireLockTarget();
            //    locked = true;
            //    fpsControllerRotation = fpsController.transform.localRotation;
            //    fpsCameraRotation = fpsCamera.localRotation;
            //} else
            //{
            //    UpdateLockTarget();
            //}

            //fpsController.m_MouseLook.XSensitivity = 0;
            //fpsController.m_MouseLook.YSensitivity = 0;
            //fpsController.m_ManuallyRotate = true;

            UpdateSword();
        } else
        {
            //fpsController.m_MouseLook.XSensitivity = fpsMouseSenitivity.x;
            //fpsController.m_MouseLook.YSensitivity = fpsMouseSenitivity.y;
            //fpsController.m_ManuallyRotate = false;

            //if (locked)
            //{
            //    lockTarget = null;
            //    locked = false;
            //}

            ResetSword();

            swordObject.waypoints.Clear();
        }

        if (!readying)
        {
            swinging = false;
        }

        SwordObject.Waypoint waypoint = new SwordObject.Waypoint();
        //swordObject.AddWayPoint(transform.position, swordRotateObject.rotation, swinging);
        swordObject.AddWayPoint(transform.localPosition, swordRotateObject.localRotation * transform.localRotation, swinging);

        //rb.mass = swinging ? swingMass : defaultMass;

        previousPosition = transform.localPosition;
    }

    void GetInput()
    {
        readying = Util.GetButton("Ready") || (readyWhenLocked && Util.GetButton("Lock"));
        swordLocked = Util.GetButton("LockSword");
        blocking = Util.GetButton("Block");
        stabbing = Util.GetButton("Stab");
        
        Vector3 previousSwordPosition = swordDamagePoint.position;

        Vector2 rawMouse = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")) * mouseSensitivity;
        if (mouseMaxMagnitude > 0)
        {
            rawMouse = Vector2.ClampMagnitude(rawMouse, mouseMaxMagnitude);
        }
        mouse = Vector2.ClampMagnitude(mouse + rawMouse, 1);


        //TODO better mouse input



        float xAxis = Input.GetAxis("Horizontal Right");
        float yAxis = Input.GetAxis("Vertical Right");

        float x, y;
        if (!Mathf.Approximately(xAxis, 0) || !Mathf.Approximately(yAxis, 0))
        {
            mouse = Vector2.zero;

            x = xAxis;
            y = yAxis;
        }
        else
        {
            x = mouse.x;
            y = mouse.y;
        }

        previousInput = input;
        input = Vector2.ClampMagnitude(new Vector2(x, y), 1);
    }

    void FixedUpdate()
    {
        //swordObject.rb.MoveRotation(swordRotateObject.rotation);
        //swordObject.rb.MovePosition(transform.position);
    }

    private void UpdateSword()
    {
        Vector3 previousSwordPosition = swordDamagePoint.position;

        if (!swordLocked)
        {
            MoveSword(input);
        }
        RotateSword(input);
        //UpdateFPSCamera(input);

        //rb.velocity = swordDamagePoint.position - previousSwordPosition;
    }

    private void MoveSword(Vector2 input)
    {
        Vector2 direction = input.normalized;
        if (input.magnitude == 0)
        {
            direction = Vector2.up;
        }
        
        Vector3 desiredPosition = Vector3.zero;
        Vector3 desiredSwordSlicePosition = Vector3.zero;

        if (blocking)
        {
            desiredPosition.x = direction.x * blockDistanceX;
            desiredPosition.y = direction.y * blockDistanceY;

            desiredSwordSlicePosition = new Vector3(0, 0, -blockOffset);

            DisableTrails();
        }
        else
        {
            if (freeSwing || swinging)
            {
                desiredPosition.x = input.x * maxX;
                desiredPosition.y = input.y * maxY;
            }
            else {
                desiredPosition.x = direction.x * maxX;
                desiredPosition.y = direction.y * maxY;
            }

            desiredPosition.z = (1 - input.magnitude) * maxZ;

            //TODO revisit stabbing
            //if (stabbing)
            //{
            //    desiredPosition += desiredPosition.normalized * stabOffset;
            //}

            desiredSwordSlicePosition = Vector3.zero;


            //calculate swing speed
            //float swingSpeed = Vector3.Distance(swordDamagePoint.position, previousSwordDamagePoint);
            float swingSpeed = Vector3.Distance(swordObject.transform.localPosition, previousSwordPosition);
            //previousSwordDamagePoint = swordDamagePoint.localPosition;

            if (CanStartSwing())
            {
                if (!swinging)
                {
                    swinging = true;

                    EnableTrail();

                    swordInfo.swingAudio.Play(audioSource, swingSpeed, minSwingSpeed, maxSwingSpeed);

                    //swingStartPosition = transform.localPosition;
                    swingStartPosition = (Vector2)swordObject.transform.localPosition;
                }
                //swingLastPosition = transform.localPosition;
                swingLastPosition = (Vector2)swordObject.transform.localPosition;
            }
            else if (swinging && CanContinueSwing())
            {

            }
            else if (swinging && CanStopSwing())
            {
                DisableTrails();

                swinging = false;
            }

            //previousSwordDamagePoint = swordDamagePoint.position;
            previousSwordPosition = swordObject.transform.localPosition;
        }

        //TODO separate into graphics and collider
        //make collider follow this path slowly, and make the graphics one follow the mouse exactly
        //transform.localPosition = Vector3.Lerp(transform.localPosition, desiredPosition, Time.deltaTime * moveSpeed);
        //swordSliceObject.localPosition = Vector3.Lerp(swordSliceObject.localPosition, desiredSwordSlicePosition, Time.deltaTime * moveSpeed);
        transform.localPosition = desiredPosition;
        swordSliceObject.localPosition = Vector3.Lerp(swordSliceObject.localPosition, desiredSwordSlicePosition, Time.deltaTime * moveSpeed);

    }

    private bool CanStartSwing()
    {
        Vector2 previous = (Vector2)previousInput;
        Vector2 current = (Vector2)input;

        Vector2 directionFromPreviousToZero = Vector2.zero - previous;
        Vector2 directionFromPreviousToCurrent = current - previous;

        float swingAngle = Vector2.Angle(directionFromPreviousToZero, directionFromPreviousToCurrent);
        float swingDistance = Vector2.Distance(previous, current);
        float calculatedSwingSpeed = swingDistance / (Time.deltaTime * Time.timeScale);
        
        return swingDistance > minSwingDistance && swingAngle < minAngleToSwing && current.magnitude < previous.magnitude;
    }

    private bool CanContinueSwing()
    {
        Vector2 previous = (Vector2)previousSwordPosition;
        Vector2 current = (Vector2)swordObject.transform.localPosition;

        Vector2 directionFromPreviousToZero = Vector2.zero - previous;
        Vector2 directionFromPreviousToCurrent = current - previous;

        float swingAngle = Vector2.Angle(directionFromPreviousToZero, directionFromPreviousToCurrent);
        float swingDistance = Vector2.Distance(previous, current);
        float calculatedSwingSpeed = swingDistance / (Time.deltaTime * Time.timeScale);
        
        return /*swingDistance > minSwingDistance*/calculatedSwingSpeed > minSwingSpeed && swingAngle < minAngleToSwing && current.magnitude < previous.magnitude;
    }

    private bool CanStopSwing()
    {
        Vector2 previous = (Vector2)previousSwordPosition;
        Vector2 current = (Vector2)swordObject.transform.localPosition;

        Vector2 directionFromStartToLast = swingLastPosition - swingStartPosition;
        //Vector2 directionFromStartToCurrent = (Vector2)transform.localPosition - swingStartPosition;
        Vector2 directionFromStartToCurrent = current - swingStartPosition;

        float swingAngle = Vector2.Angle(directionFromStartToLast, directionFromStartToCurrent);
        //float swingDistance = Vector2.Distance(previousInput, input);
        float calculatedSwingSpeed = Vector2.Distance(previous, current) / (Time.deltaTime * Time.timeScale);

        bool canStop = false;
        if (swingAngle > maxAngleToSwing)
        {
            //Debug.Log("Stopping swing, angle too big - " + swingAngle + " " + maxAngleToSwing);
            canStop = true;
        }
        else if (calculatedSwingSpeed <= swingStopSpeed)
        {
            //Debug.Log("Stopping swing, speed too slow - " + calculatedSwingSpeed + " " + swingStopSpeed);
            canStop = true;
        //} else if (current.magnitude < previous.magnitude)
        } 
        else if (input.magnitude < previousInput.magnitude && !Mathf.Approximately(input.magnitude, previousInput.magnitude))
        {
            //Debug.Log("Stopping swing, direction reversed - " + input.magnitude + " " + previousInput.magnitude);
            canStop = true;
        }

        return /*input.magnitude > 0 && *//*swingDistance < minSwingDistance || */canStop;
    }

    private void RotateSword(Vector3 input)
    {
        Vector3 desiredSwordRotation = Vector3.zero;
        Vector3 desiredSwordLookAt = Vector3.zero;
        Vector3 desiredSwordSliceRotation = Vector3.zero;

        Vector3 angleVector = transform.localPosition;
        angleVector.x = Util.ConvertRange(angleVector.x, -maxX, maxX, -swingAngleScale, swingAngleScale);
        angleVector.y = Util.ConvertRange(angleVector.y, -maxY, maxY, -swingAngleScale, swingAngleScale);
        angleVector.z = Util.ConvertRange(angleVector.z, -maxZ, maxZ, -swingAngleScale, swingAngleScale);

        float angle = Util.GetAngle(angleVector);
        if (blocking)
        {
            desiredSwordRotation.y = -90;
            desiredSwordRotation.x = -angle;

            desiredSwordSliceRotation.z = 180;

            swordRotateObject.localRotation = Quaternion.Lerp(swordRotateObject.localRotation, Quaternion.Euler(desiredSwordRotation), Time.deltaTime * rotateSpeed);

            //Quaternion newRotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(transform.TransformDirection(desiredSwordRotation)), Time.fixedDeltaTime * rotateSpeed);
            //rb.MoveRotation(newRotation);

            swordObject.modelHolder.transform.rotation = Quaternion.Lerp(swordObject.modelHolder.rotation, swordSliceObject.rotation, Time.deltaTime * rotateSpeed);
        }
        else
        {
            //desiredSwordSliceRotation.z = -angle;
            //swordObject.modelHolder.transform.rotation = Quaternion.Lerp(swordObject.modelHolder.rotation, swordSliceObject.rotation, Time.deltaTime * rotateSpeed);

            //Quaternion modelRotation = Quaternion.identity * Quaternion.Euler(0, 0, -angle);
            //swordObject.modelHolder.transform.localRotation = Quaternion.Lerp(swordObject.modelHolder.localRotation, modelRotation, Time.deltaTime * rotateSpeed);

            //if (swordObject.rb.velocity.magnitude > 0.1f)
            //{
            //    Quaternion swordSliceRotation = Quaternion.LookRotation(swordObject.modelHolder.forward, -swordObject.rb.velocity);
            //    swordObject.modelHolder.transform.rotation = Quaternion.Lerp(swordObject.modelHolder.rotation, swordSliceRotation, Time.deltaTime * rotateSpeed);
            //}

            swordObject.modelHolder.transform.localRotation = Quaternion.Lerp(swordObject.modelHolder.localRotation, Quaternion.identity, Time.deltaTime * rotateSpeed);

            Vector3 desiredSwordLookAtDir = transform.right * angleVector.x + transform.up * angleVector.y + transform.forward * angleVector.z;
            Vector3 desiredSwordLookAtPos = transform.position + desiredSwordLookAtDir;

            //swordRotateObject.localRotation = Quaternion.Lerp(swordRotateObject.localRotation, Quaternion.LookRotation(desiredSwordLookAt, Vector3.up), Time.deltaTime * rotateSpeed);
            //Vector3 up = desiredSwordLookAt.z > 0 ? transform.up : -transform.up;

            //swordRotateObject.rotation = Quaternion.Lerp(swordRotateObject.rotation, Quaternion.LookRotation(desiredSwordLookAtDir, swordRotateObject.up), Time.deltaTime * rotateSpeed);
            swordRotateObject.rotation = Quaternion.LookRotation(desiredSwordLookAtDir, swordRotateObject.up);



            Debug.DrawLine(transform.position, desiredSwordLookAtPos);
            Debug.DrawLine(desiredSwordLookAtPos, transform.parent.parent.position);
            Debug.DrawLine(transform.parent.parent.position, transform.position);
            Plane anglePlane = new Plane(transform.position, desiredSwordLookAtPos, transform.parent.parent.position);
            Vector3 planeNormal = anglePlane.normal;
            //if (desiredSwordLookAtPos.y < transform.position.y)
            //{
            //    planeNormal *= -1;
            //}
            Debug.DrawRay(transform.position, planeNormal);
            Vector3 up = Vector3.ProjectOnPlane(swordRotateObject.up, planeNormal);
            swordObject.modelHolder.transform.rotation = Quaternion.LookRotation(desiredSwordLookAtDir, up);


            //desiredSwordLookAt = new Vector3(angleVector.y * -90, angleVector.x * 90);
            //swordRotateObject.localEulerAngles = /*Vector3.Lerp(swordRotateObject.localEulerAngles, */desiredSwordLookAt/*, Time.deltaTime * rotateSpeed)*/;


            //Transform parent = transform.parent;
            //desiredSwordLookAt = parent.up * desiredSwordLookAt.y + parent.right * desiredSwordLookAt.x + parent.forward * desiredSwordLookAt.z;
            //Quaternion newRotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(desiredSwordLookAt, Vector3.up), Time.fixedDeltaTime * rotateSpeed);
            //rb.MoveRotation(newRotation);
        }

        transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.identity, Time.deltaTime * rotateSpeed);
        swordSliceObject.localRotation = /*Quaternion.Euler(desiredSwordSliceRotation);*/ Quaternion.Lerp(swordSliceObject.localRotation, Quaternion.Euler(desiredSwordSliceRotation), Time.deltaTime * rotateSpeed);
        //swordSliceObject.localRotation = Quaternion.Lerp(swordSliceObject.localRotation, Quaternion.Euler(desiredSwordSliceRotation), Time.fixedDeltaTime * rotateSpeed);



    }

    private void ResetSword()
    {
        mouse = Vector2.zero;
        input = Vector3.zero;
        previousInput = Vector3.zero;

        transform.localPosition = Vector3.Lerp(transform.localPosition, defaultPosition, Time.deltaTime * moveSpeed);
        transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(defaultRotation), Time.deltaTime * rotateSpeed);

        swordRotateObject.localPosition = Vector3.Lerp(swordRotateObject.localPosition, Vector3.zero, Time.deltaTime * moveSpeed);
        swordRotateObject.localRotation = Quaternion.Lerp(swordRotateObject.localRotation, Quaternion.Euler(Vector3.zero), Time.deltaTime * rotateSpeed);

        swordSliceObject.localPosition = Vector3.Lerp(swordSliceObject.localPosition, Vector3.zero, Time.deltaTime * moveSpeed);
        swordSliceObject.localRotation = Quaternion.Lerp(swordSliceObject.localRotation, Quaternion.Euler(Vector3.zero), Time.deltaTime * rotateSpeed);

        swordObject.modelHolder.transform.localRotation = Quaternion.Lerp(swordObject.modelHolder.transform.localRotation, Quaternion.identity, Time.deltaTime * rotateSpeed);

        swinging = false;
        DisableTrails();
    }

    private void DisableTrails()
    {
        StopDisableTrailCoRoutines();

        for(int i = 0; i < trails.Length; i++)
        {
            disableTrails[i] = StartCoroutine(DisableTrailEnum(trails[i], i));
        }
    }

    private IEnumerator DisableTrailEnum(TrailRenderer trail, int index)
    {
        if (trail != null)
        {
            float rate = trail.time / 15f;
            while (trail.time > 0)
            {
                trail.time -= rate;
                if (trail.time < 0)
                {
                    trail.time = 0;
                }
                yield return 0;
            }
        }

        disableTrails[index] = null;
    }

    void StopDisableTrailCoRoutines()
    {
        if (disableTrails != null)
        {
            foreach (Coroutine disableTrail in disableTrails)
            {
                if (disableTrail != null)
                {
                    StopCoroutine(disableTrail);
                }
            }
        }
        disableTrails = new Coroutine[trails.Length];
    }

    public void EnableTrail()
    {
        StopDisableTrailCoRoutines();

        foreach (TrailRenderer trail in trails)
        {
            trail.enabled = true;
            trail.time = trailTime;
        }
    }
}
