using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using UnityStandardAssets.Utility;
using Random = UnityEngine.Random;

namespace UnityStandardAssets.Characters.FirstPerson
{
    [RequireComponent(typeof (CharacterController))]
    [RequireComponent(typeof (AudioSource))]
    public class FirstPersonController : MonoBehaviour
    {
        public enum WalkState { WALK, RUN, SLIDE, CROUCH, DASH }

        [System.Serializable]
        public class MovementInfo
        {
            public float acceleration;
            public float speed;
            [Range(0f, 1f)]
            public float stepLength;
        }

        public MovementInfo walkInfo, runInfo, slideInfo, dashInfo;
        
        [SerializeField] private WalkState m_WalkState = WalkState.WALK;
        [SerializeField] public bool m_CanWalk = true;
        [SerializeField] public float m_GroundAcceleration;
        //[SerializeField] public float m_RunAcceleration;
        //[SerializeField] public float m_SlideAcceleration;
        //[SerializeField] public float m_DashAcceleration;
        [SerializeField] public float m_AirAcceleration;
        [SerializeField] public float m_JumpAirAcceleration;
        [SerializeField] private float m_NormalHeight;
        [SerializeField] private float m_CrouchHeight;
        [SerializeField] private float m_HeightChangeSpeed;
        [SerializeField] public float m_CurrentSpeed;
        //[SerializeField] private float m_WalkSpeed;
        //[SerializeField] private float m_RunSpeed;
        //[SerializeField] private float m_SlideSpeed;
        [SerializeField] private float m_SlideTime;
        [SerializeField] private float m_SlideElapsedTime;
        [SerializeField] private float m_RunDoubleTapTime;
        //[SerializeField] [Range(0f, 1f)] private float m_WalkStepLength = 1f;
        //[SerializeField] [Range(0f, 1f)] private float m_RunStepLength = 0.7f;
        //[SerializeField] [Range(0f, 1f)] private float m_SprintStepLength = 0.5f;
        //[SerializeField] [Range(0f, 1f)] private float m_DashStepLength = 0.7f;
        [SerializeField] private float m_JumpSpeed;
        [SerializeField] private float m_StickToGroundForce;
        [SerializeField] public bool m_EnableGravity;
        [SerializeField] public float m_GravityMultiplier;
        [SerializeField] public MouseLook m_MouseLook;
        [SerializeField] private bool m_UseFovKick;
        [SerializeField] private FOVKick m_FovKick = new FOVKick();
        [SerializeField] private bool m_UseHeadBob;
        [SerializeField] private CurveControlledBob m_HeadBob = new CurveControlledBob();
        [SerializeField] private LerpControlledBob m_JumpBob = new LerpControlledBob();
        [SerializeField] private float m_StepInterval;
        [SerializeField] private AudioClip[] m_FootstepSounds;    // an array of footstep sounds that will be randomly selected from.
        [SerializeField] private AudioClip m_JumpSound;           // the sound played when character leaves the ground.
        [SerializeField] private AudioClip m_LandSound;           // the sound played when character touches back on ground.

        [SerializeField] private bool m_collisionsEnabled;
        public Transform m_Camera;
        public Camera m_TrueCamera;
        public bool m_ManuallyRotate = false;
        private bool m_Jump;
        private bool m_Slide, m_SlideHeld, m_Sliding;
        private float m_YRotation;
        private Vector2 m_Input;
        [SerializeField] public Vector3 m_MoveDir = Vector3.zero;
        public CharacterController m_CharacterController;
        private CollisionFlags m_CollisionFlags;
        private bool m_PreviouslyGrounded;
        private Vector3 m_OriginalCameraPosition;
        private float m_StepCycle;
        private float m_NextStep;
        public bool m_Jumping;
        private bool m_JumpBoosting;


        private AudioSource m_AudioSource;

        private bool[] ignoreCollisionLayers = new bool[32];

        // Use this for initialization
        private void Start()
        {
            m_CharacterController = GetComponent<CharacterController>();
            m_TrueCamera = Camera.main;
            m_OriginalCameraPosition = m_Camera.transform.localPosition;
            m_FovKick.Setup(m_TrueCamera);
            m_HeadBob.Setup(m_TrueCamera, m_StepInterval);
            m_StepCycle = 0f;
            m_NextStep = m_StepCycle/2f;
            m_Jumping = false;
            m_AudioSource = GetComponent<AudioSource>();
			m_MouseLook.Init(transform , m_Camera.transform);

            for (int layer = 0; layer < 32; layer++)
            {
                ignoreCollisionLayers[layer] = Physics.GetIgnoreLayerCollision(gameObject.layer, layer);
            }
        }


        // Update is called once per frame
        private void Update()
        {
            RotateView();

            m_Slide = CrossPlatformInputManager.GetButtonDown("Crouch");
            m_SlideHeld = CrossPlatformInputManager.GetButton("Crouch");

            // the jump state needs to read here to make sure it is not missed
            if (!m_Jump)
            {
                m_Jump = CrossPlatformInputManager.GetButtonDown("Jump");
            }


            if (!m_PreviouslyGrounded && m_CharacterController.isGrounded)
            {
                StartCoroutine(m_JumpBob.DoBobCycle());
                PlayLandingSound();
                m_MoveDir.y = 0f;
                m_Jumping = false;
                
                m_Jump = CrossPlatformInputManager.GetButton("Jump");
            }
            if (!m_CharacterController.isGrounded && !m_Jumping && m_PreviouslyGrounded)
            {
                m_MoveDir.y = 0f;
            }

            m_PreviouslyGrounded = m_CharacterController.isGrounded;

            CharacterUpdate();
        }

        public void EnableWalk(bool enabled)
        {
            if (m_CanWalk != enabled)
            {
                m_CanWalk = enabled;
            }
        }

        public void EnableGravity(bool enabled)
        {
            if (m_EnableGravity != enabled)
            {
                m_EnableGravity = enabled;
            }
        }

        public void EnableCollisions(bool enabled)
        {
            if (m_collisionsEnabled != enabled)
            {
                m_collisionsEnabled = enabled;
                
                m_CharacterController.detectCollisions = enabled;

                for (int layer = 0; layer < 32; layer++)
                {
                    if (enabled)
                    {
                        Physics.IgnoreLayerCollision(gameObject.layer, layer, ignoreCollisionLayers[layer]);
                    }
                    else
                    {
                        Physics.IgnoreLayerCollision(gameObject.layer, layer, true);
                    }
                }
            }
        }

        public Vector3 GetBase()
        {
            return transform.position + Vector3.down * m_CharacterController.height/2;
        }


        private void PlayLandingSound()
        {
            m_AudioSource.clip = m_LandSound;
            m_AudioSource.Play();
            m_NextStep = m_StepCycle + .5f;
        }


        private void CharacterUpdate()
        {
            GetInput();

            if (m_CanWalk)
            {
                if (m_Sliding)
                {
                    m_CharacterController.height = Mathf.Lerp(m_CharacterController.height, m_CrouchHeight, Time.deltaTime * m_HeightChangeSpeed);
                } else
                {
                    m_CharacterController.height = Mathf.Lerp(m_CharacterController.height, m_NormalHeight, Time.deltaTime * m_HeightChangeSpeed);
                }

                // always move along the camera forward as it is the direction that it being aimed at
                Vector3 desiredMove = transform.forward * m_Input.y + transform.right * m_Input.x;

                // get a normal for the surface that is being touched to move along it
                RaycastHit hitInfo;
                Physics.SphereCast(transform.position, m_CharacterController.radius, Vector3.down, out hitInfo, m_CharacterController.height / 2f, Physics.AllLayers, QueryTriggerInteraction.Ignore);
                desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal);
                desiredMove = Vector3.ClampMagnitude(desiredMove, 1);

                desiredMove *= m_CurrentSpeed;

                float acceleration = m_GroundAcceleration;
                if (m_CharacterController.isGrounded)
                {
                    acceleration = GetMovementInfo().acceleration;
                } else
                {
                    if (m_Jumping)
                    {
                        acceleration = m_JumpAirAcceleration;
                    } else
                    {
                        acceleration = m_AirAcceleration;
                    }
                }
                m_MoveDir.x = Mathf.Lerp(m_MoveDir.x, desiredMove.x, Time.deltaTime * acceleration);
                m_MoveDir.z = Mathf.Lerp(m_MoveDir.z, desiredMove.z, Time.deltaTime * acceleration);
            } else
            {
                m_CharacterController.height = Mathf.Lerp(m_CharacterController.height, m_NormalHeight, Time.deltaTime * m_HeightChangeSpeed);
            }

            if (m_CanWalk && m_CharacterController.isGrounded)
            {
                m_MoveDir.y = -m_StickToGroundForce;

                if (m_Jump)
                {
                    m_MoveDir.y = m_JumpSpeed;
                    PlayJumpSound();
                    m_Jump = false;
                    m_Jumping = true;
                }
            }
            else if (m_EnableGravity)
            {
                m_MoveDir += Physics.gravity * m_GravityMultiplier * Time.deltaTime;
            } else if (m_CanWalk)
            {
                m_MoveDir.y = 0;
            }

            m_CollisionFlags = m_CharacterController.Move(m_MoveDir * Time.deltaTime);

            ProgressStepCycle(m_CurrentSpeed);
            UpdateCameraPosition(m_CurrentSpeed);

            m_MouseLook.UpdateCursorLock();
        }


        private void PlayJumpSound()
        {
            m_AudioSource.clip = m_JumpSound;
            m_AudioSource.Play();
        }

        private MovementInfo GetMovementInfo()
        {
            switch (m_WalkState)
            {
                case WalkState.RUN:
                    return runInfo;
                case WalkState.DASH:
                    return dashInfo;
                case WalkState.SLIDE:
                    return slideInfo;
                case WalkState.WALK:
                default:
                    return walkInfo;
            }
        }

        private void ProgressStepCycle(float speed)
        {
            if (m_CharacterController.velocity.sqrMagnitude > 0 && (m_Input.x != 0 || m_Input.y != 0))
            {
                m_StepCycle += (m_CharacterController.velocity.magnitude + GetMovementInfo().stepLength) * Time.deltaTime;
            }

            if (!(m_StepCycle > m_NextStep))
            {
                return;
            }

            m_NextStep = m_StepCycle + m_StepInterval;

            PlayFootStepAudio();
        }


        private void PlayFootStepAudio()
        {
            if (!m_CharacterController.isGrounded)
            {
                return;
            }
            // pick & play a random footstep sound from the array,
            // excluding sound at index 0
            int n = Random.Range(1, m_FootstepSounds.Length);
            m_AudioSource.clip = m_FootstepSounds[n];
            m_AudioSource.PlayOneShot(m_AudioSource.clip);
            // move picked sound to index 0 so it's not picked next time
            m_FootstepSounds[n] = m_FootstepSounds[0];
            m_FootstepSounds[0] = m_AudioSource.clip;
        }


        private void UpdateCameraPosition(float speed)
        {
            Vector3 newCameraPosition;
            if (!m_UseHeadBob)
            {
                return;
            }
            if (m_CharacterController.velocity.magnitude > 0 && m_CharacterController.isGrounded)
            {
                m_Camera.transform.localPosition = m_HeadBob.DoHeadBob(m_CharacterController.velocity.magnitude + (speed * GetMovementInfo().stepLength));
                newCameraPosition = m_Camera.transform.localPosition;
                newCameraPosition.y = m_Camera.transform.localPosition.y - m_JumpBob.Offset();
            }
            else
            {
                newCameraPosition = m_Camera.transform.localPosition;
                newCameraPosition.y = m_OriginalCameraPosition.y - m_JumpBob.Offset();
            }
            m_Camera.transform.localPosition = newCameraPosition;
        }


        private void GetInput()
        {
            // Read input
            float horizontal = CrossPlatformInputManager.GetAxis("Horizontal");
            float vertical = CrossPlatformInputManager.GetAxis("Vertical");
            
            // set the desired speed to be walking or running
            m_Input = new Vector2(horizontal, vertical);

            // normalize input if it exceeds 1 in combined length:
            if (m_Input.sqrMagnitude > 1)
            {
                m_Input.Normalize();
            }

            
            WalkState previousWalkState = m_WalkState;

            UpdateWalkState();

            m_CurrentSpeed = GetCurrentSpeed();

            // handle speed change to give an fov kick
            // only if the player is going to a run, is running and the fovkick is to be used
            //if (m_WalkState != previousWalkState && m_UseFovKick && m_CharacterController.velocity.sqrMagnitude > 0)
            //{
            //    StopAllCoroutines();
            //    StartCoroutine(!m_IsWalking ? m_FovKick.FOVKickUp() : m_FovKick.FOVKickDown());
            //}
        }

        private float GetCurrentSpeed()
        {
            if (m_WalkState == WalkState.SLIDE)
            {
                return ConvertRange(m_SlideElapsedTime, 0, m_SlideTime, slideInfo.speed, walkInfo.speed);
            }
            else
            {
                return GetMovementInfo().speed;
            }
        }

        private float ConvertRange(float oldValue, float oldMin, float oldMax, float newMin, float newMax)
        {
            return (((oldValue - oldMin) * (newMax - newMin)) / (oldMax - oldMin)) + newMin;
        }

        float runDoubleTapTimer;
        //private void UpdateWalkState()
        //{
        //    bool run = CrossPlatformInputManager.GetButton("Run");
        //    bool runPressed = CrossPlatformInputManager.GetButtonDown("Run");
        //    bool runReleased = CrossPlatformInputManager.GetButtonUp("Run");

        //    if (m_Input.magnitude > 0)
        //    {

        //        switch (m_WalkState)
        //        {
        //            case WalkState.WALK:
        //                if (run)
        //                {
        //                    m_WalkState = WalkState.RUN;
        //                }
        //                break;
        //            case WalkState.RUN:
        //                //if (!run)
        //                //{
        //                //    m_WalkState = WalkState.WALK;
        //                //}


        //                if (m_Slide)
        //                {
        //                    m_SlideElapsedTime = 0f;
        //                    m_WalkState = WalkState.SLIDE;
        //                }

        //                if (runReleased)
        //                {
        //                    runDoubleTapTimer = Time.time;
        //                }
        //                else if (!run && Time.time - runDoubleTapTimer > m_RunDoubleTapTime)
        //                {
        //                    m_WalkState = WalkState.WALK;
        //                }
        //                break;
        //            case WalkState.SLIDE:
        //                m_Sliding = true;

        //                m_SlideElapsedTime += Time.deltaTime;

        //                if (!m_CharacterController.isGrounded || m_SlideElapsedTime > m_SlideTime)
        //                {
        //                    m_Sliding = false;
        //                    m_WalkState = WalkState.WALK;
        //                }
        //                break;
        //        }
        //    }
        //    else
        //    {
        //        m_WalkState = WalkState.WALK;
        //    }
        //}
        private void UpdateWalkState()
        {
            bool run = CrossPlatformInputManager.GetButton("Run");
            bool runPressed = CrossPlatformInputManager.GetButtonDown("Run");
            bool runReleased = CrossPlatformInputManager.GetButtonUp("Run");

            if (m_Input.magnitude > 0)
            {
                //TODO
                //if dashing -> do dash
                //else if not dashing and run pressed -> start dash
                //else if not dashing and run held -> run
                //else -> walk
                
                switch (m_WalkState)
                {
                    case WalkState.WALK:
                        if (run)
                        {
                            m_WalkState = WalkState.RUN;
                        }
                        break;
                    case WalkState.RUN:
                        if (m_Slide)
                        {
                            m_SlideElapsedTime = 0f;
                            m_WalkState = WalkState.SLIDE;
                        }

                        if (runReleased)
                        {
                            runDoubleTapTimer = Time.time;
                        }
                        else if (!run && Time.time - runDoubleTapTimer > m_RunDoubleTapTime)
                        {
                            m_WalkState = WalkState.WALK;
                        }
                        break;
                    case WalkState.SLIDE:
                        m_Sliding = true;

                        m_SlideElapsedTime += Time.deltaTime;

                        if (!m_CharacterController.isGrounded || m_SlideElapsedTime > m_SlideTime)
                        {
                            m_Sliding = false;
                            m_WalkState = WalkState.WALK;
                        }
                        break;
                }
            }
            else
            {
                m_WalkState = WalkState.WALK;
            }
        }


        private void RotateView()
        {
            if (!m_ManuallyRotate)
            {
                m_MouseLook.LookRotation(transform, m_Camera.transform);
            }
        }


        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            Rigidbody body = hit.collider.attachedRigidbody;
            //dont move the rigidbody if the character is on top of it
            if (m_CollisionFlags == CollisionFlags.Below)
            {
                return;
            }

            if (body == null || body.isKinematic)
            {
                return;
            }
            body.AddForceAtPosition(m_CharacterController.velocity*0.1f, hit.point, ForceMode.Impulse);
        }
    }
}
