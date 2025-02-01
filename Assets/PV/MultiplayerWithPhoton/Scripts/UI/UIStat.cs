using Photon.Realtime;
using TMPro;
using UnityEngine;

namespace PV.Multiplayer
{
    public class UIStat : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI playerName;
        [SerializeField] private TextMeshProUGUI kills;
        [SerializeField] private TextMeshProUGUI deaths;
        [SerializeField] private TextMeshProUGUI score;

        private Stats _stats;

        [HideInInspector]
        public int playerNumber = -1;

        public void Enable()
        {
            gameObject.SetActive(true);
        }

        public void Disable()
        {
            gameObject.SetActive(false);
        }

        public void InitData(PlayerController player)
        {
            _stats = player.stats;
            playerNumber = player.photonView.Owner.ActorNumber;
            playerName.text = player.photonView.Owner.NickName;
            kills.text = player.stats.Kills.ToString();
            deaths.text = player.stats.Deaths.ToString();
            score.text = player.stats.Score.ToString();
        }

        public void UpdateData()
        {
            if (playerNumber == -1 || _stats == null)
            {
                return;
            }

            kills.text = _stats.Kills.ToString();
            deaths.text = _stats.Deaths.ToString();
            score.text = _stats.Score.ToString();
        }
    }
}
