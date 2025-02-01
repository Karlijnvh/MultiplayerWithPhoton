using UnityEngine;
using TMPro;
using Photon.Pun;
using System.Collections.Generic;
using Photon.Realtime;

namespace PV.Multiplayer
{
    public class UIManager : MonoBehaviour
    {
        [Header("General")]
        public GameObject mainUI;
        public GameObject profileUI;
        public GameObject deathmatchUI;
        public GameObject roomUI;
        public GameObject background;
        public TextMeshProUGUI feedbackMessage;

        [Header("Profile")]
        public TMP_InputField playerNameField;

        [Header("Deathmatch")]
        public GameObject roomListPanel;
        public GameObject createRoomPanel;
        public GameObject roomList;
        public GameObject noRoomMessage;
        public GameObject roomItemPrefab;
        public Transform roomItemContainer;
        public TMP_InputField roomNameField;

        private Dictionary<string, RoomInfo> _roomInfos = new();
        private Dictionary<string, RoomItem> _roomItems = new();

        private void OnEnable()
        {
            // Subscribe to network manager events.
            NetworkManager.OnDoProcess += ShowFeedback;
            NetworkManager.OnNetworkEvent += NetworkManager_OnNetworkEvent;
            NetworkManager.OnError += NetworkManager_OnProcessFailed;
        }

        private void Start()
        {
            // Load player name from PlayerPrefs if it exists.
            if (PlayerPrefs.HasKey("PlayerName"))
            {
                playerNameField.text = PlayerPrefs.GetString("PlayerName");
            }

            feedbackMessage.text = "Connecting...";
            feedbackMessage.gameObject.SetActive(true);

            mainUI.SetActive(false);
            profileUI.SetActive(false);
            deathmatchUI.SetActive(false);
            roomUI.SetActive(false);

            CheckRoomList();
        }

        public void SavePlayerName()
        {
            if (!string.IsNullOrEmpty(playerNameField.text))
            {
                PhotonNetwork.NickName = playerNameField.text;
            }
            else
            {
                PhotonNetwork.NickName = $"Player{Random.Range(100, 9999)}";
            }

            PlayerPrefs.SetString("PlayerName", PhotonNetwork.NickName);
        }

        public void OpenProfile()
        {
            mainUI.SetActive(false);
            profileUI.SetActive(true);
        }

        public void CloseProfile()
        {
            profileUI.SetActive(false);
            mainUI.SetActive(true);
        }

        public void OpenDeathmatch()
        {
            mainUI.SetActive(false);

            roomListPanel.SetActive(true);
            createRoomPanel.SetActive(false);
            deathmatchUI.SetActive(true);
        }

        public void CloseDeathmatch()
        {
            deathmatchUI.SetActive(false);
            roomListPanel.SetActive(true);
            createRoomPanel.SetActive(false);

            mainUI.SetActive(true);
        }

        public void CreateRoom()
        {
            if (!string.IsNullOrEmpty(roomNameField.text))
            {
                // If this room already exist then return. Photon does not support identical rooms.
                if (_roomInfos.Count > 0 && _roomInfos.ContainsKey(roomNameField.text))
                {
                    return;
                }

                // Create the room.
                NetworkManager.Instance.CreateRoom(roomNameField.text);
            }
            else
            {
                Debug.LogError("Room creation failed! Room name empty!");
            }
        }

        public void UpdateRoomList(List<RoomInfo> roomList)
        {
            for (int i = 0; i < roomList.Count; i++)
            {
                // If cached data is empty.
                if (_roomInfos.Count <= 0)
                {
                    // If room is being removed then continue to next room.
                    if (roomList[i].RemovedFromList) continue;

                    AddRoom(roomList[i]);
                }
                else
                {
                    // If room is being removed then also remove from cached data.
                    if (roomList[i].RemovedFromList)
                    {
                        if (_roomInfos.ContainsKey(roomList[i].Name))
                        {
                            _roomInfos.Remove(roomList[i].Name);

                            _roomItems.Remove(roomList[i].Name, out RoomItem roomItem);
                            Destroy(roomItem.gameObject);
                        }
                    }
                    // If room is new, eg. it does not exist in cached data, then add room.
                    else if (!_roomInfos.ContainsKey(roomList[i].Name))
                    {
                        AddRoom(roomList[i]);
                    }
                    // If room exist in cached data then update it.
                    else if(_roomItems.ContainsKey(roomList[i].Name))
                    {
                        _roomItems[roomList[i].Name].SetPlayerRatio($"{roomList[i].PlayerCount} / {roomList[i].MaxPlayers}");
                    }
                }
            }

            CheckRoomList();
        }

        private void CheckRoomList()
        {
            bool hasRooms = _roomItems.Count > 0;
            roomList.SetActive(hasRooms);
            noRoomMessage.SetActive(!hasRooms);
        }

        private void AddRoom(RoomInfo room)
        {
            Debug.Log($"{room.Name} added");
            RoomItem roomItem = Instantiate(roomItemPrefab, roomItemContainer).GetComponent<RoomItem>();
            roomItem.InitItem(room.Name, $"{room.PlayerCount} / {room.MaxPlayers}", () => JoinRoom(room.Name));

            _roomInfos[room.Name] = room;
            _roomItems[room.Name] = roomItem;
        }

        private void JoinRoom(string roomName)
        {
            ShowFeedback("Joining room...");
            NetworkManager.Instance.JoinRoom(roomName);
        }

        public void LeaveRoom()
        {
            ShowFeedback("Leaving room...");
            PhotonNetwork.LeaveRoom();
        }

        private void ShowFeedback(string message)
        {
            mainUI.SetActive(false);
            profileUI.SetActive(false);
            deathmatchUI.SetActive(false);
            roomUI.SetActive(false);

            feedbackMessage.text = message;
            feedbackMessage.gameObject.SetActive(true);
        }

        private void NetworkManager_OnNetworkEvent(NetworkEvent networkEvent)
        {
            switch (networkEvent)
            {
                case NetworkEvent.JoinedLobby:
                    OnJoinedLobby();
                    break;
                case NetworkEvent.JoinedRoom:
                    OnJoinedRoom();
                    break;
                case NetworkEvent.CreatedRoom:
                    OnCreatedRoom();
                    break;
                default:
                    break;
            }
        }

        private void OnJoinedLobby()
        {
            feedbackMessage.gameObject.SetActive(false);
            mainUI.SetActive(true);
        }

        private void OnJoinedRoom()
        {
            // Disable active UI elements
            feedbackMessage.gameObject.SetActive(false);

            // Enable Room UI
            roomUI.SetActive(true);
        }

        private void OnCreatedRoom()
        {
            // Clear room info if the player create / host a room.
            foreach (var room in _roomItems.Values)
            {
                Destroy(room.gameObject);
            }

            _roomInfos.Clear();
            _roomItems.Clear();

            CheckRoomList();
        }

        private void NetworkManager_OnProcessFailed()
        {
            // Disable all UI elements and enable only main UI
            feedbackMessage.gameObject.SetActive(false);
            profileUI.SetActive(false);
            roomUI.SetActive(false);
            deathmatchUI.SetActive(false);

            mainUI.SetActive(true);
        }

        public void Quit()
        {
            Application.Quit();
        }

        private void OnDisable()
        {
            // Unsubscribe from network manager events when disabled.
            NetworkManager.OnDoProcess -= ShowFeedback;
            NetworkManager.OnNetworkEvent -= NetworkManager_OnNetworkEvent;
            NetworkManager.OnError -= NetworkManager_OnProcessFailed;
        }
    }
}
