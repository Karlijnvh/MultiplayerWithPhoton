using Photon.Pun;
using UnityEngine;

namespace PV.Multiplayer
{
    public class Weapon : MonoBehaviour
    {
        public int damage = 10;
        public float fireRate = 0.5f;
        public float hitDistance = 100;
        public Transform shootPoint;
        public GameObject shootParticle;
        public GameObject hitParticle;
        public LayerMask hitLayer;

        private RaycastHit _hit;
        private PlayerController _player;

        public void Fire()
        {
            if (Physics.Raycast(shootPoint.position, shootPoint.forward, out _hit, hitDistance, hitLayer, QueryTriggerInteraction.Ignore))
            {
                if (_hit.transform.TryGetComponent(out _player))
                {
                    _player.photonView.RPC(nameof(_player.TakeDamage), RpcTarget.AllBuffered, damage, PhotonNetwork.NickName);
                }
            }
        }
    }
}
