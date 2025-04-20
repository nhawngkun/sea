using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkingState : MoveBaseState
{
    // Start is called before the first frame update
    public override void EnterState(MoveStateManager move)
    {
        move.ani.SetBool("Walking", true);
    }

    public override void UpdetaState(MoveStateManager move)
    {
        // Check for jump
        if (Input.GetKeyDown(KeyCode.Space) && move.IsGround())
        {
            ExitState(move, move.jumping);
            return;
        }

        if (Input.GetKey(KeyCode.LeftShift))
        {
            ExitState(move, move.run);
        }
        else if (Input.GetKeyDown(KeyCode.C))
        {
            ExitState(move, move.crouch);
        }
        else if (move.dir.magnitude < 0.1f)
        {
            ExitState(move, move.idle);
        }

        if (move.VInput < 0)
        {
            move.currmoveSpeed = move.walkbackSpeed;
        }
        else
        {
            move.currmoveSpeed = move.currwalkSpeed;
        }
    }

    void ExitState(MoveStateManager move, MoveBaseState state)
    {
        move.ani.SetBool("Walking", false);
        move.SwitchState(state);
    }
}