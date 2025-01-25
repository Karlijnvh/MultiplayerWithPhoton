using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

namespace PV.Multiplayer
{
    public class NetworkManager : MonoBehaviourPunCallbacks
    {
        public static NetworkManager Instance;

        [Tooltip("Event triggered during ongoing network processes like connecting or joining a room.")]
        public static System.Action<string> OnDoProcess;
        [Tooltip("Event triggered when a network operation fails, such as disconnection or failed room join.")]
        public static System.Action OnProcessFailed;

        // Holds the name of the room to join or create.
        private string _roomName;

        private void Awake()
        {
            Instance = this;
        }

        /// <summary>
        /// Connects to the Photon server and joins or creates a room with the specified room name and player name.
        /// </summary>
        /// <param name="roomName">Name of the room to join or create.</param>
        /// <param name="playerName">Player's display name.</param>
        public void Connect(string roomName, string playerName)
        {
            _roomName = roomName;

            // Set player nickname and store it locally.
            if (playerName.Trim().Length > 0)
            {
                PhotonNetwork.NickName = playerName;
                PlayerPrefs.SetString("PlayerName", playerName);
            }
            else
            {
                PhotonNetwork.NickName = $"Player{Random.Range(100, 9999)}";
                PlayerPrefs.SetString("PlayerName", PhotonNetwork.NickName);
            }

            // Check if the client is already connected to Photon.
            if (!PhotonNetwork.IsConnected)
            {
                // Notify listeners about the connection attempt.
                OnDoProcess?.Invoke("Connecting...");
                PhotonNetwork.ConnectUsingSettings();
            }
            else
            {
                // Notify listeners about room joining attempt.
                OnDoProcess?.Invoke($"Joining Room : {_roomName}");
                PhotonNetwork.JoinOrCreateRoom(_roomName, null, null);
            }
        }

        public override void OnConnectedToMaster()
        {
            Debug.Log("Connected to Master.");

            // Notify listeners about the connection attempt.
            OnDoProcess?.Invoke("Connecting to lobby...");
            // Join the lobby after connecting to the master server.
            PhotonNetwork.JoinLobby();
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            Debug.Log($"Disconnected from server.\nCause : {cause}");
            OnProcessFailed?.Invoke();
        }

        public override void OnJoinedLobby()
        {
            Debug.Log("Joined to Lobby.");

            // Checking if room name is not null and empty.
            if (_roomName != null && _roomName.Trim().Length > 0)
            {
                // Notify listeners about room joining attempt.
                OnDoProcess?.Invoke($"Joining Room : {_roomName}");
                // Attempt to join or create the specified room.
                PhotonNetwork.JoinOrCreateRoom(_roomName, null, null);
            }
            else
            {
                Debug.Log("Room name is empty.");
            }
        }

        public override void OnJoinedRoom()
        {
            Debug.Log("Joined a room");

            // Load the game level upon successfully joining a room.
            PhotonNetwork.LoadLevel(1);
        }

        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            Debug.LogError($"Join Room Failed with return code {returnCode} and \nMessage: {message}");
            // Notify listeners about the failure.
            OnProcessFailed?.Invoke();
        }
    }
}
