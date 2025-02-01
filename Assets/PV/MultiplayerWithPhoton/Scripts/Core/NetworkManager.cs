using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;

namespace PV.Multiplayer
{
    public enum NetworkEvent { JoinedLobby, JoinedRoom, CreatedRoom }

    public class NetworkManager : MonoBehaviourPunCallbacks
    {
        public static NetworkManager Instance;

        [Tooltip("Event triggered during ongoing network processes like connecting or joining a room.")]
        public static Action<string> OnDoProcess;
        [Tooltip("Event triggered when player joins a lobby.")]
        public static Action<NetworkEvent> OnNetworkEvent;
        [Tooltip("Event triggered when a network operation fails, such as disconnection or failed room join.")]
        public static Action OnError;

        [Tooltip("Maximum players that can join a single room.")]
        public int maxPlayers = 4;

        [HideInInspector]
        public bool IsPlayerInRoom { get; private set; }

        private UIManager _manager;

        private void Awake()
        {
            Instance = this;
            _manager = FindObjectOfType<UIManager>(true);
        }

        private void Start()
        {
            // Check if the client is already connected to Photon.
            if (!PhotonNetwork.IsConnected)
            {
                // Notify listeners about the connection attempt.
                OnDoProcess?.Invoke("Connecting...");
                PhotonNetwork.ConnectUsingSettings();
            }
        }

        public void CreateRoom(string roomName)
        {
            // Notify listeners about room joining attempt.
            OnDoProcess?.Invoke($"Joining Room : {roomName}");
            var roomOptions = new RoomOptions()
            {
                MaxPlayers = maxPlayers,
            };

            PhotonNetwork.JoinOrCreateRoom(roomName, roomOptions, null);
        }

        public void JoinRoom(string roomName)
        {
            PhotonNetwork.JoinRoom(roomName);
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
            OnError?.Invoke();
        }

        public override void OnJoinedLobby()
        {
            Debug.Log("Joined to Lobby.");
            OnNetworkEvent?.Invoke(NetworkEvent.JoinedLobby);
        }

        public override void OnJoinedRoom()
        {
            Debug.Log("Joined a room");
            IsPlayerInRoom = true;
            OnNetworkEvent?.Invoke(NetworkEvent.JoinedRoom);
        }

        public override void OnLeftRoom()
        {
            IsPlayerInRoom = false;
        }

        public override void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            _manager.UpdateRoomList(roomList);
        }

        public override void OnCreatedRoom()
        {
            OnNetworkEvent?.Invoke(NetworkEvent.CreatedRoom);
        }

        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            Debug.LogError($"Join Room Failed with return code {returnCode} and \nMessage: {message}");
            // Notify listeners about the failure.
            OnError?.Invoke();
        }
    }
}
