using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;

namespace PV.Multiplayer
{
    public class GameManager : MonoBehaviourPunCallbacks
    {
        public static GameManager Instance { get; private set; }

        public bool lockCursor;
        public GameObject playerPrefab;
        public Transform[] spawnPoints;

        [Header("Test")]
        public bool isTest = false;

        private Transform _spawnPoint;

        private void Awake()
        {
            Instance = this;
        }

        // Start is called before the first frame update
        void Start()
        {
            if (!isTest && !PhotonNetwork.IsConnected)
            {
                SceneManager.LoadScene(0);
            }
            else
            {
                if (playerPrefab == null)
                {
                    Debug.LogError("Player prefab is missing!");
                }
                else if (!isTest) 
                { 
                    SpawnPlayer();
                }
            }

            if (lockCursor)
            {
                Cursor.lockState = CursorLockMode.Confined;
            }
        }

        /// <summary>
        /// Spawns the player at random spawn position.
        /// </summary>
        private void SpawnPlayer()
        {
            // Getting a random point from spawn points.
            _spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

            // Instantiating player in network.
            PlayerController player = PhotonNetwork.Instantiate(playerPrefab.name, _spawnPoint.position, _spawnPoint.rotation).GetComponent<PlayerController>();
            CameraFollow.Instance.Init(player);
        }

        public void ReSpawn(PlayerController player)
        {
            _spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

            player.gameObject.SetActive(false);
            player.transform.SetPositionAndRotation(_spawnPoint.position, _spawnPoint.rotation);
            player.gameObject.SetActive(true);
        }

        public override void OnLeftRoom()
        {
            SceneManager.LoadScene(0);
        }

        public void LeaveRoom()
        {
            PhotonNetwork.LeaveRoom();
        }
    }
}
