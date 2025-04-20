using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrouchingState : MoveBaseState
{
    public override void EnterState(MoveStateManager move)
    {
        move.ani.SetBool("Crouching", true);

    }

    public override void UpdetaState(MoveStateManager move)
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            ExitState(move, move.run);
        }
        else if (Input.GetKeyDown(KeyCode.C))
        {
           if(move.dir.magnitude < 0.01f)
            {
                ExitState(move, move.idle);
            }
            else
            {
                ExitState(move, move.Walk);
            }
        }

        if (move.VInput < 0)
        {
            move.currmoveSpeed = move.crouchbackSpeed;

        }
        else
        {
            move.currmoveSpeed = move.currcrouchSpeed;
        }

    }
    void ExitState(MoveStateManager move, MoveBaseState state)
    {
        move.ani.SetBool("Crouching", false);
        move.SwitchState(state);
    }

    // Start is called before the first frame update

}
