using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwimmingState : MoveBaseState
{
    // Start is called before the first frame update
    public override void EnterState(MoveStateManager move)
    {
        move.ani.SetBool("Swimming", true);
        move.ani.SetBool("FloatingWater", false);

        // Reset other states
        move.ani.SetBool("Running", false);
        move.ani.SetBool("Walking", false);
        move.ani.SetBool("Crouching", false);

        // Reset vertical velocity when entering swimming state
        // This ensures we start with a neutral velocity
        move.velocity.y = 0f;
    }

    public override void UpdetaState(MoveStateManager move)
    {
        // Handle state transitions
        if (!move.IsInWater)
        {
            // If no longer in water, switch to idle or walking based on input
            if (move.dir.magnitude < 0.01f)
            {
                ExitState(move, move.idle);
            }
            else
            {
                ExitState(move, move.Walk);
            }
            return;
        }

        // If diving down (Q key)
        if (Input.GetKey(KeyCode.Q))
        {
            ExitState(move, move.diving);
            return;
        }

        // If not moving and in water, switch to floating
        if (move.dir.magnitude < 0.01f)
        {
            ExitState(move, move.floating);
            return;
        }

        // Set swim speed
        move.currmoveSpeed = move.swimmingSpeed;

        // Apply buoyancy - in swimming state
        if (move.IsUnderwater)
        {
            if (Input.GetKey(KeyCode.E))
            {
                // When underwater and pressing E to rise - use a stronger upward force
                move.Gravity = 1f; // Increased upward force to swim up more noticeably

                // Directly apply additional upward force to ensure movement
                move.velocity.y += 1f * Time.deltaTime;

                // Debug log to check if this code is being executed
               
            }
            else
            {
                // Regular swimming underwater without pressing E
                move.Gravity = 0.5f;
                move.velocity.y += 0.5f * Time.deltaTime;// Slight upward force to prevent sinking
            }
        }
        else
        {
            // When head is already above water (IsUnderwater = false)
            move.Gravity = 0f;
            move.velocity.y = 0f;// Stay at surface level, don't keep rising
        }
    }

    void ExitState(MoveStateManager move, MoveBaseState state)
    {
        move.ani.SetBool("Swimming", false);
        move.SwitchState(state);
    }
}