using UnityEngine;

namespace InfimaGames.LowPolyShooterPack
{
    public class Weapon : WeaponBehaviour
    {
        #region FIELDS SERIALIZED
        
        [Header("Firing")]
        [SerializeField] private float minArrowSpeed = 10.0f;
        [SerializeField] private float maxArrowSpeed = 50.0f;
        [SerializeField] private float maxChargeTime = 2.0f;


        [Header("Animation")]
        [SerializeField] public RuntimeAnimatorController controller;

        [Header("Resources")]
        [SerializeField] private GameObject prefabProjectile;
        [SerializeField] private AudioClip audioClipDraw;
        [SerializeField] private AudioClip audioClipShoot;

        #endregion

        #region FIELDS

        private Animator animator;
        private float chargeTime = 0.0f;
        private Transform MainCamera;

        #endregion

        #region UNITY
        
        protected override void Awake()
        {
            animator = GetComponent<Animator>();

            var gameModeService = ServiceLocator.Current.Get<IGameModeService>();
            var characterBehaviour = gameModeService.GetPlayerCharacter();
            MainCamera = characterBehaviour.GetCameraWorld().transform;
        }

        protected override void Update()
        {
            if (Input.GetButton("Fire1")) 
            {
                chargeTime += Time.deltaTime;
                chargeTime = Mathf.Clamp(chargeTime, 0, maxChargeTime);
            }

            if (Input.GetButtonUp("Fire1")) 
            {
                Fire(1.0f); // ✅ Fix: Use correct method signature
                chargeTime = 0;
            }
        }

        #endregion

        #region METHODS

        public override void Fire(float spreadMultiplier) // ✅ Fix: Correct method signature
        {
            animator.Play("Fire", 0, 0.0f);
            AudioSource.PlayClipAtPoint(audioClipShoot, transform.position);

            float finalSpeed = Mathf.Lerp(minArrowSpeed, maxArrowSpeed, chargeTime / maxChargeTime);
            Quaternion rotation = Quaternion.LookRotation(MainCamera.forward * 1000.0f);

            GameObject projectile = Instantiate(prefabProjectile, transform.position, rotation);
            Rigidbody rb = projectile.GetComponent<Rigidbody>();
            rb.useGravity = true;
            rb.linearVelocity = projectile.transform.forward * finalSpeed + Vector3.up * 1.5f;
        }

        public override void Reload()
        {
            Debug.Log("Bows don't reload!");
        }

        public override void EjectCasing() 
        {
            // Bows don’t eject casings, so do nothing
        }

        public override void FillAmmunition(int amount) 
        {
            // Bows have infinite arrows unless a quiver system is added
        }

        #endregion

        #region GETTERS

        public override Animator GetAnimator() => animator;
        public override RuntimeAnimatorController GetAnimatorController() => controller;
        public override bool IsAutomatic() => false;
        public override float GetRateOfFire() => 0;
        public override int GetAmmunitionTotal() => -1;
        public override Sprite GetSpriteBody() => null;
        public override WeaponAttachmentManagerBehaviour GetAttachmentManager() => null;
        public override AudioClip GetAudioClipFire() => audioClipShoot;
        public override AudioClip GetAudioClipFireEmpty() => null;
        public override AudioClip GetAudioClipReload() => null;
        public override AudioClip GetAudioClipReloadEmpty() => null;
        public override AudioClip GetAudioClipUnholster() => null;
        public override AudioClip GetAudioClipHolster() => null; // ✅ Fix: Implement missing method
        public override int GetAmmunitionCurrent() => -1; // ✅ Fix: Implement missing method
        public override bool HasAmmunition() => true; // ✅ Fix: Implement missing method
        public override bool IsFull() => true; // ✅ Fix: Implement missing method


        #endregion
    }
}
