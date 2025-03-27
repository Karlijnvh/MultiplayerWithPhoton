// Copyright 2021, Infima Games. All Rights Reserved.

using UnityEngine;

namespace InfimaGames.LowPolyShooterPack
{
    /// <summary>
    /// Handles animation events for the bow.
    /// </summary>
    public class WeaponAnimationEventHandler : MonoBehaviour
    {
        #region FIELDS

        /// <summary>
        /// Equipped Weapon.
        /// </summary>
        private WeaponBehaviour weapon;

        #endregion

        #region UNITY

        private void Awake()
        {
            // Cache weapon reference.
            weapon = GetComponent<WeaponBehaviour>();
        }

        #endregion
    }
}
