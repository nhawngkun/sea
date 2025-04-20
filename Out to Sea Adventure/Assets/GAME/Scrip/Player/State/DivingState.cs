using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DivingState : MoveBaseState
{
    // Start is called before the first frame update
    public override void EnterState(MoveStateManager move)
    {
        move.ani.SetBool("Swimming", true);  // Use swimming animation for diving
        move.ani.SetBool("FloatingWater", false);

        // Reset other states
        move.ani.SetBool("Running", false);
        move.ani.SetBool("Walking", false);
        move.ani.SetBool("Crouching", false);

        // Reset velocity when entering diving state
        move.velocity.y = -1.0f; // Start with a small downward push
    }

    public override void UpdetaState(MoveStateManager move)
    {
        // Handle state transitions
        if (!move.IsInWater)
        {
            // If no longer in water, switch to idle or walking based on input
            if (move.dir.magnitude < 0.1f)
            {
                ExitState(move, move.idle);
            }
            else
            {
                ExitState(move, move.Walk);
            }
            return;
        }

        // If no longer pressing Q (dive key) and not moving
        if (!Input.GetKey(KeyCode.Q) && move.dir.magnitude < 0.01f)
        {
            ExitState(move, move.floating);
            return;
        }

        // If no longer pressing Q but still moving
        if (!Input.GetKey(KeyCode.Q) && move.dir.magnitude >= 0.01f)
        {
            ExitState(move, move.swimming);
            return;
        }

        // If pressing E to swim upward
        if (Input.GetKey(KeyCode.E))
        {
            ExitState(move, move.swimming);
            return;
        }

        // When diving, adjust movement speed
        move.currmoveSpeed = move.swimmingSpeed * 0.7f;  // Slower movement when diving

        // Handle diving movement
        if (move.IsGround())
        {
            // NEW CODE: When touching seafloor in diving state, allow horizontal movement
            // but add a small downward force to keep player on the bottom
            move.Gravity = -0.5f;

            // Add a small bounce-off force when hitting the ground to prevent getting stuck
            if (move.velocity.y <= 0)
            {
                move.velocity.y = -0.5f;
            }
        }
        else
        {
            // Apply downward force for diving when not touching ground
            move.Gravity = -2.0f; // Stronger downward force when actively diving
        }
    }

    void ExitState(MoveStateManager move, MoveBaseState state)
    {
        move.ani.SetBool("Swimming", false);
        move.SwitchState(state);
    }
}