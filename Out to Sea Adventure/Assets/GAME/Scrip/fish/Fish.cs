using UnityEngine;
using System.Collections.Generic;
public class Fish : MonoBehaviour
{
    public string fishName;
    public float weight;
    public float catchDifficulty;
    public Sprite fishSprite;

    // Khởi tạo thông số cá
    public void Initialize(string name, float fishWeight, float difficulty)
    {
        fishName = name;
        weight = fishWeight;
        catchDifficulty = difficulty;
    }

    // Hành vi di chuyển của cá
    void Update()
    {
        // Logic di chuyển ngẫu nhiên của cá
        float moveSpeed = 2f;
        transform.Translate(
            new Vector3(
                Mathf.Sin(Time.time) * moveSpeed * Time.deltaTime, 
                0, 
                Mathf.Cos(Time.time) * moveSpeed * Time.deltaTime
            )
        );
    }
}
