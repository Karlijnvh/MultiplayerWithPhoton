using Photon.Pun;
using TMPro;
using UnityEngine;

namespace PV.Multiplayer
{
    public class GameUIManager : MonoBehaviour
    {
        public static GameUIManager Instance { get; private set; }

        [Header("Logging")]
        [Tooltip("Duration each log message stays visible.")]
        public float logDuration = 3;

        [Tooltip("Maximum number of visible log messages at a time.")]
        public int numberOfVisibleLogs = 4;

        [Tooltip("Container for holding log messages.")]
        public Transform logContainer;

        [Tooltip("Prefab for the log text UI element.")]
        public GameObject logTextPrefab;

        // Reference to the latest instantiated log message.
        private TextMeshProUGUI _logText;
        // Reference to the PhotonView for network synchronization.
        internal PhotonView photonView;

        private const string UIPath = "UI/";

        private void Awake()
        {
            Instance = this;
            photonView = GetComponent<PhotonView>();
        }

        /// <summary>
        /// Logs a message indicating a player was killed by another player.
        /// </summary>
        /// <param name="attackerName">The name of the attacking player.</param>
        /// <param name="victimName">The name of the victim player.</param>
        public void LogKilled(string attackerName, string victimName)
        {
            Log($"{attackerName} killed {victimName}.");
        }

        /// <summary>
        /// Logs a message indicating a player has spawned.
        /// </summary>
        /// <param name="playerName">The name of the spawned player.</param>
        [PunRPC]
        public void LogSpawned(string playerName)
        {
            Log($"{playerName} spawned.");
        }

        /// <summary>
        /// Logs a message indicating a player has left the game.
        /// </summary>
        /// <param name="playerName">The name of the player who left.</param>
        public void LogLeft(string playerName)
        {
            Log($"{playerName} left.");
        }

        /// <summary>
        /// Handles the creation and display of a log message in the UI.
        /// </summary>
        /// <param name="message">The message to display in the log.</param>
        private void Log(string message)
        {
            if (logTextPrefab != null && logContainer != null)
            {
                // Instantiate the log text prefab and set it as a child of the log container.
                _logText = PhotonNetwork.Instantiate(UIPath + logTextPrefab.name, Vector3.zero, Quaternion.identity)
                    .GetComponent<TextMeshProUGUI>();
                _logText.transform.SetParent(logContainer);

                // Set the message text.
                _logText.text = message;

                // Calculate the destroy delay based on the number of visible logs.
                float destroyDelay = logDuration;
                if (logContainer.childCount > numberOfVisibleLogs)
                {
                    int diff = Mathf.FloorToInt(logContainer.childCount / numberOfVisibleLogs) - 1;
                    destroyDelay = logDuration * (2 + diff);
                }

                // Destroy the log message after the calculated delay.
                Destroy(_logText.gameObject, destroyDelay);
            }
        }
    }
}
