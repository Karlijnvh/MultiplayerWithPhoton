using Photon.Pun;
using UnityEngine;

namespace PV.Multiplayer
{
    public class PlayerController : Movement
    {
        [Header("UI")]
        public PlayerUI playerUI;

        [Header("Camera")]
        public Transform followTarget;

        [Header("Stats")]
        public bool isDead = false;
        public int health = 100;

        private PhotonView _photonView;

        protected override void Awake()
        {
            base.Awake();

            _photonView = GetComponent<PhotonView>();

            if (playerUI == null)
            {
                playerUI = GetComponentInChildren<PlayerUI>(true);
            }
        }

        private void FixedUpdate()
        {
            if (isDead || _photonView == null || !_photonView.IsMine)
            {
                return;
            }

            UpdateMovement();

            if (playerUI != null)
            {
                playerUI.EnableReticle(_input.isAiming);
            }
        }

        [PunRPC]
        public void TakeDamage(int damage)
        {
            health -= damage;

            if (playerUI != null)
            {
                playerUI.SetHealth(health);
            }

            if (health <= 0)
            {
                isDead = true;
            }
        }
    }
}
