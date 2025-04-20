using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingState : MoveBaseState
{
    // Start is called before the first frame update
    public override void EnterState(MoveStateManager move)
    {
        move.ani.SetBool("FloatingWater", true);
        move.ani.SetBool("Swimming", false);

        // Reset other states
        move.ani.SetBool("Running", false);
        move.ani.SetBool("Walking", false);
        move.ani.SetBool("Crouching", false);
    }

    public override void UpdetaState(MoveStateManager move)
    {
        // Handle state transitions
        if (!move.IsInWater)
        {
            // If no longer in water, switch to idle
            ExitState(move, move.idle);
            return;
        }

        // If moving in water
        if (move.dir.magnitude > 0.01f && move.IsInWater)
        {
            ExitState(move, move.swimming);
            return;
        }

        // If diving down (Q key)
        if (Input.GetKey(KeyCode.Q))
        {
            ExitState(move, move.diving);
            return;
        }

        // When floating in water, apply neutral buoyancy (don't sink or rise)
        move.Gravity = 0f; // Stand in water with no vertical movement
    }

    void ExitState(MoveStateManager move, MoveBaseState state)
    {
        move.ani.SetBool("FloatingWater", false);
        move.SwitchState(state);
    }
}