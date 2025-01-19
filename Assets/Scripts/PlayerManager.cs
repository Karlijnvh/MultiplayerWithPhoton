using Cinemachine;
using Photon.Pun;
using UnityEngine;

namespace PV.Multiplayer
{
    public class PlayerManager : MonoBehaviour
    {
        private PlayerMovement _playerMovement;
        private PhotonView _photonView;

        private void Awake()
        {
            _playerMovement = GetComponent<PlayerMovement>();
            _photonView = GetComponent<PhotonView>();
        }

        private void FixedUpdate()
        {
            if (_photonView == null || !_photonView.IsMine)
            {
                return;
            }

            if (_playerMovement != null)
            {
                _playerMovement.UpdateMovement();
            }
        }
    }
}
