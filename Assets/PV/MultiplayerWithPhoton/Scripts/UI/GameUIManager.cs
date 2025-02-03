using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PV.Multiplayer
{
    public class GameUIManager : MonoBehaviourPunCallbacks
    {
        public static GameUIManager Instance { get; private set; }

        [Header("Logging")]
        [Tooltip("Duration each log message stays visible.")]
        public float logDuration = 3;

        [Tooltip("Maximum number of visible log messages at a time.")]
        public int numberOfVisibleLogs = 4;

        [Tooltip("The log text UI element.")]
        public TextMeshProUGUI logText;

        [Tooltip("List of player stat's ui representation.")]
        public GameObject uiStatPrefab;

        public Transform uiStatContainer;

        // Queue to manage log messages in the UI.
        private Queue<string> _logs = new();
        public Dictionary<int, UIStat> stats;

        private UIStat _uiStat;

        // Track the total player in room
        private int _playerCount = 0;
        private bool _isLeaving = false;

        private void Awake()
        {
            Instance = this;
            stats = new();
        }

        //public override void OnPlayerEnteredRoom(Player newPlayer)
        //{
        //    // Logs the event to the UI about the player who left.
        //    LogSpawned(newPlayer.NickName);
        //}

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            // Logs the event to the UI about the player who left.
            LogLeft(otherPlayer.NickName);
        }

        public override void OnLeftRoom()
        {
            // Return to the main menu when the local player leaves the room.
            SceneManager.LoadScene(0);
        }

        public override void OnMasterClientSwitched(Player newMasterClient)
        {
            // When Master leaves all players also leaves.
            if (!_isLeaving)
            {
                _isLeaving = true;
                // Initiate leaving the room.
                PhotonNetwork.LeaveRoom();
            }
        }

        public void LeaveRoom()
        {
            if (!_isLeaving)
            {
                _isLeaving = true;
                // Initiate leaving the room.
                PhotonNetwork.LeaveRoom();
            }
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

        public void SetStats(PlayerController player)
        {
            if (!stats.ContainsKey(player.photonView.Owner.ActorNumber))
            {
                _uiStat = Instantiate(uiStatPrefab, uiStatContainer).GetComponent<UIStat>();
                stats[player.photonView.Owner.ActorNumber] = _uiStat;
                stats[player.photonView.Owner.ActorNumber].InitData(player);
            }
        }

        public void UpdateStats(int playerNumber)
        {
            if (stats.Count > 0)
            {
                if (stats.ContainsKey(playerNumber))
                {
                    stats[playerNumber].UpdateData();
                }
            }
        }
    }
}
