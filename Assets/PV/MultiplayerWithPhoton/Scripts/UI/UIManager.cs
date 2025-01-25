using UnityEngine;
using TMPro;

namespace PV.Multiplayer
{
    public class UIManager : MonoBehaviour
    {
        [Tooltip("The lobby UI element.")]
        public GameObject lobby;
        [Tooltip("The background UI element.")]
        public GameObject background;
        [Tooltip("TextMeshPro component to show feedback messages to the player.")]
        public TextMeshProUGUI feedbackMessage;

        [Tooltip("Input field for the player's name.")]
        public TMP_InputField playerNameField;
        [Tooltip("Input field for the room name.")]
        public TMP_InputField roomNameField;

        private void OnEnable()
        {
            // Subscribe to network manager events.
            NetworkManager.OnProcessFailed += NetworkManager_OnProcessFailed;
            NetworkManager.OnDoProcess += NetworkManager_OnDoProcess;
        }

        private void Start()
        {
            // Load player name from PlayerPrefs if it exists.
            if (PlayerPrefs.HasKey("PlayerName"))
            {
                playerNameField.text = PlayerPrefs.GetString("PlayerName");
            }
        }

        public void JoinRoom()
        {
            feedbackMessage.text = "Connecting...";
            feedbackMessage.gameObject.SetActive(true);
            lobby.SetActive(false);

            // Connect to the room.
            NetworkManager.Instance.Connect(roomNameField.text, playerNameField.text);
        }

        private void NetworkManager_OnDoProcess(string message)
        {
            feedbackMessage.text = message;
        }

        private void NetworkManager_OnProcessFailed()
        {
            feedbackMessage.gameObject.SetActive(false);
            lobby.SetActive(true);
        }

        public void Quit()
        {
            Application.Quit();
        }

        private void OnDisable()
        {
            // Unsubscribe from network events when disabled.
            NetworkManager.OnProcessFailed -= NetworkManager_OnProcessFailed;
            NetworkManager.OnDoProcess -= NetworkManager_OnDoProcess;
        }
    }
}
