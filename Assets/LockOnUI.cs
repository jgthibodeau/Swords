using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LockOnUI : MonoBehaviour
{
    private LockOnController lockOnController;
    private GrapplingHook grapplingHook;
    private Camera camera;

    public RectTransform lockableIcon, lockedIcon, grapplableIcon, grapplingIcon;

    void Start()
    {
        camera = GetComponentInChildren<Camera>();
        lockOnController = GetComponent<LockOnController>();
        grapplingHook = GetComponent<GrapplingHook>();
    }

    void LateUpdate()
    {
        DrawLockUI();
    }

    void DrawLockUI()
    {
        lockedIcon.gameObject.SetActive(false);
        lockableIcon.gameObject.SetActive(false);
        grapplableIcon.gameObject.SetActive(false);
        grapplingIcon.gameObject.SetActive(false);

        if (lockOnController.lockTarget != null)
        {
            RectTransform lockIcon;

            if (lockOnController.locking)
            {
                lockIcon = lockedIcon;
            }
            else
            {
                lockIcon = lockableIcon;
            }

            lockIcon.gameObject.SetActive(true);
            Vector3 position = camera.WorldToScreenPoint(lockOnController.lockTarget.position);
            lockIcon.position = position;
        }

        if (grapplingHook.availableTarget != null)
        {
            RectTransform grappleIcon;

            if (grapplingHook.isGrappling && grapplingHook.availableTarget == grapplingHook.currentTarget)
            {
                grappleIcon = grapplingIcon;
            }
            else
            {
                grappleIcon = grapplableIcon;
            }

            grappleIcon.gameObject.SetActive(true);
            Vector3 position = camera.WorldToScreenPoint(grapplingHook.availableTarget.position);
            grappleIcon.position = position;
        }
    }
}
