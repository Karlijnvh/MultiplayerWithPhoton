using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

namespace PV.Multiplayer
{
    public class NetworkManager : MonoBehaviourPunCallbacks
    {
        public static NetworkManager Instance;

        public static System.Action<string> OnDoProcess;
        public static System.Action OnProcessFailed;

        private string _roomName;

        private void Awake()
        {
            Instance = this;
        }

        public void Connect(string roomName, string playerName)
        {
            _roomName = roomName;
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

            if (!PhotonNetwork.IsConnected)
            {
                OnDoProcess?.Invoke("Connecting...");
                PhotonNetwork.ConnectUsingSettings();
            }
            else
            {
                OnDoProcess?.Invoke($"Joining Room : {_roomName}");
                PhotonNetwork.JoinOrCreateRoom(_roomName, null, null);
            }
        }

        public override void OnConnectedToMaster()
        {
            Debug.Log("Connected to Master.");

            OnDoProcess?.Invoke("Connecting to lobby...");
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
                OnDoProcess?.Invoke($"Joining Room : {_roomName}");
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

            PhotonNetwork.LoadLevel(1);
        }

        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            Debug.LogError($"Join Room Failed with return code {returnCode} and \nMessage: {message}");
            OnProcessFailed?.Invoke();
        }
    }
}
