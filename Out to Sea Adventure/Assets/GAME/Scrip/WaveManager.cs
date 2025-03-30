using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    // Start is called before the first frame update
    public static WaveManager instance;
    public float amlitude =1f;
    public float lengh =2f;
    public float speed =1f;
    public float offset=0f;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;

        }else if(instance!=this)
        {
            Debug.Log("hi");
            Destroy(this);

        }
    }
    private void Update()
    {
        offset +=Time.deltaTime*speed;

    }
    public float GetWaveHeight(float _x){
        return amlitude *Mathf.Sin(_x/lengh + offset);
    }
}
