using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class floater : MonoBehaviour
{
    public Rigidbody rb;
    public float depthBeforeSubmergerd =1f;
    public float displacemnentAmount =3f;
    public int floatCount =1;
    public float waterDrag = 0.99f;
    public float waterAnglarDarg =0.5f;
    // Start is called before the first frame update
    private void FixedUpdate()
    {
        rb.AddForceAtPosition(Physics.gravity/ floatCount, transform.position,ForceMode.Acceleration);
        float waveHeight = WaveManager.instance.GetWaveHeight(transform.position.x);
        if(transform.position.y <waveHeight)
        {
            float displacemnenMultiplier =Mathf.Clamp01((waveHeight-transform.position.y)/depthBeforeSubmergerd)*displacemnentAmount;
            rb.AddForceAtPosition(new Vector3(0f,Mathf.Abs(Physics.gravity.y)*displacemnenMultiplier,0f),transform.position,ForceMode.Acceleration);
             rb.AddForce(displacemnenMultiplier * -rb.velocity*waterDrag*Time.fixedDeltaTime,ForceMode.VelocityChange);
            rb.AddTorque(displacemnenMultiplier * -rb.angularVelocity *Time.fixedDeltaTime,ForceMode.VelocityChange); 

        }
    }
}
