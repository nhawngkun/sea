using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Cinemachine;
using StarterAssets;
using UnityEngine.InputSystem;
using Cinemachine.Utility;
using UnityEngine.Animations.Rigging;
public class ThirdPersonShooterController : MonoBehaviour
{
    [SerializeField] private Rig rig;
    [SerializeField] private CinemachineVirtualCamera aimVR;
    [SerializeField] private float normalSensitivity;
    [SerializeField] private float aimSensitivity;
    [SerializeField] private LayerMask aimColliderLayerMask= new LayerMask();
   
    [SerializeField] private Transform PFbullet;
    [SerializeField] private Transform spawnBullet;
    [SerializeField] private Transform HitShoot;
    [SerializeField] private Transform Hitobject;
    [SerializeField] private Transform Hitlivingthing;
    [SerializeField] private Transform H;
    [SerializeField] private bool isshoot =false;
    [SerializeField] private Image img;



    private ThirdPersonController thirdPersonController;
    private StarterAssetsInputs SaInput;
    private Animator ani;
    private float aimrigWe;

    private void Awake()
    {
        thirdPersonController = GetComponent<ThirdPersonController>();
        SaInput = GetComponent<StarterAssetsInputs>();
        ani = GetComponent<Animator>();
    }
    // Start is called before the first frame update
    void Start()
    {
        img.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
       
        Vector3 mouseWorldPosition = Vector3.zero;
        rig.weight = Mathf.Lerp(rig.weight, aimrigWe, Time.deltaTime * 20f);
        Vector2 ScreenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        Transform hitTransform = null;
        if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f, aimColliderLayerMask))
        {
            H.position = raycastHit.point;
            mouseWorldPosition = raycastHit.point;
            hitTransform = raycastHit.transform;

        }
        if (SaInput.aim)
        {
            aimVR.gameObject.SetActive(true);
            thirdPersonController.SetSensitivity(aimSensitivity);
            thirdPersonController.SetRotateOnMove(false);
            ani.SetLayerWeight(1, Mathf.Lerp(ani.GetLayerWeight(1),1f,Time.deltaTime *10f));
            aimrigWe = 1f;
            Vector3 worldAimTarger = mouseWorldPosition;
            worldAimTarger.y = transform.position.y;
            Vector3 aimDurection = (worldAimTarger - transform.position).normalized;
            transform.forward = Vector3.Lerp(transform.forward, aimDurection, Time.deltaTime * 20f);
            isshoot = true;
            img.enabled = true;
        }
        else
        {
            aimVR.gameObject.SetActive(false);
            thirdPersonController.SetSensitivity(normalSensitivity);
            thirdPersonController.SetRotateOnMove(true);
            ani.SetLayerWeight(1, Mathf.Lerp(ani.GetLayerWeight(1), 0f, Time.deltaTime * 10f));
            aimrigWe = 0f;
            isshoot = false;
            img.enabled = false;
        }
        if (SaInput.shoot && isshoot)
        {
            if(hitTransform != null)
            {
                if (hitTransform.CompareTag("Enemy"))
                {
                    Instantiate(Hitlivingthing, transform.position, Quaternion.identity);
                }
                else
                {
                    Instantiate(Hitobject, transform.position, Quaternion.identity);



                }
            }
            Vector3 aimDir = (mouseWorldPosition - spawnBullet.position).normalized;
            Instantiate(PFbullet, spawnBullet.position, Quaternion.LookRotation(aimDir, Vector3.up));
            Instantiate(HitShoot, spawnBullet.position, Quaternion.LookRotation(aimDir, Vector3.up));
            SaInput.shoot = false;
        }
    }
}
