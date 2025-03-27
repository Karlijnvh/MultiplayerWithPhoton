using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PV.Multiplayer
{
    /// <summary>
    /// Handles the player's UI elements including health bar updates and reticle control.
    /// </summary>
    public class PlayerUI : MonoBehaviour
    {
        [Header("Health Bar Settings")]
        [Tooltip("Slider representing the player's health.")]
        public Slider slider;
        [Tooltip("Gradient used to color the health bar.")]
        public Gradient gradient;
        [Tooltip("Image component of the health bar fill.")]
        public Image fill;

        [Header("Reticle Settings")]
        [Tooltip("The reticle GameObject for aiming visuals.")]
        [SerializeField] private GameObject reticle;
        
        // A small delay before enabling/disabling the reticle.
        private readonly WaitForSeconds waitSeconds = new(0.15f);

        /// <summary>
        /// Sets the maximum health value for the slider and initializes the color.
        /// </summary>
        /// <param name="health">The maximum health value.</param>
        public void SetMaxHealth(int health)
        {
            slider.maxValue = health;
            slider.value = health;
            fill.color = gradient.Evaluate(1f);
        }

        /// <summary>
        /// Updates the health value and refreshes the health bar color.
        /// </summary>
        /// <param name="health">The current health value.</param>
        public void SetHealth(int health)
        {
            slider.value = health;
            fill.color = gradient.Evaluate(slider.normalizedValue);
        }

        /// <summary>
        /// Enables or disables the reticle after a slight delay.
        /// </summary>
        /// <param name="enable">True to enable the reticle, false to disable it.</param>
        public void EnableReticle(bool enable)
        {
            StartCoroutine(SetReticle(enable));
        }

        /// <summary>
        /// Coroutine to control the reticle's active state after a brief delay.
        /// </summary>
        private IEnumerator SetReticle(bool enable)
        {
            yield return waitSeconds;
            reticle.SetActive(enable);
        }
    }
}
