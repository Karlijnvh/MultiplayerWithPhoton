using UnityEngine;
using UnityEngine.InputSystem;

namespace PV.Multiplayer
{
    public class InputManager : MonoBehaviour
    {
        public static InputManager Instance;

        public Vector3 move;
        public Vector2 look;
        public bool jump;
        public bool attack;
        public bool isAiming;

        private InputActions _inputActions;
        private Vector2 _moveInput2D;

        private void Awake()
        {
            Instance = this;
            _inputActions = new();
        }

        private void OnEnable()
        {
            _inputActions.Player.Enable();
        }

        private void Start()
        {
            _inputActions.Player.Move.performed += OnMove;
            _inputActions.Player.Move.canceled += OnMove;
            _inputActions.Player.Look.performed += OnLook;
            _inputActions.Player.Look.canceled += OnLook;
            _inputActions.Player.Jump.performed += c => jump = true;
            _inputActions.Player.Jump.canceled += c => jump = false;
            _inputActions.Player.Aim.performed += c => isAiming = true;
            _inputActions.Player.Aim.canceled += c => isAiming = false;
            _inputActions.Player.Attack.performed += c => attack = true;
            _inputActions.Player.Attack.canceled += c => attack = false;
        }

        private void OnLook(InputAction.CallbackContext context)
        {
            look = context.ReadValue<Vector2>();
        }

        private void OnMove(InputAction.CallbackContext ctx)
        {
            _moveInput2D = ctx.ReadValue<Vector2>();
            move.x = _moveInput2D.x;
            move.z = _moveInput2D.y;
        }

        private void OnDisable()
        {
            _inputActions.Player.Disable();
        }
    }
}
