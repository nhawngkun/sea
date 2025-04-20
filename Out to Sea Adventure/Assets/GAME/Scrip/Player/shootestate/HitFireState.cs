using UnityEngine;

public class HitFireState : AimBaseState
{
    public override void EnterState(AimStateManager aim)
    {
        aim.ani.SetBool("Aim", false);
    }

    public override void UpdateState(AimStateManager aim)
    {
        // Switch between first-person and third-person camera (only if isShoot is true)
        if (Input.GetKeyDown(KeyCode.Mouse1) && aim.Isshoot)
        {
            aim.SwitchToFirstPerson();
        }
        else if (Input.GetKeyUp(KeyCode.Mouse1))
        {
            aim.SwitchToThirdPerson();
        }

        // Handle shooting
        HandleShooting(aim);

        if (Input.GetKey(KeyCode.Mouse0) && aim.Isshoot)
        {
            // Switch to third person before changing state
            aim.SwitchToThirdPerson();
            aim.SwitchState(aim.Aim);
        }

        // Add this to allow toggling shooting off from HitFireState
        if (Input.GetKeyDown(KeyCode.X) && aim.Isshoot)
        {
            // Switch to third person before changing state
            aim.SwitchToThirdPerson();
            aim.SwitchState(aim.noshoot);
        }
    }
}