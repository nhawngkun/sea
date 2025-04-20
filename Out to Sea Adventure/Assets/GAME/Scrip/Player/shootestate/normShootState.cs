using UnityEngine;

public class NormShootState : AimBaseState
{
    public override void EnterState(AimStateManager aim)
    {
        aim.ani.SetLayerWeight(1, Mathf.Lerp(aim.ani.GetLayerWeight(1), 0f, Time.deltaTime * 100f));
        aim.Isshoot = false;
        aim.Ribl.enabled = false;

        // Ensure third person view when entering this state
        aim.SwitchToThirdPerson();
    }

    public override void UpdateState(AimStateManager aim)
    {
        // Ensure the animation layer gradually turns off
        aim.ani.SetLayerWeight(1, Mathf.Lerp(aim.ani.GetLayerWeight(1), 0f, Time.deltaTime * 100f));

        // In normal state, always ensure we're in third-person view
        // and first-person camera is completely disabled
        if (aim.isFirstPerson)
        {
            aim.SwitchToThirdPerson();
        }

        // Make sure first-person camera is deactivated in this state
        if (aim.firstPersonCamera.gameObject.activeInHierarchy)
        {
            aim.firstPersonCamera.gameObject.SetActive(false);
        }

        // Kiểm tra nếu người chơi bấm X và KHÔNG ở dưới nước
        if (Input.GetKeyDown(KeyCode.X) && !aim.MSM.IsInWater)
        {
            aim.ani.SetLayerWeight(1, Mathf.Lerp(aim.ani.GetLayerWeight(1), 1f, Time.deltaTime * 100f));
            aim.Isshoot = true;
            aim.Ribl.enabled = true;

            // Always ensure we're starting in third-person mode when activating shooting
            aim.SwitchToThirdPerson();

            // If player is also holding the right mouse button, transition to aim state
            if (Input.GetKey(KeyCode.Mouse0))
            {
                aim.SwitchState(aim.Aim);
            }
            else
            {
                aim.SwitchState(aim.Hit);
            }
        }
    }
}