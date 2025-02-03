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
        // Used to store the player stats (eg. kills, deaths, score, etc.).
        internal Stats stats;

        // Manages the player's weapon state and actions.
        private WeaponManager _weaponManager;
        // Tracks the name of the last player who dealt damage.
        private PhotonView _lastAttacker;
        // Tracks the last attacker id.
        private int _lastAttackerID;

        protected override void Awake()
        {
            base.Awake();

            photonView = GetComponent<PhotonView>();
            _weaponManager = GetComponent<WeaponManager>();

            // Assign the PlayerUI component if not already set.
            if (playerUI == null)
            {
                playerUI = GetComponentInChildren<PlayerUI>(true);
            }

            // Disable components if this instance does not belong to the local player.
            if (!photonView.IsMine)
            {
                enabled = false;
                if (playerUI != null)
                {
                    playerUI.enabled = false;
                    playerUI.gameObject.SetActive(false);
                }
            }

            // Initialize stats and store data 
            stats = new(photonView.Owner.ActorNumber);
            GameUIManager.Instance.SetStats(this);
        }

        private void FixedUpdate()
        {
            if (photonView == null)
            {
                return;
            }

            // Update movement based on player input.
            UpdateMovement();

            if (_weaponManager != null)
            {
                // Update weapon-related logic.
                _weaponManager.DoUpdate();
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
        public void TakeDamage(int damage, int attackerID)
        {
            health -= damage; // Reduce the player's health.

            // Change last attacker on first time or when attacker changes.
            if (_lastAttacker == null || _lastAttackerID != attackerID)
            {
                _lastAttackerID = attackerID;
                _lastAttacker = PhotonView.Find(attackerID); // Record the last attacker.
            }

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

            if (_lastAttacker.TryGetComponent(out PlayerController attacker))
            {
                attacker.stats.AddKill();
            }
            else
            {
                Debug.LogError("Last attacker does not have PlayerController!");
            }

            stats.AddDeaths();

            // Log the kill in the game UI.
            GameUIManager.Instance.LogKilled(_lastAttacker.Owner.NickName, photonView.Owner.NickName);
            // Notify the game manager to respawn the player.
            GameManager.Instance.ReSpawn(this);
        }
    }

    [System.Serializable]
    public class Stats
    {
        public const string KillsKey = "Kills";
        public const string DeathsKey = "Deaths";
        public const string ScoreKey = "Score";

        public int Kills { get; private set; }
        public int Deaths { get; private set; }
        public int Score { get; private set; }

        private int _playerNumber = -1;

        public Stats(int playerNumber)
        {
            _playerNumber = playerNumber;
            Kills = 0;
            Deaths = 0;
            Score = 0;
        }

        public void AddKill()
        {
            if (_playerNumber < 0)
            {
                return;
            }

            Kills++;
            UpdateStat();
        }

        public void AddDeaths()
        {
            if (_playerNumber < 0)
            {
                return;
            }

            Deaths++;
            UpdateStat();
        }

        private void UpdateStat()
        {
            Score = Kills - Deaths;
            GameUIManager.Instance.UpdateStats(_playerNumber);
        }
    }
}
