using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class MoveStateManager : MonoBehaviour
{
    
    public Vector3 dir;
    public float HzInput;
    public float VInput;
    public float currmoveSpeed = 4f;
    public float currwalkSpeed = 4f, walkbackSpeed = 2f;
    public float currrunSpeed = 6f, runbackSpeed = 3f;
    public float currcrouchSpeed = 3f, crouchbackSpeed = 1.5f;

    // Jump properties
    public float jumpForce = 5f;

    // Swimming properties
    public float swimmingSpeed = 3.0f;
    public float sinkSpeed = 2.0f;
    public bool IsInWater = false;
    public bool IsUnderwater = false;

    [Header("Water Detection")]
    [SerializeField] Collider WaterDetector;
    [SerializeField] Collider HeadDetector;
    [SerializeField] LayerMask WaterLayers;

    CharacterController charControl;
    [SerializeField] float groundYOffset;
    [SerializeField] LayerMask groundlaymash;
    Vector3 spherePos;
    [SerializeField] public float Gravity = 10f;  // Changed to public for state access
    public Vector3 velocity;

    // Flag to determine if states should control gravity
    private bool stateControlsGravity = false;

    // States
    MoveBaseState currState;
    public IdleState idle = new IdleState();
    public WalkingState Walk = new WalkingState();
    public RunningState run = new RunningState();
    public CrouchingState crouch = new CrouchingState();
    public JumpingState jumping = new JumpingState(); // Trạng thái nhảy cải tiến

    // New water states
    public SwimmingState swimming = new SwimmingState();
    public FloatingState floating = new FloatingState();
    public DivingState diving = new DivingState();

    [HideInInspector] public Animator ani;

    void Start()
    {
        
        charControl = GetComponent<CharacterController>();
        SwitchState(idle);
        ani = GetComponent<Animator>();

        // Initialize water detectors if not assigned
        if (WaterDetector == null)
        {
            GameObject waterDetectorObj = new GameObject("WaterDetector");
            waterDetectorObj.transform.parent = transform;
            waterDetectorObj.transform.localPosition = new Vector3(0, 1.0f, 0); // Chest height
            WaterDetector = waterDetectorObj.AddComponent<SphereCollider>();
            ((SphereCollider)WaterDetector).radius = 0.1f;
            WaterDetector.isTrigger = true;
        }

        if (HeadDetector == null)
        {
            GameObject headDetectorObj = new GameObject("HeadDetector");
            headDetectorObj.transform.parent = transform;
            headDetectorObj.transform.localPosition = new Vector3(0, 1.7f, 0); // Head height
            HeadDetector = headDetectorObj.AddComponent<SphereCollider>();
            ((SphereCollider)HeadDetector).radius = 0.1f;
            HeadDetector.isTrigger = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Check for water before other movement calculations
        WaterCheck();

        DirMove();

        // Let the state update first (potentially modifying gravity)
        currState.UpdetaState(this);

        // Then apply gravity physics
        iSGravity();

        ani.SetFloat("hzinput", HzInput);
        ani.SetFloat("vinput", VInput);
    }

    public void SwitchState(MoveBaseState state)
    {
        currState = state;
        currState.EnterState(this);

        // Enable state-controlled gravity for water states
        stateControlsGravity = (state is SwimmingState || state is FloatingState || state is DivingState);
    }

    public void DirMove()
    {
        HzInput = Input.GetAxis("Horizontal");
        VInput = Input.GetAxis("Vertical");
        dir = transform.forward * VInput + transform.right * HzInput;
        charControl.Move(dir.normalized * currmoveSpeed * Time.deltaTime);
    }

    public bool IsGround()
    {
        spherePos = new Vector3(transform.position.x, transform.position.y - groundYOffset, transform.position.z);
        if (Physics.CheckSphere(spherePos, charControl.radius - 0.05f, groundlaymash))
        { return true; }
        else { return false; }
    }

    void iSGravity()
    {
        // Only set default gravity values if the current state isn't controlling gravity
        if (!stateControlsGravity)
        {
            if (IsUnderwater)
            {
                if (IsInWater)
                {
                    Gravity = 0.5f;
                }
                else
                {
                    Gravity = 0f;
                }
            }
            else
            {
                // Normal falling gravity when not in water
                Gravity = -10f;
            }
        }

        // Apply the calculated gravity
        if (!IsGround())
        {
            velocity.y += Gravity * Time.deltaTime;
        }
        else
        {
            // NEW CODE HERE: Check if underwater before stopping vertical movement
            if (IsInWater)
            {
                // If underwater on seafloor, allow buoyancy to affect the player
                // Keep applying small upward force to gently float off the seafloor
                if (velocity.y < 0)
                {
                    // Reset downward velocity and add a small upward push
                    velocity.y = 0.5f;
                }
            }
            else if (velocity.y < 0)
            {
                // Only on land, set the standard ground velocity
                velocity.y = -2f;
            }
        }

        // Handle surface floating logic
        if (IsInWater && !IsUnderwater && velocity.y > 0)
        {
            // If head is above water and rising, stop rising
            velocity.y = 0;
        }

        charControl.Move(new Vector3(0, velocity.y, 0) * Time.deltaTime);
    }

    private void WaterCheck()
    {
        // Check if character is in water
        IsInWater = Physics.CheckSphere(WaterDetector.bounds.center, 0.1f, WaterLayers, QueryTriggerInteraction.Collide);

        // Check if head is underwater
        IsUnderwater = Physics.CheckSphere(HeadDetector.bounds.center, 0.1f, WaterLayers, QueryTriggerInteraction.Collide);

        // Auto-transition to swimming state when entering water
        // IMPORTANT: We now explicitly check for JumpingState to ensure it transitions properly
        if (IsInWater && !(currState is SwimmingState || currState is FloatingState || currState is DivingState))
        {
            // Let JumpingState handle its own transitions - it already has water detection logic
            if (currState is JumpingState)
            {
                // Do nothing, let JumpingState handle water transitions
                return;
            }

            // For all other states, handle water transitions here
            if (Input.GetKey(KeyCode.Q))
            {
                SwitchState(diving);
            }
            else if (dir.magnitude > 0.1f)
            {
                // Swim if moving in water
                SwitchState(swimming);
            }
            else
            {
                // Float if just standing in water
                SwitchState(floating);
            }
        }

        // Also transition to swimming while underwater if currently floating and moving
        if (IsUnderwater && currState is FloatingState && dir.magnitude > 0.1f)
        {
            SwitchState(swimming);
        }
    }

    //void OnDrawGizmos()
    //{
    //    // Draw ground check sphere
    //    Gizmos.color = Color.red;
    //    Gizmos.DrawWireSphere(spherePos, charControl.radius - 0.05f);

    //    // Draw water detector spheres if they exist
    //    if (WaterDetector != null)
    //    {
    //        Gizmos.color = Color.blue;
    //        Gizmos.DrawWireSphere(WaterDetector.bounds.center, 0.1f);
    //    }

    //    if (HeadDetector != null)
    //    {
    //        Gizmos.color = Color.cyan;
    //        Gizmos.DrawWireSphere(HeadDetector.bounds.center, 0.1f);
    //    }
    //}
}