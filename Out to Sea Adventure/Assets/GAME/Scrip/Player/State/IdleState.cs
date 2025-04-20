using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : MoveBaseState
{
    // Start is called before the first frame update
    public override void EnterState(MoveStateManager move)
    {
        // Reset all animation bools to ensure clean state
       
    }

    public override void UpdetaState(MoveStateManager move)
    {
        // Check for jump
        if (Input.GetKeyDown(KeyCode.Space) && move.IsGround())
        {
            ExitState(move, move.jumping);
            return;
        }
        // Check for water first
        if (move.IsInWater)
        {
            move.SwitchState(move.floating);
            return;
        }

        // Check for jump input
        if (Input.GetKeyDown(KeyCode.Space) && move.IsGround())
        {
            move.SwitchState(move.jumping);
            return;
        }

        // Regular land movement transitions
        if (move.dir.magnitude > 0.01f)
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                move.SwitchState(move.run);
            }
            else
            {
                move.SwitchState(move.Walk);
            }
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            move.SwitchState(move.crouch);
        }
    }
    void ExitState(MoveStateManager move, MoveBaseState state)
    {
       
        move.SwitchState(state);
    }
}