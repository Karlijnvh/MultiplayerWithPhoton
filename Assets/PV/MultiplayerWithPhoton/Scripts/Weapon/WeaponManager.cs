using UnityEngine;

namespace PV.Multiplayer
{
    public class WeaponManager : MonoBehaviour
    {
        public Weapon primaryWeapon;

        private float _attackDelay = 0f;

        private InputManager Input 
        { 
            get 
            { 
                return InputManager.Instance; 
            } 
        }

        public void DoUpdate()
        {
            if (Input == null)
            {
                return;
            }

            if (Input.attack)
            {
                if (_attackDelay <= 0f)
                {
                    _attackDelay = primaryWeapon.fireRate;
                    primaryWeapon.Fire();
                }

                _attackDelay -= Time.deltaTime;
            }
            else if (_attackDelay > 0f)
            {
                _attackDelay = 0f;
            }
        }
    }
}
