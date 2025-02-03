using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

namespace PV.Multiplayer
{
    public class RoomItem : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _name;
        [SerializeField] private TextMeshProUGUI _playerRatio;
        [SerializeField] private Button _button;

        public void InitItem(string roomName, string playerRatio, UnityAction onClickHandler)
        {
            _name.text = roomName;
            _playerRatio.text = playerRatio;
            _button.onClick.AddListener(onClickHandler);
        }

        public void SetPlayerRatio(string playerRatio) => _playerRatio.text = playerRatio;

        public void Enable() => _button.interactable = true;
        
        public void Disable() => _button.interactable = false;
    }
}
