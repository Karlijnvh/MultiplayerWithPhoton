using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace PV.Multiplayer
{
    public class PlayerUI : MonoBehaviour
    {
        [SerializeField] private Slider healthSlider;
        [SerializeField] private TextMeshProUGUI healthText;
        [SerializeField] private GameObject reticle;
        public RectTransform aimReticle;

        private readonly WaitForSeconds waitSeconds = new(0.15f);

        public void SetHealth(int health)
        {
            healthSlider.value = health;
            healthText.text = health.ToString();
        }

        public void EnableReticle(bool enable)
        {
            StartCoroutine(SetReticle(enable));
        }

        IEnumerator SetReticle(bool enable)
        {
            yield return waitSeconds;

            reticle.SetActive(enable);
        }
    }
}
