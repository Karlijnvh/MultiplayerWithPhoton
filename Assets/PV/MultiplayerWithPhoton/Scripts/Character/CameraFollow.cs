using UnityEngine;
using Cinemachine;

namespace PV.Multiplayer
{
    public class CameraFollow : MonoBehaviour
    {
        public static CameraFollow Instance;

        public InputManager input;
        public CinemachineVirtualCameraBase followCamera;
        public CinemachineVirtualCameraBase aimCamera;
        public float angleUpDown = 20;
        public float lookMultiplier = 1;
        public float rotationSmooth = 0.1f;

        private Transform _followTarget;
        private Vector3 _targetAngle;
        private Quaternion _targetRotation;

        void Awake()
        {
            Instance = this;
            if (input == null)
            {
                input = InputManager.Instance;
            }
        }

        public void Init(PlayerController playerController)
        {
            if (followCamera == null || aimCamera == null)
            {
                Debug.LogError("Follow and Aim cameras are missing.");
                return;
            }

            if (followCamera != null)
            {
                followCamera.Follow = playerController.followTarget;
            }
            if (aimCamera != null)
            {
                aimCamera.Follow = playerController.followTarget;
                _followTarget = playerController.followTarget;
                aimCamera.GetComponent<Cinemachine3rdPersonAim>().AimTargetReticle = playerController.playerUI.GetReticle();
            }
        }

        void Update()
        {
            if (_followTarget == null)
            {
                return;
            }

            if (input.isAiming && followCamera.gameObject.activeSelf)
            {
                followCamera.gameObject.SetActive(false);
                aimCamera.gameObject.SetActive(true);
            }
            else if (!input.isAiming && aimCamera.gameObject.activeSelf)
            {
                aimCamera.gameObject.SetActive(false);
                followCamera.gameObject.SetActive(true);
            }

            _targetAngle.y = input.look.x * lookMultiplier;
            _targetAngle.x = input.isAiming ? 0 : angleUpDown;
            _targetAngle.z = 0;

            if (input.look.x != 0)
            {
                _followTarget.Rotate(Vector3.up, _targetAngle.y, Space.World);
            }
            else
            {
                _targetRotation = Quaternion.Euler(_targetAngle);
                _followTarget.localRotation = Quaternion.Slerp(_followTarget.localRotation, _targetRotation, rotationSmooth * Time.deltaTime);
            }
        }
    }
}
