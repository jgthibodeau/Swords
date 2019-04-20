using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sword : MonoBehaviour
{
    public float moveSpeed, rotateSpeed;
    public float minAttackAngle, maxAttackAngle;
    public float maxX, maxY;
    public float blockOffset, blockDistanceX, blockDistanceY;
    public float mouseSensitivity;

    public Transform swordRotateObject;
    public Transform swordSliceObject;
    public UnityStandardAssets.Characters.FirstPerson.FirstPersonController fpsController;
    public Transform fpsCharacter;

    public float trailTime = 0.1f;
    public TrailRenderer trail;
    private Coroutine disableTrail;

    private Animator animator;

    private Vector3 defaultPosition;
    private Vector3 defaultRotation;

    private float mousePosX = 0;
    private float mousePosY = 0;

    public float lockSpeed, lockRotation;
    private Vector2 fpsMouseSenitivity;

    private bool swinging = false;
    public float minSwingSpeed, maxSwingSpeed, swingStopSpeed;
    public AudioClip[] swingClips;
    private AudioSource audioSource;
    public float minSwingPitch, maxSwingPitch, minSwingVolume, maxSwingVolume;

    private bool locked;
    private Quaternion fpsControllerRotation;
    private Quaternion fpsCharacterRotation;

    private bool locking, blocking;

    private Vector2 previousPosition;

    // Start is called before the first frame update
    void Start()
    {
        defaultPosition = transform.localPosition;
        defaultRotation = transform.localEulerAngles;
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        fpsMouseSenitivity = new Vector2(fpsController.m_MouseLook.XSensitivity, fpsController.m_MouseLook.YSensitivity);
    }

    // Update is called once per frame
    void Update()
    {
        locking = Input.GetButton("Lock");
        blocking = Input.GetButton("Block");

        if (locking || blocking)
        {
            //fpsController.m_MouseLook.XSensitivity = Mathf.Lerp(fpsController.m_MouseLook.XSensitivity, 0, Time.deltaTime * lockSpeed);
            //fpsController.m_MouseLook.YSensitivity = Mathf.Lerp(fpsController.m_MouseLook.YSensitivity, 0, Time.deltaTime * lockSpeed);

            if (!locked)
            {
                locked = true;
                fpsControllerRotation = fpsController.transform.localRotation;
                fpsCharacterRotation = fpsCharacter.localRotation;
            }

            fpsController.m_MouseLook.XSensitivity = 0;
            fpsController.m_MouseLook.YSensitivity = 0;
            
            UpdateSword();
        } else
        {
            //fpsController.m_MouseLook.XSensitivity = Mathf.Lerp(fpsController.m_MouseLook.XSensitivity, fpsMouseSenitivity.x, Time.deltaTime * lockSpeed);
            //fpsController.m_MouseLook.YSensitivity = Mathf.Lerp(fpsController.m_MouseLook.YSensitivity, fpsMouseSenitivity.y, Time.deltaTime * lockSpeed);
            fpsController.m_MouseLook.XSensitivity = fpsMouseSenitivity.x;
            fpsController.m_MouseLook.YSensitivity = fpsMouseSenitivity.y;

            if (locked)
            {
                locked = false;
            }

            ResetSword();
        }
    }

    private void UpdateSword()
    {
        mousePosX = Mathf.Clamp(mousePosX + Input.GetAxis("Mouse X") * mouseSensitivity, -1, 1);
        mousePosY = Mathf.Clamp(mousePosY + Input.GetAxis("Mouse Y") * mouseSensitivity, -1, 1);

        float xAxis = Input.GetAxis("Horizontal Right");
        float yAxis = Input.GetAxis("Vertical Right");

        float x, y;
        if (!Mathf.Approximately(xAxis, 0) || !Mathf.Approximately(yAxis, 0))
        {
            mousePosX = 0;
            mousePosY = 0;

            x = xAxis;
            y = yAxis;
        }
        else
        {
            x = mousePosX;
            y = mousePosY;
        }

        Vector2 input = Vector2.ClampMagnitude(new Vector2(x, y), 1);
        Vector2 direction = input.normalized;
        if (input.magnitude == 0)
        {
            direction = Vector2.up;
        }


        //move to direction
        Vector3 desiredPosition = Vector3.zero;
        Vector3 desiredSwordPosition = Vector3.zero;
        Vector3 desiredSwordSlicePosition = Vector3.zero;

        if (blocking)
        {
            desiredPosition.x = direction.x * blockDistanceX;
            desiredPosition.y = direction.y * blockDistanceY;

            desiredSwordSlicePosition = new Vector3(0, -blockOffset, 0);

            DisableTrail();
        }
        else
        {
            desiredPosition.x = input.x * maxX;
            desiredPosition.y = input.y * maxY;
            
            desiredSwordSlicePosition = Vector3.zero;


            //calculate swing speed
            float swingSpeed = Vector2.Distance(previousPosition, input);
            Debug.Log(swingSpeed);
            if (swingSpeed < swingStopSpeed)
            {
                DisableTrail();

                swinging = false;
            }
            else if (swingSpeed > minSwingSpeed) // TODO and check swing direction is towards/away from center
            {
                if (!swinging)
                {
                    swinging = true;

                    EnableTrail();

                    audioSource.volume = ConvertRange(swingSpeed, minSwingSpeed, maxSwingSpeed, minSwingVolume, maxSwingVolume);
                    audioSource.pitch = ConvertRange(swingSpeed, minSwingSpeed, maxSwingSpeed, minSwingPitch, maxSwingPitch);//Random.Range(minSwingPitch, maxSwingPitch);
                    AudioClip swingClip = swingClips[Random.Range(0, swingClips.Length - 1)];
                    audioSource.PlayOneShot(swingClip);
                }
            }
        }
        previousPosition = input;

        transform.localPosition = Vector3.Lerp(transform.localPosition, desiredPosition, Time.deltaTime * moveSpeed);
        swordRotateObject.localPosition = Vector3.Lerp(swordRotateObject.localPosition, desiredSwordPosition, Time.deltaTime * moveSpeed);
        swordSliceObject.localPosition = Vector3.Lerp(swordSliceObject.localPosition, desiredSwordSlicePosition, Time.deltaTime * moveSpeed);



        //rotate
        Vector3 desiredSwordRotation = Vector3.zero;
        Vector3 desiredSwordSliceRotation = Vector3.zero;

        float angle = GetAngle(Vector2.up, transform.localPosition);
        if (blocking)
        {
            desiredSwordRotation.z = 90 - angle;
            
            desiredSwordSliceRotation.x = 0;
        }
        else
        {
            //if (!swinging)
            //{
            //    desiredSwordRotation.z = -angle;

            //    desiredSwordSliceRotation.x = ConvertRange(Mathf.Abs(input.magnitude), 0, 1, minAttackAngle, maxAttackAngle);
            //} else
            //{
            //    desiredSwordRotation.z = swordRotateObject.localEulerAngles.z;

            //    desiredSwordSliceRotation.x = swordSliceObject.localEulerAngles.x;
            //}
            desiredSwordRotation.x = ConvertRange(transform.localPosition.y, -maxY, maxY, 180, 0);
            desiredSwordRotation.z = ConvertRange(transform.localPosition.x, -maxX, maxX, 90, -90);

            desiredSwordSliceRotation.y = -angle;
        }

        //transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(Vector3.zero), Time.deltaTime * rotateSpeed);
        //swordRotateObject.localRotation = Quaternion.Lerp(swordRotateObject.localRotation, Quaternion.Euler(desiredSwordRotation), Time.deltaTime * rotateSpeed);
        //swordSliceObject.localRotation = Quaternion.Lerp(swordSliceObject.localRotation, Quaternion.Euler(desiredSwordSliceRotation), Time.deltaTime * rotateSpeed);

        transform.localRotation = Quaternion.Euler(Vector3.zero);
        swordRotateObject.localRotation = Quaternion.Euler(desiredSwordRotation);
        swordSliceObject.localRotation = Quaternion.Euler(desiredSwordSliceRotation);






        Quaternion newFpsControllerRotation = fpsControllerRotation * Quaternion.Euler(0, input.x * lockRotation, 0);
        Quaternion newFpsCharacterRotation = fpsCharacterRotation * Quaternion.Euler(-input.y * lockRotation, 0, 0);

        fpsController.transform.localRotation = Quaternion.Lerp(fpsController.transform.localRotation, newFpsControllerRotation, Time.deltaTime * lockSpeed);
        fpsCharacter.localRotation = Quaternion.Lerp(fpsCharacter.localRotation, newFpsCharacterRotation, Time.deltaTime * lockSpeed);
    }

    private void ResetSword()
    {
        mousePosX = 0;
        mousePosY = 0;
        previousPosition = Vector3.zero;

        transform.localPosition = Vector3.Lerp(transform.localPosition, defaultPosition, Time.deltaTime * moveSpeed);
        transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(defaultRotation), Time.deltaTime * rotateSpeed);

        swordRotateObject.localPosition = Vector3.Lerp(swordRotateObject.localPosition, Vector3.zero, Time.deltaTime * moveSpeed);
        swordRotateObject.localRotation = Quaternion.Lerp(swordRotateObject.localRotation, Quaternion.Euler(Vector3.zero), Time.deltaTime * rotateSpeed);

        swordSliceObject.localPosition = Vector3.Lerp(swordSliceObject.localPosition, Vector3.zero, Time.deltaTime * moveSpeed);
        swordSliceObject.localRotation = Quaternion.Lerp(swordSliceObject.localRotation, Quaternion.Euler(Vector3.zero), Time.deltaTime * rotateSpeed);

        DisableTrail();
    }

    private void DisableTrail()
    {
        if (disableTrail != null)
        {
            StopCoroutine(disableTrail);
        }
        disableTrail = StartCoroutine(DisableTrailEnum());
    }

    private IEnumerator DisableTrailEnum()
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

        disableTrail = null;
    }

    public void EnableTrail()
    {
        if (disableTrail != null)
        {
            StopCoroutine(disableTrail);
        }

        if (trail != null)
        {
            trail.enabled = true;
            trail.time = trailTime;
        }
    }

    public float GetAngle(Vector2 fromVector2, Vector2 toVector2)
    {
        float angle = Vector2.Angle(fromVector2, toVector2);
        Vector3 cross = Vector3.Cross(fromVector2, toVector2);

        if (cross.z > 0)
        {
            angle = 360 - angle;
        }

        return angle;
    }

    public float ConvertRange(float oldValue, float oldMin, float oldMax, float newMin, float newMax)
    {
        return (((oldValue - oldMin) * (newMax - newMin)) / (oldMax - oldMin)) + newMin;
    }
}
