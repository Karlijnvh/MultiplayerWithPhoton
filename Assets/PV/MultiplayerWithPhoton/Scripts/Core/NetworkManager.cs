using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using ExitGames.Client.Photon;

namespace PV.Multiplayer
{
    public enum NetworkEvent { JoinedLobby, JoinedRoom, CreatedRoom }

    public class NetworkManager : MonoBehaviourPunCallbacks
    {
        public static NetworkManager Instance;

        [Tooltip("Maximum players that can join a single room.")]
        public int maxPlayers = 4;

        [HideInInspector]
        public bool isLeaving = false;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            // Check if the client is already connected to Photon.
            if (!PhotonNetwork.IsConnected)
            {
                // Notify listeners about the connection attempt.
                MenuUIManager.Instance.ShowFeedback("Connecting...");
                PhotonNetwork.ConnectUsingSettings();
                PhotonNetwork.AutomaticallySyncScene = true;
            }
        }

        public void CreateRoom(string roomName)
        {
            // Notify listeners about room joining attempt.
            MenuUIManager.Instance.ShowFeedback($"Joining Room : {roomName}");
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
            // Notify listeners about the connection attempt.
            MenuUIManager.Instance.ShowFeedback("Connecting to lobby...");
            // Join the lobby after connecting to the master server.
            PhotonNetwork.JoinLobby();
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            Debug.Log($"Disconnected from server.\nCause : {cause}");
            MenuUIManager.Instance.OnError();
        }

        public override void OnJoinedLobby()
        {
            MenuUIManager.Instance.OnNetworkEvent(NetworkEvent.JoinedLobby);
        }

        public override void OnJoinedRoom()
        {
            isLeaving = false;
            MenuUIManager.Instance.OnNetworkEvent(NetworkEvent.JoinedRoom);
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            MenuUIManager.Instance.OnPlayerEnter(newPlayer.ActorNumber);
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            MenuUIManager.Instance.OnPlayerLeft(otherPlayer);
        }

        public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
            MenuUIManager.Instance.OnPlayerPropsUpdate(targetPlayer.ActorNumber, changedProps);
        }

        public override void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            MenuUIManager.Instance.UpdateRoomList(roomList);
        }

        public override void OnCreatedRoom()
        {
            MenuUIManager.Instance.OnNetworkEvent(NetworkEvent.CreatedRoom);
        }

        public override void OnMasterClientSwitched(Player newMasterClient)
        {
            if (!isLeaving)
            {
                isLeaving = true;
                PhotonNetwork.LeaveRoom();
            }
        }

        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            Debug.LogError($"Join Room Failed with return code {returnCode} and \nMessage: {message}");
            // Notify listeners about the failure.
            MenuUIManager.Instance.OnError();
        }
    }
}