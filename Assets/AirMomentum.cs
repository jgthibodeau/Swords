using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirMomentum : MonoBehaviour
{
    public UnityStandardAssets.Characters.FirstPerson.FirstPersonController fpsController;

    public bool inAir, manuallyMoved;
    public Vector3 currentDirection;

    // Start is called before the first frame update
    void Start()
    {
        fpsController = GetComponentInChildren<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (inAir)
        {
            if (!manuallyMoved)
            {
                //adjust currentDirection due to gravity

                if (fpsController.m_CharacterController.isGrounded)
                {
                    Land();
                }
                else
                {
                    currentDirection += Vector3.down * fpsController.m_GravityMultiplier;
                    fpsController.m_CharacterController.Move(transform.position + currentDirection);
                }
            } else
            {
                manuallyMoved = false;
                fpsController.EnableCollisions(true);
            }
        }
        else
        {
            fpsController.EnableCollisions(true);
        }
    }

    public void MoveToward(Vector3 newPosition, float speed)
    {
        currentDirection = (newPosition - transform.position).normalized * speed;
        fpsController.m_CharacterController.Move(newPosition);

        fpsController.EnableCollisions(false);
        inAir = true;
        manuallyMoved = true;
    }

    public void Land()
    {
        inAir = false;
    }
}
