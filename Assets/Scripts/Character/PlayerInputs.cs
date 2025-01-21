using UnityEngine;
using UnityEngine.InputSystem;

namespace PV.Multiplayer
{
    public class PlayerInputs : MonoBehaviour
    {
        public Vector3 move;
        public bool jump;

        private InputActions _inputActions;
        private Vector2 _moveInput2D;

        private void Awake()
        {
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
            _inputActions.Player.Jump.performed += c => jump = true;
            _inputActions.Player.Jump.canceled += c => jump = false;
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
