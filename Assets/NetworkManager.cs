using UnityEngine;
using Photon.Pun;

namespace PV.Multiplayer
{
    public class NetworkManager : MonoBehaviourPunCallbacks
    {

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
        }
    }
}
