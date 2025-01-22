using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using Cinemachine;

namespace PV.Multiplayer
{
    public class GameManager : MonoBehaviourPunCallbacks
    {
        public bool lockCursor;
        public GameObject playerPrefab;
        public Transform[] spawnPoints;

        [Header("Test")]
        public bool isTest = false;

        private Vector3 _spawnPosition;

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
            _spawnPosition = spawnPoints[Random.Range(0, spawnPoints.Length)].position;

            // Instantiating player in network.
            GameObject player = PhotonNetwork.Instantiate(playerPrefab.name, _spawnPosition, Quaternion.identity);

            CameraFollow.Instance.Init(player.GetComponent<PlayerController>());
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
