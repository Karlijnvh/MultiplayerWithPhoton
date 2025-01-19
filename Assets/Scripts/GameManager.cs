using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using Cinemachine;

namespace PV.Multiplayer
{
    public class GameManager : MonoBehaviourPunCallbacks
    {
        public GameObject playerPrefab;
        public CinemachineVirtualCameraBase cinemachineCamera;
        public Transform[] spawnPoints;

        private Vector3 _spawnPosition;

        // Start is called before the first frame update
        void Start()
        {
            if (!PhotonNetwork.IsConnected)
            {
                SceneManager.LoadScene(0);
            }
            else
            {
                if (cinemachineCamera == null)
                {
                    cinemachineCamera = FindObjectOfType<CinemachineVirtualCameraBase>();
                }

                if (playerPrefab == null)
                {
                    Debug.LogError("Player prefab is missing!");
                }
                else
                {
                    SpawnPlayer();
                }
            }
        }

        /// <summary>
        /// Spawns the player at random spawn position.
        /// </summary>
        private void SpawnPlayer()
        {
            Debug.Log("11111");
            // Getting a random point from spawn points.
            _spawnPosition = spawnPoints[Random.Range(0, spawnPoints.Length)].position;

            // Instantiating player in network.
            GameObject player = PhotonNetwork.Instantiate(playerPrefab.name, _spawnPosition, Quaternion.identity);
            if (cinemachineCamera != null)
            {
                cinemachineCamera.Follow = player.transform;
                cinemachineCamera.LookAt = player.transform;
            }
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
