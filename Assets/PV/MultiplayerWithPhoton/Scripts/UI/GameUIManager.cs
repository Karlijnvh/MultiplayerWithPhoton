using Photon.Pun;
using TMPro;
using UnityEngine;

namespace PV.Multiplayer
{
    public class GameUIManager : MonoBehaviour
    {
        public static GameUIManager Instance { get; private set; }

        [Header("Logging")]
        public float logDuration = 3;
        public int numberOfVisibleLogs = 4;
        public Transform logContainer;
        public GameObject logTextPrefab;

        private TextMeshProUGUI _logText;

        private void Awake()
        {
            Instance = this;
        }

        public void Log(string message)
        {
            if (logTextPrefab != null && logContainer != null)
            {
                _logText = PhotonNetwork.Instantiate(logTextPrefab.name, Vector3.zero, Quaternion.identity).GetComponent<TextMeshProUGUI>();
                _logText.transform.SetParent(logContainer);
                _logText.text = message;

                // Calculating destroy time
                float destroyDelay = logDuration;
                if (logContainer.childCount > numberOfVisibleLogs)
                {
                    int diff = Mathf.FloorToInt(logContainer.childCount / numberOfVisibleLogs) - 1;
                    destroyDelay = logDuration * (2 + diff);
                }

                Destroy(_logText.gameObject, destroyDelay);
            }
        }
    }
}
