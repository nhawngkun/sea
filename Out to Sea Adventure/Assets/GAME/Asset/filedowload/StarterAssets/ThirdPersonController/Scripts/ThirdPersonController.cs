using UnityEngine;
#if ENABLE_INPUT_SYSTEM 
using UnityEngine.InputSystem;
#endif

/* Note: animations are called via the controller for both the character and capsule using animator null checks
 */

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM 
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class ThirdPersonController : MonoBehaviour
    {
        [Header("Player")]
        [Tooltip("Move speed of the character in m/s")]
        public float MoveSpeed = 2.0f;

        [Tooltip("Sprint speed of the character in m/s")]
        public float SprintSpeed = 5.335f;

        [Tooltip("How fast the character turns to face movement direction")]
        [Range(0.0f, 0.3f)]
        public float RotationSmoothTime = 0.12f;

        [Tooltip("Acceleration and deceleration")]
        public float SpeedChangeRate = 10.0f;
        public float Sensitivity = 1f;

        public AudioClip LandingAudioClip;
        public AudioClip[] FootstepAudioClips;
        [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

        [Space(10)]
        [Tooltip("The height the player can jump")]
        public float JumpHeight = 1.2f;

        [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
        public float Gravity = -15.0f;

        [Space(10)]
        [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
        public float JumpTimeout = 0.50f;

        [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
        public float FallTimeout = 0.15f;

        [Header("Player Grounded")]
        [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
        public bool Grounded = true;

        [Tooltip("Useful for rough ground")]
        public float GroundedOffset = -0.14f;

        [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
        public float GroundedRadius = 0.28f;

        [Tooltip("What layers the character uses as ground")]
        public LayerMask GroundLayers;

        [Header("Cinemachine")]
        [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
        public GameObject CinemachineCameraTarget;

        [Tooltip("How far in degrees can you move the camera up")]
        public float TopClamp = 70.0f;

        [Tooltip("How far in degrees can you move the camera down")]
        public float BottomClamp = -30.0f;

        [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
        public float CameraAngleOverride = 0.0f;

        [Tooltip("For locking the camera position on all axis")]
        public bool LockCameraPosition = false;

        [Header("Swimming")]

        public bool IsInWater = false;

        [Tooltip("Kiểm tra đầu của player có đang ở dưới nước")]
        public bool IsUnderwater = false;

        [Tooltip("Collider phát hiện nước gần cổ")]
        public Collider WaterDetector;

        [Tooltip("Collider phát hiện đầu dưới nước")]
        public Collider HeadDetector;

        [Tooltip("Tốc độ bơi của nhân vật (m/s)")]
        public float SwimSpeed = 3.0f;

        [Tooltip("Tốc độ chìm khi nhấn phím Q")]
        public float SinkSpeed = 2.0f;

        [Tooltip("Layers đại diện cho nước")]
        public LayerMask WaterLayers;

        // Biến để kiểm tra xem người chơi có đang nhấn K không
        private bool _sinking = false;

        // Biến để kiểm tra xem người chơi có đang nhấn H không
        private bool _rising = false;

        // ID hoạt ảnh cho bơi
        private int _animIDSwimming;
        private int _animIDFloating;

        // cinemachine
        private float _cinemachineTargetYaw;
        private float _cinemachineTargetPitch;

        // player
        private float _speed;
        private float _animationBlend;
        private float _targetRotation = 0.0f;
        private float _rotationVelocity;
        private float _verticalVelocity;
        private float _terminalVelocity = 53.0f;

        // timeout deltatime
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;

        // animation IDs
        private int _animIDSpeed;
        private int _animIDGrounded;
        private int _animIDJump;
        private int _animIDFreeFall;
        private int _animIDMotionSpeed;

#if ENABLE_INPUT_SYSTEM 
        private PlayerInput _playerInput;
#endif
        private Animator _animator;
        private CharacterController _controller;
        private StarterAssetsInputs _input;
        private GameObject _mainCamera;
        private bool _rotateOnMove = true;

        private const float _threshold = 0.01f;

        private bool _hasAnimator;

        private bool IsCurrentDeviceMouse
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                return _playerInput.currentControlScheme == "KeyboardMouse";
#else
				return false;
#endif
            }
        }


        private void Awake()
        {
            // get a reference to our main camera
            if (_mainCamera == null)
            {
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }
        }

        private void Start()
        {
            _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;

            _hasAnimator = TryGetComponent(out _animator);
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<StarterAssetsInputs>();
#if ENABLE_INPUT_SYSTEM 
            _playerInput = GetComponent<PlayerInput>();
#else
			Debug.LogError( "Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif

            AssignAnimationIDs();

            // reset our timeouts on start
            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;
        }

        private void Update()
        {
            _hasAnimator = TryGetComponent(out _animator);

            // Kiểm tra nước trước khi tính toán chuyển động khác
            WaterCheck();

            // Kiểm tra nút K để chìm và nút H để nổi
            CheckSwimmingInputs();

            // Điều chỉnh trọng lực dựa trên trạng thái nước
            UpdateGravity();

            // Kiểm tra grounded status luôn được thực hiện
            GroundedCheck();

            // Vô hiệu hóa khả năng nhảy khi ở trong nước hoặc dưới nước
            if (IsInWater || IsUnderwater)
            {
                _input.jump = false;
            }

            // Chỉ thực hiện chuyển động bình thường nếu không bơi
            if (!IsInWater)
            {
                JumpAndGravity();
                Move();
            }
            else
            {
                Swim();
                // Đảm bảo FreeFall tắt khi bơi
                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDFreeFall, false);
                    _animator.SetBool(_animIDJump, false);
                }
            }
        }

        private void CheckSwimmingInputs()
        {
            // Kiểm tra phím K có được nhấn không (để chìm)
            _sinking = Input.GetKey(KeyCode.Q) && IsInWater;

            // Kiểm tra phím H có được nhấn không (để nổi lên) - chỉ hoạt động khi đầu ở dưới nước
            _rising = Input.GetKey(KeyCode.E) && IsUnderwater;
        }

        private void WaterCheck()
        {
            // Check if the water detector is colliding with water
            IsInWater = Physics.CheckSphere(WaterDetector.bounds.center, 0.1f, WaterLayers, QueryTriggerInteraction.Collide);

            // Check if the head is underwater
            IsUnderwater = Physics.CheckSphere(HeadDetector.bounds.center, 0.1f, WaterLayers, QueryTriggerInteraction.Collide);

            // Update swimming animation states
            UpdateSwimAnimation();
        }

        private void UpdateSwimAnimation()
        {
            if (_hasAnimator)
            {
                if (IsInWater)
                {
                    // Get movement input magnitude to check if player is moving
                    bool isMoving = _input.move.magnitude > 0.1f;

                    // Use _sinking flag for diving animation or _rising for rising animation
                    if (_sinking || _rising || isMoving)
                    {
                        // When sinking, rising, or moving, use swimming animation
                        _animator.SetBool(_animIDSwimming, true);
                        _animator.SetBool(_animIDFloating, false);
                    }
                    else
                    {
                        // When stationary in water, use floating animation
                        _animator.SetBool(_animIDSwimming, false);
                        _animator.SetBool(_animIDFloating, true);
                    }

                    // Disable other animations when in water
                    _animator.SetBool(_animIDFreeFall, false);
                    _animator.SetBool(_animIDJump, false);

                    // Reset ground movement speed parameter
                    _animator.SetFloat(_animIDSpeed, 0);
                }
                else
                {
                    // Reset swim animations when on land
                    _animator.SetBool(_animIDSwimming, false);
                    _animator.SetBool(_animIDFloating, false);
                }
            }
        }
        private void UpdateGravity()
        {
            if (IsUnderwater)
            {
                // Đầu dưới nước
                if (_sinking)
                {
                    // Khi đang nhấn K, lực kéo xuống
                    Gravity = -1.0f;
                }
                else if (_rising)
                {
                    // Khi đang nhấn H, lực đẩy lên mạnh
                    Gravity = 1.0f;
                }
                else
                {
                    // Khi đầu dưới nước và không có input, mặc định nổi nhẹ
                    Gravity = 0.5f;
                }
            }
            else if (IsInWater)
            {
                // Trong nước nhưng đầu không chìm
                if (_sinking)
                {
                    // Khi đang nhấn K, tạo lực kéo xuống
                    Gravity = -1.0f;
                }
                else
                {
                    // Khi đang ở trong nước và không nhấn K, không có trọng lực
                    Gravity = 0f;
                }
                // Nút H không hoạt động khi đầu không ở dưới nước
            }
            else
            {
                // Trên đất - trọng lực bình thường
                Gravity = -15.0f;
            }
        }

        private void Swim()
        {
            // Get movement direction relative to camera
            Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

            if (_input.move != Vector2.zero)
            {
                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                                  _mainCamera.transform.eulerAngles.y;
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                    RotationSmoothTime);

                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }

            Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

            // Calculate vertical movement
            float verticalMovement = 0.0f;

            // If sinking, apply downward force
            if (_sinking)
            {
                verticalMovement = -SinkSpeed;
            }
            // If rising, apply upward force
            else if (_rising)
            {
                verticalMovement = SinkSpeed; // Using same speed value for consistency
            }

            // Set movement speed based on swimming
            float targetSpeed = SwimSpeed;

            // Calculate speed and blend
            float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

            // Apply speed only when moving or sinking or rising
            if (_input.move.magnitude > 0.1f || _sinking || _rising)
            {
                _speed = Mathf.Lerp(_speed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);
            }
            else
            {
                // When not moving, slowly reduce speed
                _speed = Mathf.Lerp(_speed, 0, Time.deltaTime * SpeedChangeRate);
            }

            // Calculate movement vector
            Vector3 movement;

            if (_sinking || _rising)
            {
                // If sinking or rising, prioritize vertical movement
                movement = (targetDirection.normalized * (_speed * Time.deltaTime) * 0.5f) +
                           (Vector3.up * verticalMovement * Time.deltaTime) +
                           (Vector3.up * Gravity * Time.deltaTime);
            }
            else if (_input.move.magnitude > 0.1f)
            {
                // Normal swimming movement when input is detected
                movement = (targetDirection.normalized * (_speed * Time.deltaTime)) +
                           (Vector3.up * verticalMovement * Time.deltaTime) +
                           (Vector3.up * Gravity * Time.deltaTime);
            }
            else
            {
                // When floating, only apply gravity/buoyancy
                movement = Vector3.up * Gravity * Time.deltaTime;
            }

            _controller.Move(movement);

            // Update animator
            if (_hasAnimator)
            {
                _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
                _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
            }
        }

        private void LateUpdate()
        {
            CameraRotation();
        }

        private void AssignAnimationIDs()
        {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDJump = Animator.StringToHash("Jump");
            _animIDFreeFall = Animator.StringToHash("FreeFall");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
            _animIDSwimming = Animator.StringToHash("Swimming");
            _animIDFloating = Animator.StringToHash("FloatingWater");
        }

        private void GroundedCheck()
        {
            // set sphere position, with offset
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
                transform.position.z);
            Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
                QueryTriggerInteraction.Ignore);

            // update animator if using character
            if (_hasAnimator)
            {
                _animator.SetBool(_animIDGrounded, Grounded);
            }
        }

        private void CameraRotation()
        {
            // if there is an input and camera position is not fixed
            if (_input.look.sqrMagnitude >= _threshold && !LockCameraPosition)
            {
                //Don't multiply mouse input by Time.deltaTime;
                float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

                _cinemachineTargetYaw += _input.look.x * deltaTimeMultiplier*Sensitivity;
                _cinemachineTargetPitch += _input.look.y * deltaTimeMultiplier*Sensitivity;
            }

            // clamp our rotations so our values are limited 360 degrees
            _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

            // Cinemachine will follow this target
            CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride,
                _cinemachineTargetYaw, 0.0f);
        }

        private void Move()
        {
            // set target speed based on move speed, sprint speed and if sprint is pressed
            float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;

            // a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

            // note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is no input, set the target speed to 0
            if (_input.move == Vector2.zero) targetSpeed = 0.0f;

            // a reference to the players current horizontal velocity
            float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

            float speedOffset = 0.1f;
            float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

            // accelerate or decelerate to target speed
            if (currentHorizontalSpeed < targetSpeed - speedOffset ||
                currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                // creates curved result rather than a linear one giving a more organic speed change
                // note T in Lerp is clamped, so we don't need to clamp our speed
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                    Time.deltaTime * SpeedChangeRate);

                // round speed to 3 decimal places
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
            if (_animationBlend < 0.01f) _animationBlend = 0f;

            // normalise input direction
            Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

            // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is a move input rotate player when the player is moving
            if (_input.move != Vector2.zero)
            {
                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                                  _mainCamera.transform.eulerAngles.y;
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                    RotationSmoothTime);
                if (_rotateOnMove)
                {
                    transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
                }

                // rotate to face input direction relative to camera position
                
            }


            Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

            // move the player
            _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) +
                             new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

            // update animator if using character
            if (_hasAnimator)
            {
                _animator.SetFloat(_animIDSpeed, _animationBlend);
                _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
            }
        }

        private void JumpAndGravity()
        {
            if (Grounded)
            {
                // reset the fall timeout timer
                _fallTimeoutDelta = FallTimeout;

                // update animator if using character
                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDJump, false);
                    _animator.SetBool(_animIDFreeFall, false);
                }

                // stop our velocity dropping infinitely when grounded
                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -2f;
                }

                // Jump
                if (_input.jump && _jumpTimeoutDelta <= 0.0f)
                {
                    // the square root of H * -2 * G = how much velocity needed to reach desired height
                    _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

                    // update animator if using character
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDJump, true);
                    }
                }

                // jump timeout
                if (_jumpTimeoutDelta >= 0.0f)
                {
                    _jumpTimeoutDelta -= Time.deltaTime;
                }
            }
            else
            {
                // reset the jump timeout timer
                _jumpTimeoutDelta = JumpTimeout;

                // fall timeout
                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -= Time.deltaTime;
                }
                else
                {
                    // update animator if using character
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDFreeFall, true);
                    }
                }

                // if we are not grounded, do not jump
                _input.jump = false;
            }

            // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
            if (_verticalVelocity < _terminalVelocity)
            {
                _verticalVelocity += Gravity * Time.deltaTime;
            }
        }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        private void OnDrawGizmosSelected()
        {
            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            if (Grounded) Gizmos.color = transparentGreen;
            else Gizmos.color = transparentRed;

            // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
            Gizmos.DrawSphere(
                new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
                GroundedRadius);
        }
        public void SetSensitivity(float newSensitivity)
        {
            Sensitivity = newSensitivity;
        }
        public void SetRotateOnMove(bool newRotateOnMove)
        {
            _rotateOnMove = newRotateOnMove;
        }

        private void OnFootstep(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                if (FootstepAudioClips.Length > 0)
                {
                    var index = Random.Range(0, FootstepAudioClips.Length);
                    AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(_controller.center), FootstepAudioVolume);
                }
            }
        }

        private void OnLand(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(_controller.center), FootstepAudioVolume);
            }
        }
    }
}