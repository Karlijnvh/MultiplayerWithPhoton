using UnityEngine;
using Cinemachine;

namespace PV.Multiplayer
{
    public class CameraFollow : MonoBehaviour
    {
        public static CameraFollow Instance;

        [SerializeField] private InputManager _input;
        [SerializeField] private CinemachineVirtualCameraBase followCamera;
        [SerializeField] private CinemachineVirtualCameraBase aimCamera;
        [SerializeField] private float angleUpDown = 20;

        private Transform _followTarget;
        private Vector3 _targetAngle;
        public float lookMultiplier;
        public float rotationSmooth;
        public Vector3 rotation;

        // Start is called before the first frame update
        void Awake()
        {
            Instance = this;
            if (_input == null)
            {
                _input = FindObjectOfType<InputManager>();
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
                aimCamera.GetComponent<Cinemachine3rdPersonAim>().AimTargetReticle = playerController.playerUI.aimReticle;
            }
        }

        // Update is called once per frame
        void LateUpdate()
        {
            if (_followTarget == null)
            {
                return;
            }

            if (_input.isAiming && followCamera.gameObject.activeSelf)
            {
                followCamera.gameObject.SetActive(false);
                aimCamera.gameObject.SetActive(true);
            }
            else if (!_input.isAiming && aimCamera.gameObject.activeSelf)
            {
                aimCamera.gameObject.SetActive(false);
                followCamera.gameObject.SetActive(true);
            }

            _targetAngle.y += _input.look.x * lookMultiplier;

            if (_input.look.x == 0)
            {
                _targetAngle.y = 0;
            }

            var rotQ = Quaternion.AngleAxis(_targetAngle.y, Vector3.up);
            _targetAngle = rotQ.eulerAngles;
            _targetAngle.x = _input.isAiming ? 0 : angleUpDown;
            _targetAngle.z = 0;
            rotQ.eulerAngles = _targetAngle;

            _followTarget.localRotation = Quaternion.Slerp(_followTarget.localRotation, rotQ, Time.deltaTime * rotationSmooth);
        }
    }
}
