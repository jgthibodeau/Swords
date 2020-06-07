using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class Throw : MonoBehaviour
{
    public UnityStandardAssets.Characters.FirstPerson.FirstPersonController fpsController;

    public int maxThrows;
    public int throwsLeft;
    public float throwRegainRate;
    public float throwRegainDelay;
    public float currentThrowDelay;

    public float minForce, maxForce, throwForceGain;
    private float throwForce;
    public float rotateSpeed;

    public GameObject throwable;
    public Transform throwPoint;

    public LineRenderer lineRenderer;
    public bool drawLine;
    public int resolution;

    float g;

    void Start()
    {
        fpsController = GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>();
        g = Physics.gravity.magnitude;

        throwForce = minForce;
        throwsLeft = maxThrows;
    }

    // Update is called once per frame
    void Update()
    {
        if (CrossPlatformInputManager.GetButton("Throw"))
        {
            throwForce = Mathf.Min(throwForce + throwForceGain * Time.deltaTime, maxForce);
        } else
        {
            RegainThrow();
        }

        if (CrossPlatformInputManager.GetButtonUp("Throw"))
        {
            if (CanThrow())
            {
                ThrowObject();
                throwsLeft--;
                currentThrowDelay = throwRegainDelay;
            }
            
            throwForce = minForce;
        }
    }

    void LateUpdate()
    {
        if (CrossPlatformInputManager.GetButton("Throw") && drawLine)
        {
            lineRenderer.enabled = true;
            RenderArc();
        } else
        {
            lineRenderer.enabled = false;
        }
    }

    bool CanThrow()
    {
        return throwsLeft > 0;
    }

    void RegainThrow()
    {
        if (CrossPlatformInputManager.GetButtonDown("Reload"))
        {
            if (throwsLeft < maxThrows)
            {
                throwsLeft++;
            }
        }
        if (currentThrowDelay <= 0)
        {
            if (throwsLeft < maxThrows)
            {
                currentThrowDelay = throwRegainRate;
                throwsLeft++;
            }
        }
        else
        {
            currentThrowDelay -= Time.deltaTime;
        }
    }

    void ThrowObject()
    {
        //throw object
        GameObject thrown = GameObject.Instantiate(throwable);
        thrown.transform.position = throwPoint.position;
        thrown.transform.rotation = throwPoint.rotation;

        Rigidbody rb = thrown.GetComponent<Rigidbody>();

        //rb.velocity = fpsController.m_CharacterController.velocity;
        rb.velocity = thrown.transform.forward * throwForce;
        rb.AddTorque(thrown.transform.up * rotateSpeed, ForceMode.VelocityChange);
    }

    void RenderArc()
    {
        lineRenderer.positionCount = resolution + 1;

        lineRenderer.SetPositions(CalculateArcArray());
    }

    Vector3[] CalculateArcArray()
    {
        Vector3[] arcArray = new Vector3[resolution + 1];

        float angle = Vector3.Angle(throwPoint.forward, transform.forward) * Mathf.Deg2Rad;
        if (throwPoint.forward.y < 0)
        {
            angle *= -1;
        }

        //float maxDistance = Mathf.Sin(angle * 2) * throwForce * throwForce / g;
        float vSinAngle = throwForce * Mathf.Sin(angle);
        float maxDistance = (throwForce * Mathf.Cos(angle) / g) * (vSinAngle + Mathf.Sqrt(Mathf.Pow(vSinAngle, 2) + 2 * g * throwPoint.position.y));

        for (int i = 0; i <= resolution; i++)
        {
            float t = (float)i / (float)resolution;
            arcArray[i] = CalculateArcPoint(t, maxDistance, throwForce, angle);
        }

        return arcArray;
    }

    Vector3 CalculateArcPoint(float t, float maxDistance, float velocity, float angle)
    {
        float x = t * maxDistance;
        float y = throwPoint.position.y + x * Mathf.Tan(angle) - ((g * x * x) / (2 * Mathf.Pow(velocity * Mathf.Cos(angle), 2)));

        Vector3 point = throwPoint.position;
        point += transform.forward * x;
        point.y = y;

        return point;
    }
}
