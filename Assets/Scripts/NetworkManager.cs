using UnityEngine;
using Photon.Pun;

namespace PV.Multiplayer
{
    public class NetworkManager : MonoBehaviourPunCallbacks
    {
        public GameObject playerPrefab;
        public Transform[] spawnPoints;

        private Vector3 _spawnPosition;

        private void Start()
        {
            if (!PhotonNetwork.IsConnected)
            {
                PhotonNetwork.ConnectUsingSettings();
            }
        }

        public override void OnConnectedToMaster()
        {
            base.OnConnectedToMaster();

            Debug.Log("Connected to Master.");
            PhotonNetwork.JoinLobby();
        }

        public override void OnJoinedLobby()
        {
            base.OnJoinedLobby();

            Debug.Log("Joined to Lobby.");
            PhotonNetwork.JoinOrCreateRoom("ding", null, null);
        }

        public override void OnJoinedRoom()
        {
            base.OnJoinedRoom();

            Debug.Log("Joined a room");
            SpawnPlayer();
        }

        /// <summary>
        /// Spawns the player at random spawn position.
        /// </summary>
        private void SpawnPlayer()
        {
            // Getting a random point from spawn points.
            _spawnPosition = spawnPoints[Random.Range(0, spawnPoints.Length)].position;

            // Instantiating player in network.
            PhotonNetwork.Instantiate(playerPrefab.name, _spawnPosition, Quaternion.identity);
        }
    }
}
