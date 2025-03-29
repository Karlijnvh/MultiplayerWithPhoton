using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace PV.Multiplayer
{
    /// <summary>
    /// Beheert de UI voor het hoofdmenu, profiel, deathmatch en room-functionaliteiten.
    /// Zorgt voor spelerconnectie, roomcreatie en spelersready-statussen.
    /// </summary>
    public class MenuUIManager : MonoBehaviourPunCallbacks
    {
        public static MenuUIManager Instance;

        [Header("General")]
        public GameObject mainUI;
        public GameObject profileUI;
        public GameObject deathmatchUI;
        public GameObject roomUI;
        public GameObject settingsUI;
        public TextMeshProUGUI feedbackMessage;

        [Header("Profile")]
        public TMP_InputField playerNameField;

        [Header("Deathmatch")]
        public GameObject roomListPanel;
        public GameObject createRoomPanel;
        [Tooltip("Object met de lijst van rooms.")]
        public GameObject roomList;
        public GameObject noRoomMessage;
        public GameObject roomItemPrefab;
        [Tooltip("Container om alle room-items in te plaatsen.")]
        public Transform roomItemContainer;
        public TMP_InputField roomNameField;
        public TextMeshProUGUI gameTimeText;
        public TextMeshProUGUI maxPlayersText;
        public Slider gameTimeSlider;
        public Slider maxPlayersSlider;
        public Toggle lockCursor;

        [Header("Room")]
        [Tooltip("Container om alle speler-items in de room te plaatsen.")]
        public Transform playerItemContainer;
        public GameObject playerItemPrefab;
        public Image readyButton;
        [Tooltip("Kleur van de ready knop als de speler klaar is.")]
        public Color readyColor = Color.white;
        [Tooltip("Kleur van de ready knop als de speler niet klaar is.")]
        public Color notReadyColor = Color.white;
        [Tooltip("Knop voor de host om de game te starten zodra alle spelers klaar zijn.")]
        public Button startGameButton;

        // Dictionaries voor roominformatie, UI-items en speler-items.
        private Dictionary<string, RoomInfo> _roomInfos = new Dictionary<string, RoomInfo>();
        private Dictionary<string, RoomItem> _roomItems = new Dictionary<string, RoomItem>();
        private Dictionary<int, PlayerItem> _playerItems = new Dictionary<int, PlayerItem>();

        private int _localID = -1;
        private readonly float _toggleDelay = 1f; // Tijd tussen toggles.
        private float _lastToggleTime = 0f; // Tijdstip van de laatste toggle.

        private const string DEFAULT_NAME = "Noobie";
        private const string READY_KEY = "IsReady";     // Key voor ready status.
        private const string GAME_TIME = "GameTime";      // Key voor game time in de room.
        private const string LOCK_CURSOR = "LockCursor";  // Key voor cursor-lock.

        private Hashtable _playerProps; // Gecachte speler custom properties.

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            // Laad spelernaam uit PlayerPrefs als die bestaat.
            if (PlayerPrefs.HasKey("PlayerName"))
            {
                playerNameField.text = PlayerPrefs.GetString("PlayerName");
            }

            // Initiële feedback
            feedbackMessage.text = "Connecting...";
            feedbackMessage.gameObject.SetActive(true);

            // Verberg alle UI-panelen
            mainUI.SetActive(false);
            profileUI.SetActive(false);
            deathmatchUI.SetActive(false);
            roomUI.SetActive(false);
            settingsUI.SetActive(false);

            // Zet de ready-knop in de beginstatus.
            readyButton.color = notReadyColor;

            // Beheer de cursor-lock op basis van PlayerPrefs.
            if (PlayerPrefs.HasKey(LOCK_CURSOR))
            {
                if (PlayerPrefs.GetInt(LOCK_CURSOR) == 1)
                {
                    lockCursor.isOn = true;
                    Cursor.lockState = CursorLockMode.Confined;
                }
                else
                {
                    lockCursor.isOn = false;
                    Cursor.lockState = CursorLockMode.None;
                }
            }
            else
            {
                lockCursor.isOn = true;
                Cursor.lockState = CursorLockMode.Confined;
                PlayerPrefs.SetInt(LOCK_CURSOR, 1);
            }

            CheckRoomList();
            SavePlayerName(); // Zorgt ervoor dat er altijd een spelernaam is.
        }

        /// <summary>
        /// Slaat de spelernaam op in Photon en PlayerPrefs.
        /// </summary>
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

        // --- Menu navigatie methoden ---
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

        public void OpenSettings()
        {
            mainUI.SetActive(false);
            settingsUI.SetActive(true);
        }

        public void CloseSettings()
        {
            settingsUI.SetActive(false);
            mainUI.SetActive(true);
        }

        public void OnLockCursorChanged(bool lockCursorValue)
        {
            PlayerPrefs.SetInt(LOCK_CURSOR, lockCursorValue ? 1 : 0);
            Cursor.lockState = lockCursorValue ? CursorLockMode.Confined : CursorLockMode.None;
        }

        /// <summary>
        /// Maakt een nieuwe room als de roomnaam geldig is.
        /// </summary>
        public void CreateRoom()
        {
            if (!string.IsNullOrEmpty(roomNameField.text))
            {
                if (_roomInfos.Count > 0 && _roomInfos.ContainsKey(roomNameField.text))
                {
                    return;
                }
                NetworkManager.Instance.CreateRoom(roomNameField.text);
            }
            else
            {
                Debug.LogError("Room creation failed! Room name empty!");
            }
        }

        /// <summary>
        /// Update de roomlijst-UI met de nieuwste roominformatie.
        /// </summary>
        public void UpdateRoomList(List<RoomInfo> roomList)
        {
            for (int i = 0; i < roomList.Count; i++)
            {
                if (_roomInfos.Count <= 0)
                {
                    if (roomList[i].RemovedFromList) continue;
                    AddRoom(roomList[i]);
                }
                else
                {
                    if (roomList[i].RemovedFromList)
                    {
                        if (_roomInfos.ContainsKey(roomList[i].Name))
                        {
                            _roomInfos.Remove(roomList[i].Name);
                            _roomItems.Remove(roomList[i].Name, out RoomItem roomItem);
                            Destroy(roomItem.gameObject);
                        }
                    }
                    else if (!_roomInfos.ContainsKey(roomList[i].Name))
                    {
                        AddRoom(roomList[i]);
                    }
                    else
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

        /// <summary>
        /// Controleert of er rooms zijn en toont een passend bericht.
        /// </summary>
        private void CheckRoomList()
        {
            bool hasRooms = _roomItems.Count > 0;
            roomList.SetActive(hasRooms);
            noRoomMessage.SetActive(!hasRooms);
        }

        /// <summary>
        /// Voegt een nieuwe room toe aan de roomlijst-UI.
        /// </summary>
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

        /// <summary>
        /// Probeert een room te joinen als deze beschikbaar en niet vol is.
        /// </summary>
        private void JoinRoom(string roomName)
        {
            if (_roomInfos.ContainsKey(roomName))
            {
                if (!_roomInfos[roomName].IsOpen)
                {
                    Debug.Log("Game already started.");
                    return;
                }
                else if (_roomInfos[roomName].PlayerCount >= _roomInfos[roomName].MaxPlayers)
                {
                    Debug.Log("Room is full.");
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

        /// <summary>
        /// Toont een feedbackbericht tijdens een actie.
        /// </summary>
        public void ShowFeedback(string message)
        {
            mainUI.SetActive(false);
            profileUI.SetActive(false);
            deathmatchUI.SetActive(false);
            roomUI.SetActive(false);
            feedbackMessage.text = message;
            feedbackMessage.gameObject.SetActive(true);
        }

        // --- Photon Callback Overrides ---
        public override void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            UpdateRoomList(roomList);
        }

        public void OnPlayerEnteredRoom(Playert newPlayer)
        {
            // Update de spelerslijst zodra een nieuwe speler de room binnenkomt.
            if (!_playerItems.ContainsKey(newPlayer.ActorNumber))
            {
                CreatePlayerItem(newPlayer.ActorNumber);
            }
            CheckAllPlayersReady();
        }

        public void OnPlayerLeftRoom(Player otherPlayer)
        {
            // Update de spelerslijst zodra een speler de room verlaat.
            if (_playerItems.ContainsKey(otherPlayer.ActorNumber))
            {
                RemovePlayerItem(otherPlayer.ActorNumber);
            }
            CheckAllPlayersReady();
        }

        public void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
            // Update de ready-status van de speler als deze verandert.
            if (changedProps.ContainsKey(READY_KEY))
            {
                if (_playerItems.ContainsKey(targetPlayer.ActorNumber))
                {
                    _playerItems[targetPlayer.ActorNumber].SetStatus((bool)changedProps[READY_KEY]);
                }
                else
                {
                    CreatePlayerItem(targetPlayer.ActorNumber);
                }
                CheckAllPlayersReady();
            }
        }

        public void OnGameTimeChanged()
        {
            gameTimeText.text = gameTimeSlider.value.ToString();
        }

        public void OnMaxPlayersChanged()
        {
            maxPlayersText.text = maxPlayersSlider.value.ToString();
            NetworkManager.Instance.SetMaxPlayers((int)maxPlayersSlider.value);
        }

        /// <summary>
        /// Callback voor de startknop; de host kan hiermee de game starten zodra alle spelers klaar zijn.
        /// </summary>
        public void StartGameButtonPressed()
        {
            if (PhotonNetwork.IsMasterClient && startGameButton.interactable)
            {
                PhotonNetwork.LoadLevel(1);
            }
        }

        public void ToggleReadyStatus()
        {
            if (_localID == -1 || Time.time - _lastToggleTime < _toggleDelay)
            {
                return;
            }
            _lastToggleTime = Time.time;

            bool currentStatus = _playerProps.ContainsKey(READY_KEY) ? (bool)_playerProps[READY_KEY] : false;
            SetLocalReadyStatus(!currentStatus);
            CheckAllPlayersReady();
        }

        /// <summary>
        /// Wijzigt de lokale ready-status en werkt de UI bij.
        /// </summary>
        private void SetLocalReadyStatus(bool isReady)
        {
            _playerProps[READY_KEY] = isReady;
            PhotonNetwork.LocalPlayer.SetCustomProperties(_playerProps);
            readyButton.color = isReady ? readyColor : notReadyColor;
            _playerItems[_localID].SetStatus(isReady);
        }

        /// <summary>
        /// Creëert een nieuw UI-element voor een speler in de room.
        /// </summary>
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

        /// <summary>
        /// Verwijdert het UI-element van een speler die de room verlaat.
        /// </summary>
        private void RemovePlayerItem(int actorNumber)
        {
            if (_playerItems.Remove(actorNumber, out PlayerItem item))
            {
                Destroy(item.gameObject);
            }
        }

        /// <summary>
        /// Behandelt fouten door de juiste UI te tonen.
        /// </summary>
        public void OnError()
        {
            if (feedbackMessage != null)
            {
                feedbackMessage.gameObject.SetActive(false);
                profileUI.SetActive(false);
                roomUI.SetActive(false);
                deathmatchUI.SetActive(false);
                mainUI.SetActive(true);
            }
        }

        /// <summary>
        /// Sluit de applicatie af.
        /// </summary>
        public void Quit()
        {
            Application.Quit();
        }

        // --- Extra helper-methoden uit de eerste versie ---

        /// <summary>
        /// Verwerkt netwerkgebeurtenissen en roept de juiste callback op.
        /// </summary>
        public void OnNetworkEvent(NetworkEvent networkEvent)
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

        /// <summary>
        /// Callback wanneer de speler de lobby betreedt.
        /// </summary>
        public void OnJoinedLobby()
        {
            feedbackMessage.gameObject.SetActive(false);
            mainUI.SetActive(true);
            if (_localID != -1)
            {
                _localID = -1;
                foreach (PlayerItem item in _playerItems.Values)
                {
                    Destroy(item.gameObject);
                }
                _playerItems.Clear();
                readyButton.color = notReadyColor;
                if (_playerProps != null)
                {
                    _playerProps[READY_KEY] = false;
                    PhotonNetwork.LocalPlayer.SetCustomProperties(_playerProps);
                }
            }
            NetworkManager.Instance.SetMaxPlayers(4);
        }

        /// <summary>
        /// Callback wanneer de speler een room betreedt.
        /// </summary>
        public void OnJoinedRoom()
        {
            feedbackMessage.gameObject.SetActive(false);
            foreach (Player player in PhotonNetwork.PlayerList)
            {
                if (!_playerItems.ContainsKey(player.ActorNumber))
                    CreatePlayerItem(player.ActorNumber);
            }
            _localID = PhotonNetwork.LocalPlayer.ActorNumber;
            _playerProps = new Hashtable() { { READY_KEY, false } };
            PhotonNetwork.LocalPlayer.SetCustomProperties(_playerProps);
            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { GAME_TIME, (int)gameTimeSlider.value } });
                startGameButton.gameObject.SetActive(true);
                startGameButton.interactable = false;
            }
            else
            {
                startGameButton.gameObject.SetActive(false);
            }
            roomUI.SetActive(true);
        }

        /// <summary>
        /// Callback wanneer de speler een room aanmaakt.
        /// </summary>
        public void OnCreatedRoom()
        {
            foreach (var room in _roomItems.Values)
            {
                Destroy(room.gameObject);
            }
            _roomInfos.Clear();
            _roomItems.Clear();
            CheckRoomList();
            _localID = PhotonNetwork.LocalPlayer.ActorNumber;
            PlayerItem playerItem = Instantiate(playerItemPrefab, playerItemContainer).GetComponent<PlayerItem>();
            playerItem.InitItem(PhotonNetwork.NickName);
            _playerItems[_localID] = playerItem;
            _playerProps = new Hashtable() { { READY_KEY, false } };
            PhotonNetwork.LocalPlayer.SetCustomProperties(_playerProps);
            if (PhotonNetwork.IsMasterClient)
            {
                startGameButton.gameObject.SetActive(true);
                startGameButton.interactable = false;
            }
        }

        /// <summary>
        /// Extra helper-method voor het toevoegen van een speler via actorNumber.
        /// </summary>
        public void OnPlayerEnter(int actorNumber)
        {
            if (!_playerItems.ContainsKey(actorNumber))
            {
                CreatePlayerItem(actorNumber);
            }
            CheckAllPlayersReady();
        }

        /// <summary>
        /// Extra helper-method voor het verwijderen van een speler.
        /// </summary>
        public void OnPlayerLeft(Player player)
        {
            if (_playerItems.ContainsKey(player.ActorNumber))
            {
                RemovePlayerItem(player.ActorNumber);
            }
            CheckAllPlayersReady();
        }

        /// <summary>
        /// Extra helper-method voor het updaten van speler properties.
        /// </summary>
        public void OnPlayerPropsUpdate(int actorNumber, Hashtable changedProps)
        {
            Player targetPlayer = PhotonNetwork.CurrentRoom.GetPlayer(actorNumber);
            if (targetPlayer != null)
            {
                OnPlayerPropertiesUpdate(targetPlayer, changedProps);
            }
        }

        /// <summary>
        /// Controleert of alle spelers in de room klaar zijn.
        /// Als dit het geval is en de lokale speler de host is, wordt de startknop geactiveerd.
        /// </summary>
        private void CheckAllPlayersReady()
        {
            bool allReady = true;
            foreach (Player p in PhotonNetwork.PlayerList)
            {
                if (p.CustomProperties.ContainsKey(READY_KEY) && _playerItems.ContainsKey(p.ActorNumber))
                {
                    if (!(bool)p.CustomProperties[READY_KEY])
                    {
                        allReady = false;
                        break;
                    }
                }
                else
                {
                    allReady = false;
                    break;
                }
            }
            if (_playerItems.Count < 2 || NetworkManager.Instance.isLeaving)
                allReady = false;
            if (PhotonNetwork.IsMasterClient)
                startGameButton.interactable = allReady;
        }
    }
}
