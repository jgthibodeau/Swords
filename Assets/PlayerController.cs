using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public enum PlayerState
    {
        NORMAL, GRAPPLING, WALLRUNNING, SLIDING
    }
    public PlayerState currentState = PlayerState.NORMAL;
    
    private UnityStandardAssets.Characters.FirstPerson.FirstPersonController fpsController;
    private WallRun wallRun;
    private GrapplingHook grapplingHook;
    //private Slide slide;

    void Start()
    {
        fpsController = GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>();

        wallRun = GetComponent<WallRun>();
        wallRun.playerController = this;

        grapplingHook = GetComponent<GrapplingHook>();
        grapplingHook.playerController = this;
    }

    // Update is called once per frame
    void Update()
    {
        StateBehavior();
    }

    void StateBehavior()
    {
        switch (currentState)
        {
            case PlayerState.NORMAL:
                fpsController.EnableWalk(true);
                fpsController.EnableGravity(true);
                fpsController.EnableCollisions(true);
                break;
            case PlayerState.GRAPPLING:
                //disable fps control input
                break;
            case PlayerState.WALLRUNNING:
                //disable fps control input
                break;
            case PlayerState.SLIDING:
                //disable fps control input
                break;
        }
    }

    void CleanupPreviousState(PlayerState previousState)
    {
        switch (previousState)
        {
            case PlayerState.NORMAL:
                break;
            case PlayerState.GRAPPLING:
                grapplingHook.Halt();
                break;
            case PlayerState.WALLRUNNING:
                wallRun.Halt();
                break;
            case PlayerState.SLIDING:
                //slide.Halt();
                break;
        }
    }
    
    public void StateStart(PlayerState newState)
    {
        if (currentState != newState)
        {
            Debug.Log("Cleaning state " + currentState);
            CleanupPreviousState(currentState);
            Debug.Log("Starting state " + newState);
            currentState = newState;
        }
    }

    public void StateEnd(PlayerState staleState)
    {
        if (currentState == staleState)
        {
            StateStart(PlayerState.NORMAL);
        }
    }
}
