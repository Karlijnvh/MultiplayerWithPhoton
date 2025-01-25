using Photon.Pun;
using UnityEngine;

namespace PV.Multiplayer
{
    public class Weapon : MonoBehaviour
    {
        [Header("Weapon Properties")]
        [Tooltip("The amount of damage dealt by the weapon.")]
        public int damage = 10;
        [Tooltip("The rate of fire (times it can fire in 1 second).")]
        public float fireRate = 2;
        [Tooltip("The maximum distance the weapon's raycast can reach.")]
        public float hitDistance = 100;

        [Header("Weapon Components")]
        [Tooltip("The point from which the weapon will shoot (origin of the raycast).")]
        public Transform shootPoint;
        [Tooltip("Particle effect to display when firing.")]
        public GameObject shootParticle;
        [Tooltip("Particle effect to display when a target is hit.")]
        public GameObject hitParticle;
        [Tooltip("The layers that the weapon's raycast can hit.")]
        public LayerMask hitLayer;

        // Stores information about the object hit by the raycast.
        private RaycastHit _hit;
        // Reference to the PlayerController component of the hit target.
        private PlayerController _player;

        /// <summary>
        /// Fires the weapon, performing a raycast to detect and damage targets within range.
        /// </summary>
        public void Fire()
        {
            // Perform a raycast from the shoot point in the forward direction.
            if (Physics.Raycast(shootPoint.position, shootPoint.forward, out _hit, hitDistance, hitLayer, QueryTriggerInteraction.Ignore))
            {
                // Check if the hit object has a PlayerController component.
                if (_hit.transform.TryGetComponent(out _player))
                {
                    // Send the damage and player info to all clients.
                    _player.photonView.RPC(nameof(_player.TakeDamage), RpcTarget.AllBuffered, damage, PhotonNetwork.NickName);
                }
            }
        }
    }
}
