using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
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

        [Tooltip("The log text UI element.")]
        public TextMeshProUGUI logText;

        // Reference to the PhotonView for network synchronization.
        internal PhotonView photonView;

        // Queue to manage log messages in the UI.
        private Queue<string> _logs = new();

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
            // Ensure only the MasterClient broadcasts the log message to all players.
            if (PhotonNetwork.IsMasterClient)
            {
                photonView.RPC(nameof(Log), RpcTarget.All, $"{attackerName} killed {victimName}.");
            }
        }

        /// <summary>
        /// Logs a message indicating a player has spawned.
        /// </summary>
        /// <param name="playerName">The name of the spawned player.</param>
        public void LogSpawned(string playerName)
        {
            // Ensure only the MasterClient broadcasts the log message to all players.
            if (PhotonNetwork.IsMasterClient)
            {
                photonView.RPC(nameof(Log), RpcTarget.All, $"{playerName} spawned.");
            }
        }

        /// <summary>
        /// Logs a message indicating a player has left the game.
        /// </summary>
        /// <param name="playerName">The name of the player who left.</param>
        public void LogLeft(string playerName)
        {
            // Ensure only the MasterClient broadcasts the log message to all players.
            if (PhotonNetwork.IsMasterClient)
            {
                photonView.RPC(nameof(Log), RpcTarget.All, $"{playerName} left.");
            }
        }

        /// <summary>
        /// Handles the creation and display of a log message in the UI.
        /// </summary>
        /// <param name="message">The message to display in the log.</param>
        [PunRPC]
        private void Log(string message)
        {
            if (logText != null)
            {
                _logs.Enqueue(message);
                UpdateLog(); // Update the displayed log.

                // Calculate the destroy delay based on the number of visible logs.
                float delay = logDuration;
                if (_logs.Count > numberOfVisibleLogs)
                {
                    int diff = Mathf.FloorToInt(_logs.Count / numberOfVisibleLogs) - 1;
                    delay = logDuration * (2 + diff);
                }

                // Start a coroutine to remove the log after the calculated delay.
                StartCoroutine(RemoveLog(delay));
            }
        }

        private void UpdateLog()
        {
            // Combine all log messages into a single string and update the UI.
            logText.text = string.Join("\n", _logs.ToArray());
        }

        private IEnumerator RemoveLog(float delay)
        {
            yield return new WaitForSeconds(delay);

            if (_logs.Count > 0)
            {
                // Remove the oldest log and refresh the displayed logs.
                _logs.Dequeue();
                UpdateLog();
            }
        }
    }
}
