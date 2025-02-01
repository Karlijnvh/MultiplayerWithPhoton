using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;

namespace PV.Multiplayer
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Tooltip("Locks the cursor to the game window when enabled.")]
        public bool lockCursor;

        [Tooltip("The prefab used for spawning players.")]
        public GameObject playerPrefab;

        [Tooltip("Array of spawn points where players can be spawned.")]
        public Transform[] spawnPoints;

        // The selected spawn point for the player.
        private Transform _spawnPoint;

        private void Awake()
        {
            Instance = this;
        }

        // Start is called before the first frame update
        void Start()
        {
            // If not in testing mode and not connected to Photon, load the main menu scene.
            if (!PhotonNetwork.IsConnected)
            {
                SceneManager.LoadScene(0);
            }
            else
            {
                if (playerPrefab == null)
                {
                    Debug.LogError("Player prefab is missing!");
                }
                else
                { 
                    SpawnPlayer();
                }
            }

            if (lockCursor)
            {
                // Lock the cursor to the game window.
                Cursor.lockState = CursorLockMode.Confined;
            }
        }

        /// <summary>
        /// Spawns the player at random spawn position.
        /// </summary>
        private void SpawnPlayer()
        {
            // Select a random spawn point.
            _spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

            // Instantiate the player prefab on the network.
            PlayerController player = PhotonNetwork.Instantiate(ResourcePaths.Character + playerPrefab.name, _spawnPoint.position, _spawnPoint.rotation).GetComponent<PlayerController>();

            // Initialize the camera to follow the newly spawned player.
            CameraFollow.Instance.Init(player);
        }

        /// <summary>
        /// Respawns the given player at a random spawn point.
        /// </summary>
        /// <param name="player">The player to respawn.</param>
        public void ReSpawn(PlayerController player)
        {
            // Select a random spawn point.
            _spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

            // Temporarily deactivate the player for repositioning.
            player.gameObject.SetActive(false);
            player.transform.SetPositionAndRotation(_spawnPoint.position, _spawnPoint.rotation);
            player.gameObject.SetActive(true);
        }
    }
}
