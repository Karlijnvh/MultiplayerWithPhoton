using UnityEngine;
using TMPro;

namespace PV.Multiplayer
{
    public class UIManager : MonoBehaviour
    {
        public GameObject lobby;
        public GameObject background;
        public TextMeshProUGUI feedbackMessage;
        public TMP_InputField playerNameField;
        public TMP_InputField roomNameField;

        private void OnEnable()
        {
            NetworkManager.OnProcessFailed += NetworkManager_OnProcessFailed;
            NetworkManager.OnDoProcess += NetworkManager_OnDoProcess;
        }

        public void JoinRoom()
        {
            feedbackMessage.text = "Connecting...";
            feedbackMessage.gameObject.SetActive(true);
            lobby.SetActive(false);
            NetworkManager.Instance.Connect(roomNameField.text);
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

        private void OnDisable()
        {
            NetworkManager.OnProcessFailed -= NetworkManager_OnProcessFailed;
            NetworkManager.OnDoProcess -= NetworkManager_OnDoProcess;
        }
    }
}
