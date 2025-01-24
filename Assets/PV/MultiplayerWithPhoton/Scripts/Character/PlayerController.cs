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

        internal PhotonView photonView;

        private WeaponManager weaponManager;
        private string lastAttackerName;

        protected override void Awake()
        {
            base.Awake();

            photonView = GetComponent<PhotonView>();
            weaponManager = GetComponent<WeaponManager>();

            if (playerUI == null)
            {
                playerUI = GetComponentInChildren<PlayerUI>(true);
            }

            if (photonView != null && !photonView.IsMine)
            {
                enabled = false;
                if (playerUI != null)
                {
                    playerUI.enabled = false;
                    playerUI.gameObject.SetActive(false);
                }
            }
        }

        private void FixedUpdate()
        {
            if (photonView == null)
            {
                return;
            }

            UpdateMovement();

            if (weaponManager != null)
            {
                weaponManager.DoUpdate();
            }

            if (playerUI != null)
            {
                playerUI.EnableReticle(Input.isAiming);
            }
        }

        [PunRPC]
        public void TakeDamage(int damage, string attackerName)
        {
            health -= damage;
            lastAttackerName = attackerName;

            if (playerUI != null)
            {
                playerUI.SetHealth(health);
            }

            if (health <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            health = 100;

            if (playerUI != null)
            {
                playerUI.SetHealth(health);
            }

            GameUIManager.Instance.Log($"{lastAttackerName} killed {photonView.Owner.NickName}");
            GameManager.Instance.ReSpawn(this);
        }
    }
}
