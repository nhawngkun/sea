using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private Rigidbody rb;
    public Transform hitObjectEffect;
    public Transform hitLivingEffect;

    // Start is called before the first frame update
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // Try to get references from AimStateManager if not already assigned
        if (hitObjectEffect == null || hitLivingEffect == null)
        {
            AimStateManager aimManager = FindObjectOfType<AimStateManager>();
            if (aimManager != null)
            {
                hitObjectEffect = aimManager.hitObjectEffect;
                hitLivingEffect = aimManager.hitLivingEffect;
            }
        }
    }

    void Start()
    {
        float speed = 40f;
        rb.velocity = transform.forward * speed;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Get the hit point from the bullet's position
        Vector3 hitPoint = transform.position;

        // For more accurate hit point, you can use Raycast
        RaycastHit hit;
        if (Physics.Raycast(transform.position - transform.forward * 0.1f, transform.forward, out hit, 0.2f))
        {
            hitPoint = hit.point;
        }

        // Check the tag of the hit object
        if (other.CompareTag("Enemy"))
        {
            if (hitLivingEffect != null)
            {
                Instantiate(hitLivingEffect, hitPoint, Quaternion.LookRotation(transform.forward));
            }
        }
        else
        {
            if (hitObjectEffect != null)
            {
                Instantiate(hitObjectEffect, hitPoint, Quaternion.LookRotation(transform.forward));
            }
        }

        // Destroy the bullet after trigger
        Destroy(gameObject);
    }
}