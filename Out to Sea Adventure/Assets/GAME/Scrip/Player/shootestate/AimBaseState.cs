using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

// Update AimBaseState to add shooting functionality
public abstract class AimBaseState
{
    public abstract void EnterState(AimStateManager aim);
    public abstract void UpdateState(AimStateManager aim);

    // Common shooting logic that can be called from different states
    protected void HandleShooting(AimStateManager aim)
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) && aim.Isshoot)
        {
            // Get screen center for raycast
            Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
            Ray ray = Camera.main.ScreenPointToRay(screenCenter);
            RaycastHit hit;
            Vector3 targetPoint = aim.aimPos.position; // Default to aim position

            // Perform raycast to determine target point
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, aim.aimMask))
            {
                targetPoint = hit.point;
            }

            // Calculate direction for bullet
            Vector3 shootDirection = (targetPoint - aim.bulletSpawnPoint.position).normalized;

            // Spawn bullet and muzzle flash
            Transform bulletInstance = Object.Instantiate(aim.bulletPrefab, aim.bulletSpawnPoint.position, Quaternion.LookRotation(shootDirection, Vector3.up));

            // Set effect references on the bullet
            Bullet bulletScript = bulletInstance.GetComponent<Bullet>();
            if (bulletScript != null)
            {
                bulletScript.hitObjectEffect = aim.hitObjectEffect;
                bulletScript.hitLivingEffect = aim.hitLivingEffect;
            }

            Object.Instantiate(aim.muzzleFlashPrefab, aim.bulletSpawnPoint.position, Quaternion.LookRotation(shootDirection, Vector3.up));

            // Play shooting animation
            aim.ani.SetTrigger("Shoot");

            // Call recoil effect
            aim.StartCoroutine(aim.CameraRecoil());
        }
    }
}