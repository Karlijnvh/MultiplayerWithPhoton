using TMPro;
using UnityEngine;

namespace PV.Multiplayer
{
    public class PlayerItem : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _name;
        [SerializeField] private TextMeshProUGUI _status;

        public void InitItem(string name)
        {
            _name.text = name;
            _status.text = "Not Ready";
        }

        public void SetStatus(bool isReady) => _status.text = isReady ? "Ready" : "Not Ready";
    }
}
