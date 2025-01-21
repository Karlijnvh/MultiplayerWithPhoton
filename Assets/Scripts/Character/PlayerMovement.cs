using UnityEngine;

namespace PV.Multiplayer
{
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Movement")]
        [Tooltip("Speed of player when moving.")]
        public float moveSpeed = 3;

        [Tooltip("How fast the player turns to face move direction.")]
        public float rotationSmoothRate = 15;

        [Tooltip("How fast the player changes speed.")]
        public float speedChangeRate = 10;

        [Header("Jump")]
        [Tooltip("If the player is jumping right now.")]
        public bool isJumping;

        [Tooltip("The height that player can jump.")]
        public float jumpHeight = 1.5f;

        [Tooltip("Custom gravity used by player for jump calculation. Actual gravity is -9.81f.")]
        public float customGravity = -15f;

        [Header("Grounded")]
        [Tooltip("If the player is grounded or not.")]
        public bool isGrounded;

        [Tooltip("Radius of sphere which is used in ground detection. Make it slightly lower than capsule collider's radius.")]
        public float groundCheckRadius = 0.48f;

        [Tooltip("Offset of ground check, useful when ground is rough.")]
        public float groundOffset = 0.65f;

        [Tooltip("Layer to check ground.")]
        public LayerMask groundLayer;

        private Rigidbody _rigid;
        private PlayerInputs _input;
        private PlayerManager _manager;
        private Transform _cameraTransform;

        private float _moveSpeed;
        private float _currentMoveSpeed;
        private float _vertVel;

        private Vector3 _currentMoveVelocity;
        private Vector3 _moveDirection;
        private Vector3 _targetDirection;
        private Vector3 _verticalVelocity;
        private Vector3 _spherePosition;
        private Quaternion _targetRotation;
        private Quaternion _playerRotation;

        private void Awake()
        {
            _rigid = GetComponent<Rigidbody>();
            _input = GetComponent<PlayerInputs>();
            _manager = GetComponent<PlayerManager>();
            _cameraTransform = Camera.main.transform;
        }

        public void UpdateMovement()
        {
            CheckGrounded();
            HandleMovement();
            HandleRotation();
            HandleJump();
        }

        private void CheckGrounded()
        {
            // Getting sphere position with some offset so ground check can be detected accurately.
            _spherePosition = transform.position;
            _spherePosition.y -= groundOffset;
            // Casting rays in down direction to check if it hits ground.
            isGrounded = Physics.CheckSphere(_spherePosition, groundCheckRadius, groundLayer, QueryTriggerInteraction.Ignore);
        }

        private void HandleMovement()
        {
            // Deciding the move speed of player.
            _moveSpeed = moveSpeed;

            if (_input.move == Vector3.zero)
            {
                _moveSpeed = 0;
            }

            // Calculating move direction based on camera forward and player input.
            _moveDirection = _cameraTransform.forward * _input.move.z;
            _moveDirection += _cameraTransform.right * _input.move.x;
            _moveDirection.y = 0;
            _moveDirection.Normalize();

            // Calculating the current speed of the player.
            _currentMoveVelocity = _rigid.velocity;
            _currentMoveVelocity.y = 0;
            _currentMoveSpeed = _currentMoveVelocity.magnitude;

            if (_currentMoveSpeed < _moveSpeed - 0.1f || _currentMoveSpeed > _moveSpeed + 0.1f)
            {
                _moveSpeed = Mathf.Lerp(_currentMoveSpeed, _moveSpeed, speedChangeRate * Time.deltaTime);
            }

            _moveDirection *= _moveSpeed;

            // Applying direction velocity.
            _rigid.velocity = _moveDirection;
        }

        private void HandleRotation()
        {
            // Calculating move direction based on camera forward and player input
            _targetDirection = _cameraTransform.forward * _input.move.z;
            _targetDirection += _cameraTransform.right * _input.move.x;
            _targetDirection.Normalize();

            // Checking if target direction is zero, then applying forward direction
            if (_targetDirection == Vector3.zero)
            {
                _targetDirection = transform.forward;
            }

            // Getting rotation from direction
            _targetRotation = Quaternion.LookRotation(_targetDirection);
            // Resetting x and z axis, So player rotates only in y-axis
            _targetRotation.x = 0;
            _targetRotation.z = 0;

            // Smoothing the transition effect from transform's rotation to target rotation
            _playerRotation = Quaternion.Slerp(transform.rotation, _targetRotation, rotationSmoothRate * Time.fixedDeltaTime);
            transform.rotation = _playerRotation;
        }

        private void HandleJump()
        {
            if (isGrounded)
            {
                isJumping = false;

                // Reset vertical velocity
                if (_vertVel < 0)
                {
                    _vertVel = -3;
                }

                // Checking jump input
                if (_input.jump)
                {
                    isJumping = true;

                    // Applying move direction so new velocity does
                    // not modify the direction player is going
                    _verticalVelocity = _moveDirection;

                    // Calculating square root of (-2 * jump height * gravity)
                    // to get velocity needed to jump to the specified height
                    _vertVel = Mathf.Sqrt(-2 * jumpHeight * customGravity);
                    _verticalVelocity.y = _vertVel;

                    // Applying velocity
                    _rigid.velocity = _verticalVelocity;
                }
            }
            else
            {

                // Set vertical velocity to original velocity
                _verticalVelocity = _rigid.velocity;

                // Keep increase gravity if it does not reach its limit.
                // Limit is useful to stop it increasing infinitely.
                if (_vertVel > -50)
                {
                    // Gradually increasing gravity
                    _vertVel += customGravity * Time.deltaTime;
                }

                // Set vertical velocity on y axis
                _verticalVelocity.y = _vertVel;
                _rigid.velocity = _verticalVelocity;
            }
        }
    }
}
