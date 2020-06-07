using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallRun : MonoBehaviour
{
    public PlayerController playerController;

    public UnityStandardAssets.Characters.FirstPerson.FirstPersonController fpsController;
    public Transform fpsCamera;
    public LayerMask wallRunLayers;

    public float wallRunGravity, minAngleToRun, speedMultiplier, wallRunJump, wallRunJumpOff, wallDistance, wallStickForce;

    public float wallAngle, wallJumpOffAngle, wallRotateSpeed;

    public bool wallRunning = false;
    private float originalGravity;

    private Vector3 wallRunDirection;
    private Vector3 wallDirection;
    private Vector3 desiredUp;
    private Vector3 wallJumpOffDirection;

    void Start()
    {
        fpsController = GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>();

        originalGravity = fpsController.m_GravityMultiplier;
    }

    void Update()
    {
        if (wallRunning)
        {
            ContinueWallRun();
        }
        else
        {
            desiredUp = Vector3.up;
        }

        if (transform.up != desiredUp)
        {
            Vector3 newUp = Vector3.MoveTowards(transform.up, desiredUp, Time.deltaTime * wallRotateSpeed);
            Vector3 forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up);
            transform.localRotation = Quaternion.LookRotation(forward, newUp);
            fpsController.m_MouseLook.m_CharacterTargetRot = fpsController.transform.localRotation;
        }
        else
        {
            fpsController.m_ManuallyRotate = false;
        }
    }

    bool OnWall()
    {
        Debug.DrawRay(transform.position, wallDirection * wallDistance);
        bool nextToWall = Physics.Raycast(transform.position, wallDirection, wallDistance, wallRunLayers);
        return !fpsController.m_CharacterController.isGrounded && nextToWall;
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (!wallRunning && !fpsController.m_CharacterController.isGrounded && Util.LayerInMask(wallRunLayers, hit.gameObject.layer)) {
            WallRunWall wall = hit.gameObject.GetComponentInParent<WallRunWall>();

            if (wall != null)
            {
                InitiateWallRun(hit.moveDirection, hit.normal, wall.transform);
            }
        }
    }

    void InitiateWallRun(Vector3 direction, Vector3 normal, Transform wall)
    {
        playerController.StateStart(PlayerController.PlayerState.WALLRUNNING);

        wallRunning = true;

        //disable normal movement
        fpsController.EnableWalk(false);
        fpsController.m_Jumping = false;

        wallDirection = -normal.normalized;

        wallRunDirection = Vector3.ProjectOnPlane(direction, normal);
        wallRunDirection.y = 0;

        if (wallRunDirection.magnitude > minAngleToRun)
        {
            wallRunDirection = wallRunDirection.normalized * speedMultiplier;
        }
        wallRunDirection *= fpsController.m_CurrentSpeed;

        wallRunDirection.y = Mathf.Max(fpsController.m_MoveDir.y, wallRunJump);

        wallRunDirection += wallDirection * wallStickForce;

        fpsController.m_MoveDir = wallRunDirection;

        desiredUp = Vector3.MoveTowards(Vector3.up, normal, wallAngle * Mathf.Deg2Rad);
        wallJumpOffDirection = Vector3.MoveTowards(Vector3.up, normal, wallJumpOffAngle * Mathf.Deg2Rad);
    }

    void ContinueWallRun()
    {
        if (OnWall())
        {
            fpsController.EnableWalk(false);
            fpsController.m_GravityMultiplier = wallRunGravity;

            //TODO if jump pressed, jump off the wall
            if (Input.GetButtonDown("Jump"))
            {
                fpsController.m_MoveDir += wallJumpOffDirection * wallRunJumpOff;
            }
        }
        else
        {
            Debug.Log("Grounded, stopping wall run");
            playerController.StateEnd(PlayerController.PlayerState.WALLRUNNING);
        }
    }

    public void Halt()
    {
        wallRunning = false;
        fpsController.EnableCollisions(true);
        fpsController.EnableWalk(true);
        fpsController.EnableGravity(true);
        fpsController.m_GravityMultiplier = originalGravity;
    }
}
