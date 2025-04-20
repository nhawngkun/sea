using UnityEngine;

public class AimState : AimBaseState
{
    public override void EnterState(AimStateManager aim)
    {
        aim.ani.SetBool("Aim", true);
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

        // State switching logic
        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            // Switch to third person view before changing state
            aim.SwitchToThirdPerson();
            aim.SwitchState(aim.Hit);
        }

        if (Input.GetKeyDown(KeyCode.F) && aim.Isshoot)
        {
            // Switch to third person view before changing to no shoot state
            aim.SwitchToThirdPerson();
            aim.SwitchState(aim.noshoot);
        }
    }
}
