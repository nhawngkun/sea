using System.Collections;
using Cinemachine;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class AimStateManager : MonoBehaviour
{
    [SerializeField] public MoveStateManager MSM;
    AimBaseState currState;
    public HitFireState Hit = new HitFireState();
    public AimState Aim = new AimState();
    public NormShootState noshoot = new NormShootState();
    public bool Isshoot = false;
    public float xAxis, yAxis;
    [SerializeField] Transform cameraRoot; // Parent object that controls camera rotation
    [SerializeField] Transform cameraTransform; // Actual camera transform
    [SerializeField] float mouseSense = 1f;

    // Camera collision variables
    [SerializeField] LayerMask collisionLayers;
    [SerializeField] float defaultCameraDistance = 3f;
    [SerializeField] float minCameraDistance = 0.5f;
    [SerializeField] float collisionOffset = 0.2f;
    [SerializeField] float smoothSpeed = 10f;
    [HideInInspector] public Animator ani;
    [HideInInspector] public CinemachineVirtualCamera Vcam;

    // Camera references for first and third person views
    [SerializeField] public CinemachineVirtualCamera thirdPersonCamera;
    [SerializeField] public CinemachineVirtualCamera firstPersonCamera;

    // Positions for first person camera
    [SerializeField] public Transform firstPersonCameraPosition;

    // Camera switching properties
    [HideInInspector] public bool isFirstPerson = false;
    [SerializeField] public float cameraSwitchDuration = 0.4f; // Durée de transition en secondes
    private bool isTransitioning = false; // Pour éviter les transitions multiples

    [SerializeField] public Transform aimPos;
    [SerializeField] float aimSpeed = 20f;
    [SerializeField] public LayerMask aimMask;

    // Add shooting-related properties from ThirdPersonShooterController
    [SerializeField] public Transform bulletPrefab;
    [SerializeField] public Transform bulletSpawnPoint;
    [SerializeField] public Transform muzzleFlashPrefab;
    [SerializeField] public Transform hitObjectEffect;
    [SerializeField] public Transform hitLivingEffect;
    [SerializeField] private float recoilAmount = 0.1f;
    [SerializeField] private float recoilDuration = 0.1f;

    [SerializeField] public RigBuilder Ribl;

    void Start()
    {
        MSM = GetComponent<MoveStateManager>();

        Ribl.enabled = false;

        // Initialize both cameras
        if (thirdPersonCamera == null)
        {
            thirdPersonCamera = GetComponentInChildren<CinemachineVirtualCamera>();
        }

        // Make sure first person camera exists
        if (firstPersonCamera == null)
        {
            Debug.LogError("First person camera reference is missing!");
        }

        // Set up initial camera state - completely disable first person camera at start
        Vcam = thirdPersonCamera; // Default to third person camera
        thirdPersonCamera.Priority = 20;
        firstPersonCamera.Priority = 0; // Set priority to 0 to ensure it's completely disabled
        firstPersonCamera.gameObject.SetActive(false); // Completely deactivate the first person camera
        isFirstPerson = false;

        ani = GetComponent<Animator>();

        // If no camera transform assigned, try to find the main camera
        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;

        // Initialize camera position
        currentCameraDistance = defaultCameraDistance;

        // Lock cursor for proper camera control
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        SwitchState(noshoot);
    }

    void Update()
    {
        // Camera rotation input
        xAxis += Input.GetAxisRaw("Mouse X") * mouseSense;
        yAxis -= Input.GetAxisRaw("Mouse Y") * mouseSense;
        yAxis = Mathf.Clamp(yAxis, -80, 80);

        // Update the current state
        currState.UpdateState(this);

        // Update aim position based on raycast
        Vector2 screenCentre = new Vector2(Screen.width / 2, Screen.height / 2);
        Ray ray = Camera.main.ScreenPointToRay(screenCentre);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, aimMask))
        {
            aimPos.position = Vector3.Lerp(aimPos.position, hit.point, aimSpeed * Time.deltaTime);
        }
    }

    private void LateUpdate()
    {
        // Only apply mouse rotation in third-person mode
        if (!isFirstPerson)
        {
            // Apply rotation
            transform.rotation = Quaternion.Euler(0, xAxis, 0); // Rotate player horizontally
            cameraRoot.localRotation = Quaternion.Euler(yAxis, 0, 0); // Tilt camera vertically

            // Handle camera collision only in third person mode
            HandleCameraCollision();
        }
        else
        {
            // In first-person mode, only rotate the player body (not the camera)
            transform.rotation = Quaternion.Euler(0, xAxis, 0);

            // First person camera stays fixed at head position
            if (firstPersonCameraPosition != null)
            {
                firstPersonCamera.transform.position = firstPersonCameraPosition.position;
                firstPersonCamera.transform.rotation = Quaternion.Euler(yAxis, xAxis, 0); // Allow vertical looking
            }
        }
    }

    // Camera collision variables
    private float currentCameraDistance;

    private void HandleCameraCollision()
    {
        // Calculate camera direction and ideal position
        Vector3 cameraDirection = -cameraRoot.forward; // Camera looks backwards from its parent
        Vector3 idealPosition = transform.position; // Start from player position
        idealPosition += Vector3.up * 0.5f; // Optional: Add a height offset

        // Perform the collision check
        RaycastHit hit;
        if (Physics.Raycast(idealPosition, cameraDirection, out hit, defaultCameraDistance, collisionLayers))
        {
            // If collision detected, move camera closer
            currentCameraDistance = Mathf.Lerp(
                currentCameraDistance,
                Mathf.Clamp(hit.distance - collisionOffset, minCameraDistance, defaultCameraDistance),
                Time.deltaTime * smoothSpeed
            );

            // Debug visualization
            Debug.DrawRay(idealPosition, cameraDirection * hit.distance, Color.red);
        }
        else
        {
            // Gradually return to default position when no collision
            currentCameraDistance = Mathf.Lerp(
                currentCameraDistance,
                defaultCameraDistance,
                Time.deltaTime * (smoothSpeed * 0.5f)
            );

            // Debug visualization
            Debug.DrawRay(idealPosition, cameraDirection * defaultCameraDistance, Color.green);
        }

        // Apply the position to camera
        cameraTransform.position = idealPosition + (cameraDirection * currentCameraDistance);
        cameraTransform.LookAt(idealPosition); // Make camera look at the player
    }

    // Modified: Switch to first-person view with transition duration
    public void SwitchToFirstPerson()
    {
        // Only allow switching to first person if in shooting mode and not already transitioning
        if (!Isshoot || isFirstPerson || isTransitioning) return;

        // Start the transition coroutine
        StartCoroutine(TransitionToFirstPerson());
    }

    // Modified: Switch back to third-person view with transition duration
    public void SwitchToThirdPerson()
    {
        if (!isFirstPerson || isTransitioning) return; // Already in third person or transitioning

        // Start the transition coroutine
        StartCoroutine(TransitionToThirdPerson());
    }

    // New coroutine for transitioning to first person camera
    private IEnumerator TransitionToFirstPerson()
    {
        isTransitioning = true;
        
        // Make sure both cameras are active during transition
        firstPersonCamera.gameObject.SetActive(true);
        thirdPersonCamera.gameObject.SetActive(true);
        
        // Start with third person camera having priority
        thirdPersonCamera.Priority = 20;
        firstPersonCamera.Priority = 10;
        
        // Gradually increase first person camera priority and decrease third person
        float elapsed = 0f;
        
        while (elapsed < cameraSwitchDuration)
        {
            float t = elapsed / cameraSwitchDuration;
            
            // Update camera priorities for smooth transition
            firstPersonCamera.Priority = (int)Mathf.Lerp(10, 30, t);
            thirdPersonCamera.Priority = (int)Mathf.Lerp(20, 0, t);
            
            // Update first person camera position
            if (firstPersonCameraPosition != null)
            {
                firstPersonCamera.transform.position = firstPersonCameraPosition.position;
                firstPersonCamera.transform.rotation = Quaternion.Euler(yAxis, xAxis, 0);
            }
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Ensure the transition completes with correct final state
        firstPersonCamera.Priority = 30;
        thirdPersonCamera.Priority = 0;
        thirdPersonCamera.gameObject.SetActive(false);
        
        // Set state flags
        isFirstPerson = true;
        Vcam = firstPersonCamera;
        isTransitioning = false;
    }

    // New coroutine for transitioning to third person camera
    private IEnumerator TransitionToThirdPerson()
    {
        isTransitioning = true;
        
        // Make sure both cameras are active during transition
        firstPersonCamera.gameObject.SetActive(true);
        thirdPersonCamera.gameObject.SetActive(true);
        
        // Start with first person camera having priority
        firstPersonCamera.Priority = 30;
        thirdPersonCamera.Priority = 10;
        
        // Gradually increase third person camera priority and decrease first person
        float elapsed = 0f;
        
        while (elapsed < cameraSwitchDuration)
        {
            float t = elapsed / cameraSwitchDuration;
            
            // Update camera priorities for smooth transition
            thirdPersonCamera.Priority = (int)Mathf.Lerp(10, 30, t);
            firstPersonCamera.Priority = (int)Mathf.Lerp(30, 0, t);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Ensure the transition completes with correct final state
        thirdPersonCamera.Priority = 30;
        firstPersonCamera.Priority = 0;
        firstPersonCamera.gameObject.SetActive(false);
        
        // Set state flags
        isFirstPerson = false;
        Vcam = thirdPersonCamera;
        isTransitioning = false;
    }

    public void SwitchState(AimBaseState state)
    {
        currState = state;
        currState.EnterState(this);
    }

    // Camera recoil effect for shooting
    public IEnumerator CameraRecoil()
    {
        // Store original vertical angle
        float originalYAxis = yAxis;

        // Apply recoil (upward kick)
        yAxis -= recoilAmount;

        // Wait for recoil duration
        yield return new WaitForSeconds(recoilDuration);

        // Gradually return to original position
        float elapsed = 0f;
        float returnDuration = recoilDuration * 2f; // Return more slowly

        while (elapsed < returnDuration)
        {
            yAxis = Mathf.Lerp(yAxis, originalYAxis, elapsed / returnDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Ensure exact return to original position
        yAxis = originalYAxis;
    }
}