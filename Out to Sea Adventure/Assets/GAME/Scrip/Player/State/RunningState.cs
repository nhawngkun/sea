using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunningState : MoveBaseState
{
    // Start is called before the first frame update
    public override void EnterState(MoveStateManager move)
    {
        move.ani.SetBool("Running", true);

    }

    public override void UpdetaState(MoveStateManager move)
    {
        // Check for jump
        if (Input.GetKeyDown(KeyCode.Space) && move.IsGround())
        {
            ExitState(move, move.jumping);
            return;
        }
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            ExitState(move, move.Walk);
        }
        else if (move.dir.magnitude < 0.1f)
        {
            ExitState(move, move.idle);
        }
        if (move.VInput < 0)
        {
            move.currmoveSpeed = move.runbackSpeed;

        }
        else
        {
            move.currmoveSpeed = move.currrunSpeed;
        }
    }
    void ExitState(MoveStateManager move, MoveBaseState state)
    {
        move.ani.SetBool("Running", false);
        move.SwitchState(state);
    }
}