using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LockOnController : MonoBehaviour
{
    [System.Serializable]
    public class TargetInfo {
        public float minDistance;
        public float maxDistance;
        public float minRadius, maxRadius;
        [Range(0, 10)]
        public int radiusSteps;

        public Transform FindTarget(Vector3 position, Vector3 forward, LayerMask layers)
        {
            position += forward.normalized * minDistance;

            Transform target = null;
            if (radiusSteps == 0)
            {
                target = FindTarget(position, forward, layers, minRadius);
            }
            else
            {
                float radiusStepSize = (maxRadius - minRadius) / radiusSteps;
                for (int i = 0; i <= radiusSteps; i++)
                {
                    target = FindTarget(position, forward, layers, minRadius + radiusStepSize * i);
                    if (target != null)
                    {
                        break;
                    }
                }
            }

            return target;
        }

        private Transform FindTarget(Vector3 position, Vector3 forward, LayerMask layers, float radius)
        {
            if (Physics.SphereCast(position, radius, forward, out RaycastHit hit, (maxDistance - minDistance), layers))
            {
                Rigidbody hitRb = hit.transform.GetComponentInParent<Rigidbody>();
                if (hitRb != null)
                {
                    return hitRb.transform;
                }
                else
                {
                    return hit.transform;
                }
            }

            return null;
        }
    }


    public Sword sword;

    public UnityStandardAssets.Characters.FirstPerson.FirstPersonController fpsController;
    public Transform fpsCamera;

    public LayerMask lockableLayers;

    public float lockSpeed, lockRotation;
    private Vector2 fpsMouseSenitivity;

    public bool lockOnEnabled;
    public bool autoReTarget;
    public Transform lockTarget;

    public TargetInfo lockOnInfo;

    public bool readying, blocking, locking, swording;

    private Quaternion baseFpsControllerRotation, baseFpsCameraRotation;

    // Start is called before the first frame update
    void Start()
    {
        fpsMouseSenitivity = new Vector2(fpsController.m_MouseLook.XSensitivity, fpsController.m_MouseLook.YSensitivity);
    }

    // Update is called once per frame
    void Update()
    {
        GetInput();

        if (!locking)
        {
            AqcuireLockTarget();
        } else
        {
            UpdateLockTarget();
        }
        
        if (Locked())
        {
            CalculateCameraForLock();
        } else if (!swording)
        {
            baseFpsControllerRotation = fpsController.transform.localRotation;
            baseFpsCameraRotation = fpsCamera.localRotation;
        }

        Quaternion fpsControllerRotation = baseFpsControllerRotation;
        Quaternion fpsCameraRotation = baseFpsCameraRotation;

        if (swording)
        {
            fpsControllerRotation *= Quaternion.Euler(0, sword.input.x * lockRotation, 0);
            fpsCameraRotation *= Quaternion.Euler(-sword.input.y * lockRotation, 0, 0);
        }

        if (Locked() || swording)
        {
            fpsController.m_MouseLook.XSensitivity = 0;
            fpsController.m_MouseLook.YSensitivity = 0;
            fpsController.m_ManuallyRotate = true;

            fpsController.transform.localRotation = Quaternion.Slerp(fpsController.transform.localRotation, fpsControllerRotation, Time.deltaTime * lockSpeed);
            fpsCamera.localRotation = Quaternion.Slerp(fpsCamera.localRotation, fpsCameraRotation, Time.deltaTime * lockSpeed);

            fpsController.m_MouseLook.m_CharacterTargetRot = fpsController.transform.localRotation;
            fpsController.m_MouseLook.m_CameraTargetRot = fpsCamera.localRotation;
        } else
        {
            fpsController.m_MouseLook.XSensitivity = fpsMouseSenitivity.x;
            fpsController.m_MouseLook.YSensitivity = fpsMouseSenitivity.y;
            fpsController.m_ManuallyRotate = false;
        }
    }

    bool Locked()
    {
        return locking && lockTarget != null;
    }
    
    void GetInput()
    {
        readying = Util.GetButton("Ready");
        blocking = Util.GetButton("Block");
        locking = Util.GetButton("Lock");

        swording = readying || blocking || locking;
    }

    private void AqcuireLockTarget()
    {
        if (lockOnEnabled)
        {
            lockTarget = lockOnInfo.FindTarget(fpsCamera.position, fpsCamera.forward, lockableLayers);
        } else
        {
            lockTarget = null;
        }
    }

    private void UpdateLockTarget()
    {
        if (lockTarget != null && !lockTarget.gameObject.activeSelf)
        {
            lockTarget = null;
        }

        if (lockTarget == null && autoReTarget)
        {
            AqcuireLockTarget();
        }
    }

    private void CalculateCameraForLock()
    {
        //reset base fpsControllerRotation and fpsCameraRotation to point at the target
        Vector3 targetPosition = lockTarget.position;

        Vector3 direction = targetPosition - fpsCamera.transform.position;
        Quaternion lookAt = Quaternion.LookRotation(direction, Vector3.up);

        baseFpsControllerRotation = Quaternion.identity * Quaternion.Euler(0, lookAt.eulerAngles.y, 0);
        baseFpsCameraRotation = Quaternion.identity * Quaternion.Euler(lookAt.eulerAngles.x, 0, 0);
    }
}
