using UnityEngine;
using TMPro;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Photon.Realtime;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace PV.Multiplayer
{
    public class MenuUIManager : MonoBehaviour
    {
        public static MenuUIManager Instance;

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

        [Header("Room")]
        public Transform playerItemContainer;
        public GameObject playerItemPrefab;
        public Image readyButton;
        public Color readyColor = Color.white;
        public Color notReadyColor = Color.white;

        private Dictionary<string, RoomInfo> _roomInfos = new();
        private Dictionary<string, RoomItem> _roomItems = new();
        private Dictionary<int, PlayerItem> _playerItems = new();

        private int _localID = -1;
        private readonly float _toggleDelay = 1f; // Time to wait before toggling the ready status
        private float _lastToggleTime = 0f; // The last time we toggle the ready status

        private const string DEFAULT_NAME = "Noobie";
        public const string READY_KEY = "IsReady";

        private Hashtable _playerProps;
        private readonly WaitForSeconds _waitForCountdown = new(1);
        private bool _canLoadLevel = false;

        private void Awake()
        {
            Instance = this;
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

            readyButton.color = notReadyColor;

            CheckRoomList();
            SavePlayerName(); // Insures that there will be a player name.
        }

        public void SavePlayerName()
        {
            if (!string.IsNullOrEmpty(playerNameField.text))
            {
                PhotonNetwork.NickName = playerNameField.text;
            }
            else
            {
                PhotonNetwork.NickName = DEFAULT_NAME;
                playerNameField.text = DEFAULT_NAME;
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
                    else if(_roomInfos.ContainsKey(roomList[i].Name))
                    {
                        _roomInfos[roomList[i].Name] = roomList[i];

                        if (!roomList[i].IsOpen || roomList[i].PlayerCount >= roomList[i].MaxPlayers)
                        {
                            _roomItems[roomList[i].Name].Disable();
                        }
                        else
                        {
                            _roomItems[roomList[i].Name].Enable();
                        }
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
            RoomItem roomItem = Instantiate(roomItemPrefab, roomItemContainer).GetComponent<RoomItem>();
            roomItem.InitItem(room.Name, $"{room.PlayerCount} / {room.MaxPlayers}", () => JoinRoom(room.Name));
            if (!room.IsOpen || room.PlayerCount >= room.MaxPlayers)
            {
                roomItem.Disable();
            }

            _roomInfos[room.Name] = room;
            _roomItems[room.Name] = roomItem;
        }

        private void JoinRoom(string roomName)
        {
            if (_roomInfos.ContainsKey(roomName))
            {
                if (!_roomInfos[roomName].IsOpen)
                {
                    Debug.Log("Game already started.");
                    // TODO: use UI element to show message to player.
                    return;
                }
                else if (_roomInfos[roomName].PlayerCount >= _roomInfos[roomName].MaxPlayers)
                {
                    Debug.Log("Room is full.");
                    // TODO: use UI element to show message to player.
                    return;
                }

                ShowFeedback("Joining room...");
                NetworkManager.Instance.JoinRoom(roomName);
            }
        }

        public void LeaveRoom()
        {
            if (!NetworkManager.Instance.isLeaving)
            {
                ShowFeedback("Leaving room...");
                NetworkManager.Instance.isLeaving = true;
                PhotonNetwork.LeaveRoom();
            }
        }

        public void ShowFeedback(string message)
        {
            mainUI.SetActive(false);
            profileUI.SetActive(false);
            deathmatchUI.SetActive(false);
            roomUI.SetActive(false);

            feedbackMessage.text = message;
            feedbackMessage.gameObject.SetActive(true);
        }

        public void OnNetworkEvent(NetworkEvent networkEvent)
        {
            switch (networkEvent)
            {
                case NetworkEvent.JoinedLobby: OnJoinedLobby();
                    break;
                case NetworkEvent.JoinedRoom: OnJoinedRoom();
                    break;
                case NetworkEvent.CreatedRoom: OnCreatedRoom();
                    break;
                default:
                    break;
            }
        }

        public void OnJoinedLobby()
        {
            feedbackMessage.gameObject.SetActive(false);
            mainUI.SetActive(true);

            // If it is not the first time joining lobby, eg. When player leaves a room.
            if (_localID != -1)
            {
                // Set local id to -1 on joined lobby. Because the player is not in any room.
                _localID = -1;

                // Reset players list. It also works when the player leaves the room.
                foreach (PlayerItem item in _playerItems.Values)
                {
                    Destroy(item.gameObject);
                }
                _playerItems.Clear();

                // Reset ready status.
                readyButton.color = notReadyColor;
                _playerProps[READY_KEY] = false;
                PhotonNetwork.LocalPlayer.SetCustomProperties(_playerProps);
            }
        }

        public void OnJoinedRoom()
        {
            // Disable active UI elements.
            feedbackMessage.gameObject.SetActive(false);

            // Create all players list in current room.
            foreach (Player player in PhotonNetwork.PlayerList)
            {
                if (!_playerItems.ContainsKey(player.ActorNumber))
                {
                    CreatePlayerItem(player.ActorNumber);
                }
            }

            // Cache local player's ID.
            _localID = PhotonNetwork.LocalPlayer.ActorNumber;

            // Setting local player properties.
            _playerProps = new() { { READY_KEY, false } };
            PhotonNetwork.LocalPlayer.SetCustomProperties(_playerProps);

            // Enable Room UI.
            roomUI.SetActive(true);
        }

        public void OnCreatedRoom()
        {
            // Clear room info if the player create / host a room.
            foreach (var room in _roomItems.Values)
            {
                Destroy(room.gameObject);
            }

            _roomInfos.Clear();
            _roomItems.Clear();

            CheckRoomList();

            // Creation of player item UI in room
            _localID = PhotonNetwork.LocalPlayer.ActorNumber;
            PlayerItem playerItem = Instantiate(playerItemPrefab, playerItemContainer).GetComponent<PlayerItem>();
            playerItem.InitItem(PhotonNetwork.NickName);
            _playerItems[_localID] = playerItem;

            _playerProps = new() { { READY_KEY, false } };
            PhotonNetwork.LocalPlayer.SetCustomProperties(_playerProps);
        }

        public void OnPlayerEnter(int actorNumber)
        {
            if (!_playerItems.ContainsKey(actorNumber))
            {
                CreatePlayerItem(actorNumber);
            }
        }

        public void OnPlayerLeft(Player player)
        {
            if (_playerItems.ContainsKey(player.ActorNumber))
            {
                player.SetCustomProperties(new Hashtable());
                RemovePlayerItem(player.ActorNumber);
            }

            CheckAllPlayersReady();
        }

        private void CheckAllPlayersReady()
        {
            foreach (Player p in PhotonNetwork.PlayerList)
            {
                if (p.CustomProperties.ContainsKey(READY_KEY) && _playerItems.ContainsKey(p.ActorNumber))
                {
                    if (!(bool)p.CustomProperties[READY_KEY])
                    {
                        if (_canLoadLevel)
                        {
                            _canLoadLevel = false;
                            if (!roomUI.activeSelf)
                            {
                                // Disable feedback UI.
                                feedbackMessage.gameObject.SetActive(false);
                                // Enable Room UI.
                                roomUI.SetActive(true);
                            }
                        }
                        return;
                    }
                }
            }

            // Return if not enough players.
            if (_playerItems.Count < 2)
            {
                return;
            }

            // Return if the player is about to leave.
            if (NetworkManager.Instance.isLeaving)
            {
                return;
            }

            if (!_canLoadLevel)
            {
                StartCoroutine(LoadLevelDelay());
            }
            else if (PhotonNetwork.IsMasterClient)
            {
                // Close the room, so no more players can join.
                PhotonNetwork.CurrentRoom.IsOpen = false;
                // Load Game Scene for all players.
                PhotonNetwork.LoadLevel(1);
            }
        }

        IEnumerator LoadLevelDelay()
        {
            ShowFeedback("Loading Level In 3...");
            yield return _waitForCountdown;

            ShowFeedback("Loading Level In 2...");
            yield return _waitForCountdown;

            ShowFeedback("Loading Level In 1...");
            yield return _waitForCountdown;

            ShowFeedback("Loading Level...");
            _canLoadLevel = true;
            CheckAllPlayersReady();
        }

        public void OnPlayerPropsUpdate(int actorNumber, Hashtable props)
        {
            if (props.ContainsKey(READY_KEY))
            {
                if (_playerItems.ContainsKey(actorNumber))
                {
                    _playerItems[actorNumber].SetStatus((bool)props[READY_KEY]);
                }
                else
                {
                    CreatePlayerItem(actorNumber);
                }

                CheckAllPlayersReady();
            }
        }

        public void ToggleReadyStatus()
        {
            if (_localID == -1 || Time.time - _lastToggleTime < _toggleDelay)
            {
                return;
            }

            _lastToggleTime = Time.time;

            if (_playerProps.ContainsKey(READY_KEY))
            {
                SetLocalReadyStatus(!(bool)_playerProps[READY_KEY]);
            }
            else
            {
                SetLocalReadyStatus(false);
            }
        }

        private void SetLocalReadyStatus(bool isReady)
        {
            _playerProps[READY_KEY] = isReady;
            PhotonNetwork.LocalPlayer.SetCustomProperties(_playerProps);

            readyButton.color = isReady ? readyColor : notReadyColor;
            _playerItems[_localID].SetStatus(isReady);
        }

        private void CreatePlayerItem(int actorNumber)
        {
            PlayerItem playerItem = Instantiate(playerItemPrefab, playerItemContainer).GetComponent<PlayerItem>();
            Player player = PhotonNetwork.CurrentRoom.GetPlayer(actorNumber);
            playerItem.InitItem(player != null ? player.NickName : DEFAULT_NAME);

            if (player.CustomProperties.ContainsKey(READY_KEY))
            {
                playerItem.SetStatus((bool)player.CustomProperties[READY_KEY]);
            }

            _playerItems[actorNumber] = playerItem;
        }

        private void RemovePlayerItem(int actorNumber)
        {
            _playerItems.Remove(actorNumber, out PlayerItem item);
            Destroy(item.gameObject);
        }

        public void OnError()
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
    }
}