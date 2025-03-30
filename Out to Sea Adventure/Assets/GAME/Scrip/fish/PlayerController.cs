using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public FishingSystem fishingSystem;

    private Vector2 moveInput;
    private CharacterController characterController;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
    }

    void Update()
    {
        // Di chuyển
        

        // Ném câu
       if (Input.GetKeyDown(KeyCode.Space))
        {
            fishingSystem.Cast();
        }
    }
}
