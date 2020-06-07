// SmoothDamp based head bob
// Not so good, yet
//
// 1. Turn off other head bobs
// 2. Put the script on the Character root
// 3. Assign the player's camera as 'Head'
// 4. Play, fiddle with values
//
// It requires Character Controller component to function
//
// by Nothke

using UnityEngine;

public class StepBob : MonoBehaviour
{
    public Transform head;

    public float headSmoothing = 0.2f;
    public float bobHeight = -0.1f;
    public float bobAfterDistance = 2;
    public float bobDuration = 0.1f;

    float startHeight;

    CharacterController _character;
    CharacterController character { get { if (!_character) _character = GetComponent<CharacterController>(); return _character; } }

    void Start()
    {
        startHeight = head.transform.localPosition.y;
    }

    float yVelo;
    float ySmoothVelo;

    float bobDistance = 1;
    float bobTime;

    const float THRESHOLD_SPEED = 0.01f;
    const float FIRST_STEP_DISTANCE = 0.2f;

    public delegate void StepAction();
    public event StepAction OnStep;

    void Update()
    {
        Vector3 velo = character.velocity;
        yVelo = velo.y;

        float deltaV = new Vector2(velo.x, velo.z).magnitude;
        float deltaDistance = deltaV * Time.deltaTime;

        if (deltaV < THRESHOLD_SPEED)
            bobDistance = FIRST_STEP_DISTANCE;

        bobDistance -= deltaDistance;

        bobTime -= Time.deltaTime;

        if (bobDistance < 0 && character.isGrounded)
        {
            if (OnStep != null)
                OnStep();

            bobTime = bobDuration;
            bobDistance = bobAfterDistance;
        }

        float yTarget = character.velocity.y * 0.1f;
        if (bobTime > 0) yTarget += bobHeight;
        yTarget = Mathf.Clamp(yTarget, -1, 0.1f);
        yTarget += startHeight;

        float y = Mathf.SmoothDamp(head.localPosition.y, yTarget, ref ySmoothVelo, headSmoothing);

        head.localPosition = new Vector3(0, y, 0);
    }
}