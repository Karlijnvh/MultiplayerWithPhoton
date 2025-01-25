using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace PV.Multiplayer
{
    public class PlayerUI : MonoBehaviour
    {
        [Tooltip("UI slider to visually display the player's health.")]
        [SerializeField] private Slider healthSlider;

        [Tooltip("Text element to display the player's health as a numeric value.")]
        [SerializeField] private TextMeshProUGUI healthText;

        [Tooltip("The reticle GameObject for aiming visuals.")]
        [SerializeField] private GameObject reticle;

        [Tooltip("RectTransform for customizing reticle properties (e.g., positioning).")]
        [SerializeField] private RectTransform aimReticle;
        
        // Delay duration for enabling/disabling the reticle.
        private readonly WaitForSeconds waitSeconds = new(0.15f);

        /// <summary>
        /// Updates the health UI elements to reflect the current health value.
        /// </summary>
        /// <param name="health">The current health value of the player.</param>
        public void SetHealth(int health)
        {
            healthSlider.value = health;
            healthText.text = health.ToString();
        }

        /// <summary>
        /// Enables or disables the reticle after a slight delay.
        /// </summary>
        /// <param name="enable">True to enable the reticle, false to disable it.</param>
        public void EnableReticle(bool enable)
        {
            StartCoroutine(SetReticle(enable));
        }

        IEnumerator SetReticle(bool enable)
        {
            yield return waitSeconds;
            reticle.SetActive(enable);
        }

        /// <summary>
        /// Provides access to the RectTransform of the aim reticle.
        /// </summary>
        /// <returns>The RectTransform of the aim reticle.</returns>
        public RectTransform GetReticle()
        {
            return aimReticle;
        }
    }
}
