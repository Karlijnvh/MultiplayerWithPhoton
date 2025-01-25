using Photon.Pun;
using UnityEngine;

namespace PV.Multiplayer
{
    public class PlayerController : Movement
    {
        [Header("UI")]
        [Tooltip("Reference to the player's UI.")]
        public PlayerUI playerUI;

        [Header("Camera")]
        [Tooltip("Target Transform for the camera to follow.")]
        public Transform followTarget;

        [Header("Stats")]
        [Tooltip("The player's current health.")]
        public int health = 100;

        // Reference to the PhotonView component for network synchronization.
        internal PhotonView photonView;

        // Manages the player's weapon state and actions.
        private WeaponManager weaponManager;
        // Tracks the name of the last player who dealt damage.
        private string lastAttackerName;

        protected override void Awake()
        {
            base.Awake();

            photonView = GetComponent<PhotonView>();
            weaponManager = GetComponent<WeaponManager>();

            // Assign the PlayerUI component if not already set.
            if (playerUI == null)
            {
                playerUI = GetComponentInChildren<PlayerUI>(true);
            }

            // Disable components if this instance does not belong to the local player.
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

            // Update movement based on player input.
            UpdateMovement();

            if (weaponManager != null)
            {
                // Update weapon-related logic.
                weaponManager.DoUpdate();
            }

            if (playerUI != null)
            {
                // Update the reticle state based on aiming input.
                playerUI.EnableReticle(Input.isAiming);
            }
        }

        /// <summary>
        /// Handles damage taken by the player and updates health and UI.
        /// </summary>
        /// <param name="damage">The amount of damage dealt.</param>
        /// <param name="attackerName">The name of the player who dealt the damage.</param>
        [PunRPC]
        public void TakeDamage(int damage, string attackerName)
        {
            health -= damage; // Reduce the player's health.
            lastAttackerName = attackerName; // Record the attacker's name.

            if (playerUI != null)
            {
                playerUI.SetHealth(health);
            }

            // Handle player death if health drops to zero or below.
            if (health <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            // Reset health for respawning.
            health = 100;

            if (playerUI != null)
            {
                playerUI.SetHealth(health);
            }

            // Log the kill in the game UI.
            GameUIManager.Instance.LogKilled(lastAttackerName, photonView.Owner.NickName);
            // Notify the game manager to respawn the player.
            GameManager.Instance.ReSpawn(this);
        }
    }
}
