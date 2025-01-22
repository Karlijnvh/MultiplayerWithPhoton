using UnityEngine;
using TMPro;

namespace PV.Multiplayer
{
    public class GameUIManager : MonoBehaviour
    {
        public static GameUIManager Instance { get; private set; }

        [SerializeField] private TextMeshProUGUI playerName;

        private void Awake()
        {
            Instance = this;
        }
    }
}
