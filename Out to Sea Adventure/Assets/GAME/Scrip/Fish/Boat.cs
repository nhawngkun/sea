using Ditzelgames;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarterAssets; // Added for ThirdPersonController
using DG.Tweening; // Thêm DOTween namespace

public class Boat : MonoBehaviour
{
    //visible Properties
    public Transform Motor;
    public float SteerPower = 500f;
    public float Power = 5f;
    public float MaxSpeed = 10f;
    public float Drag = 0.1f;

    // Added steering wheel settings from BoatController
    [Header("Bánh Lái Setting")]
    public Transform steeringWheel;      // Bánh lái của thuyền
    public float maxSteeringAngle = 45.0f; // Góc quay tối đa của bánh lái
    public float steeringTweenDuration = 0.3f; // Thời gian xoay bánh lái bằng DOTween
    public float continuousRotationSpeed = 120f; // Tốc độ xoay liên tục khi giữ phím (độ/giây)

    // Added player interaction from BoatController
    [Header("Player Interaction")]
    public string driveTriggerTag = "Drive";  // Tag của trigger zone để hiển thị prompt lái thuyền
    public GameObject drivePromptUI;          // UI prompt để lái thuyền
    public GameObject exitPromptUI;           // UI prompt để thoát thuyền
    public KeyCode driveKey = KeyCode.F;      // Phím để lái thuyền
    public KeyCode exitKey = KeyCode.F;       // Phím để thoát thuyền
    public Transform playerSeatPosition;      // Vị trí ngồi của người chơi khi lái thuyền
    public Camera cameraPlayer;
    public Camera cameraBoat;

    //used Components
    protected Rigidbody Rigidbody;
    protected Quaternion StartRotation;

    // Added from BoatController
    public bool canDrive = false;       // Có thể lái thuyền hay không
    public bool isDriving = false;      // Đang lái thuyền hay không
    private GameObject player;           // Đối tượng người chơi
    private MoveStateManager playerController; // Component controller của người chơi
    private Transform originalPlayerParent;

    // DOTween variables
    private Tweener steeringTweener;    // Biến lưu trữ tweener cho bánh lái
    private int currentSteerDirection = 0; // Hướng quay hiện tại của bánh lái

    //internal Properties
    protected Vector3 CamVel;

    public void Awake()
    {
        Rigidbody = GetComponent<Rigidbody>();
        StartRotation = Motor.localRotation;
    }

    private void Start()
    {
        // Added from BoatController Start method
        if (cameraBoat != null)
            cameraBoat.enabled = false;

        // Khởi tạo trạng thái ban đầu
        if (drivePromptUI != null)
            drivePromptUI.SetActive(false);

        if (exitPromptUI != null)
            exitPromptUI.SetActive(false);

        // Tìm người chơi trong scene
        player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            playerController = player.GetComponent<MoveStateManager>();
        }
        else
        {
            Debug.LogError("Không tìm thấy đối tượng Player!");
        }
    }

    public void Update()
    {
        // Added UI interaction logic from BoatController
        if (!isDriving && canDrive) // Không lái nhưng đang ở trong vùng trigger
        {
            // Hiển thị prompt lái và kiểm tra người chơi có muốn lái không
            if (drivePromptUI != null)
                drivePromptUI.SetActive(true);

            if (Input.GetKeyDown(driveKey))
            {
                EnterBoat();
            }
        }
        else if (isDriving)
        {
            // Hiển thị prompt thoát và kiểm tra người chơi có muốn thoát không
            if (exitPromptUI != null)
                exitPromptUI.SetActive(true);

            if (Input.GetKeyDown(exitKey))
            {
                ExitBoat();
            }

            // Xử lý xoay bánh lái với DOTween
            HandleSteeringWheelRotation();
        }
    }

    // Phương thức xử lý xoay bánh lái mượt mà với DOTween
    private void HandleSteeringWheelRotation()
    {
        int newSteerDirection = 0;

        // Xác định hướng lái
        if (Input.GetKey(KeyCode.A))
            newSteerDirection = -1;  // Sang phải (theo yêu cầu)
        if (Input.GetKey(KeyCode.D))
            newSteerDirection = 1; // Sang trái (theo yêu cầu)

        if (steeringWheel != null)
        {
            // Khi giữ phím A hoặc D
            if (newSteerDirection != 0)
            {
                // Hủy tween cũ nếu đang chạy
                if (steeringTweener != null && steeringTweener.IsActive())
                {
                    steeringTweener.Kill();
                }

                // Xoay bánh lái liên tục vô hạn khi giữ phím
                float rotationSpeed = continuousRotationSpeed * Time.deltaTime * newSteerDirection;

                // Xoay thêm dựa vào hướng và tốc độ
                steeringWheel.Rotate(0, 0, -rotationSpeed, Space.Self);

                // Lưu lại hướng quay hiện tại
                currentSteerDirection = newSteerDirection;
            }
            // Khi không nhấn phím, bánh lái từ từ quay về vị trí ban đầu
            else if (currentSteerDirection != 0)
            {
                steeringTweener = steeringWheel.DOLocalRotate(new Vector3(0, 0, 0), 0.5f)
                    .SetEase(Ease.OutQuad);
                currentSteerDirection = 0;
            }
        }
    }

    public void FixedUpdate()
    {
        // Only control boat if driving
        if (!isDriving)
            return;

        //default direction
        var forceDirection = transform.forward;
        var steer = 0;
        //steer direction [-1,0,1]
        if (Input.GetKey(KeyCode.A))
            steer = 1;
        if (Input.GetKey(KeyCode.D))
            steer = -1;

        //Rotational Force
        Rigidbody.AddForceAtPosition(steer * transform.right * SteerPower / 100f, Motor.position);

        //compute vectors
        var forward = Vector3.Scale(new Vector3(1, 0, 1), transform.forward);
        var targetVel = Vector3.zero;

        //forward/backward power
        if (Input.GetKey(KeyCode.W))
            PhysicsHelper.ApplyForceToReachVelocity(Rigidbody, forward * MaxSpeed, Power);
        if (Input.GetKey(KeyCode.S))
            PhysicsHelper.ApplyForceToReachVelocity(Rigidbody, forward * -MaxSpeed, Power);

        //Motor rotation
        Motor.SetPositionAndRotation(Motor.position, transform.rotation * StartRotation * Quaternion.Euler(0, 30f * steer, 0));
    }

    // Added boat entry/exit methods from BoatController
    private void EnterBoat()
    {
        if (player != null && playerController != null)
        {
            isDriving = true;

            if (cameraBoat != null)
            {
                cameraBoat.tag = "MainCamera";
                cameraBoat.enabled = true;
            }

            if (cameraPlayer != null)
                cameraPlayer.enabled = false;

            // Vô hiệu hóa ThirdPersonController
            playerController.enabled = false;

            // Lưu parent ban đầu của người chơi
            originalPlayerParent = player.transform.parent;

            // Đặt người chơi làm con của thuyền
            player.transform.parent = transform;

            // Di chuyển người chơi đến vị trí ngồi trên thuyền
            if (playerSeatPosition != null)
            {
                player.transform.position = playerSeatPosition.position;
                player.transform.rotation = playerSeatPosition.rotation;
            }

            // Ẩn prompt lái
            if (drivePromptUI != null)
                drivePromptUI.SetActive(false);

            // Hiển thị prompt thoát
            if (exitPromptUI != null)
                exitPromptUI.SetActive(true);
        }
    }

    private void ExitBoat()
    {
        if (player != null && playerController != null)
        {
            isDriving = false;

            if (cameraBoat != null)
                cameraBoat.tag = "Untagged";

            if (cameraPlayer != null)
                cameraPlayer.enabled = true;

            if (cameraBoat != null)
                cameraBoat.enabled = false;

            // Kích hoạt lại ThirdPersonController
            playerController.enabled = true;

            // Đặt người chơi về parent ban đầu
            player.transform.parent = originalPlayerParent;

            // Di chuyển người chơi ra khỏi thuyền (có thể thêm vị trí thoát thuyền)
            player.transform.position = transform.position + transform.right * 2f; // Di chuyển sang phải 2 đơn vị

            // Ẩn prompt thoát
            if (exitPromptUI != null)
                exitPromptUI.SetActive(false);
            if (drivePromptUI != null)
                drivePromptUI.SetActive(false);
        }
    }

    // Added trigger detection methods from BoatController
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isDriving)
        {
            canDrive = true;

            // Hiển thị prompt lái
            if (drivePromptUI != null)
                drivePromptUI.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && !isDriving)
        {
            canDrive = false;

            // Ẩn prompt lái
            if (drivePromptUI != null)
                drivePromptUI.SetActive(false);
            Debug.Log("hi");
        }
    }

    // Khi tắt/hủy script, hủy các tween đang chạy
    private void OnDisable()
    {
        if (steeringTweener != null && steeringTweener.IsActive())
        {
            steeringTweener.Kill();
        }
    }
}