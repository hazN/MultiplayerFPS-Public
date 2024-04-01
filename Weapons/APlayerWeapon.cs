using FishNet.Object;
using FPS.Player;
using UnityEngine;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using FishNet.Component.Animating;
using System.Collections;
using RPG.Sounds;
using FPS.GameManager;
using Cinemachine;
using MoreMountains.Feedbacks;

namespace FPS.Weapons
{
    public abstract class APlayerWeapon : NetworkBehaviour
    {
        [BoxGroup("Weapon Stats")]
        public int damage = 10;
        [BoxGroup("Weapon Stats")]
        public int range = 100;
        [BoxGroup("Weapon Stats")]
        public float fireRate = 0.5f;
        [BoxGroup("Weapon Stats")]
        public float weaponSpread = 0.1f;
        [BoxGroup("Weapon Stats")]
        public int maxAmmo = 10;
        private int currentAmmo;
        [BoxGroup("Weapon Stats")]
        public float reloadTime = 1.2f;
        [BoxGroup("Weapon Config")]
        public LayerMask weaponHitLayers;
        [BoxGroup("Weapon Config")]
        [SerializeField] private ParticleSystem muzzleFlash, bloodParticles, terrainHitParticle;
        [BoxGroup("Weapon Config")]
        [SerializeField] private GameObject bulletHole;
        [BoxGroup("Weapon Config")]
        [SerializeField] private SoundEffectSO fireSound, reloadSound;
        [BoxGroup("Weapon Config")]
        [SerializeField] private MMFeedbacks mmfFeedback;

        private AudioSource audioSource;
        public Transform rightHandIKTarget, leftHandIKTarget;
        private Transform cameraTransform;
        private float lastFireTime;
        private NetworkAnimator networkAnimator;
        private Animator anim;
        bool isReloading = false;
        private void Awake()
        {
            cameraTransform = transform.parent.GetComponentInChildren<Camera>().transform;
            networkAnimator = GetComponent<NetworkAnimator>();
            anim = GetComponent<Animator>();
            anim.speed = 1 / fireRate;
            audioSource = GetComponent<AudioSource>();
            currentAmmo = maxAmmo;
        }

        public void Fire()
        {
            // Make sure mouse is not over UI
            if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }
            if (currentAmmo <= 0 || isReloading)
            {
                return;
            }
            // Control fire rate
            if (Time.time < lastFireTime + fireRate)
            {
                return;
            }

            lastFireTime = Time.time;
            AnimateWeapon();
            //mmfFeedback.PlayFeedbacks();
            currentAmmo--;
            GameUIManager.SetAmmoText($"{currentAmmo}/{maxAmmo}");
            // Apply slight randomization for raycast using spread
            if (Physics.Raycast(cameraTransform.position, cameraTransform.forward + Random.insideUnitSphere * weaponSpread, out RaycastHit hit, range, weaponHitLayers))
            {
                if (hit.collider.TryGetComponent(out PlayerHealth playerHealth))
                {
                    playerHealth.TakeDamage(damage, OwnerId);
                    // Create a hit particle and destroy it after 3 seconds
                    StartCoroutine(DelayedEffect(hit, bloodParticles, 3));
                    return;
                }
                // Create a hit particle and destroy it after 3 seconds
                StartCoroutine(DelayedEffect(hit, terrainHitParticle, 3));
                StartCoroutine(DelayedEffect(hit, bulletHole, 30));
            }
        }
        public int GetAmmo()
        {
            return currentAmmo;
        }
        public void Reload()
        {
            if (currentAmmo == maxAmmo)
                return;
            isReloading = true;
            StartCoroutine(ReloadWeapon());
        }
        public IEnumerator ReloadWeapon()
        {
            networkAnimator.SetTrigger("Reload");
            reloadSound.Play(audioSource);
            yield return new WaitForSeconds(reloadTime);
            currentAmmo = maxAmmo;
            GameUIManager.SetAmmoText($"{currentAmmo}/{maxAmmo}");
            isReloading = false;
        }
        public IEnumerator DelayedEffect(RaycastHit hit, GameObject effect, float lifetime)
        {
            yield return new WaitForSeconds(fireRate);
            Destroy(Instantiate(effect, hit.point + (hit.normal * 0.01f), Quaternion.FromToRotation(Vector3.up, hit.normal)), lifetime);
        }
        public IEnumerator DelayedEffect(RaycastHit hit, ParticleSystem effect, float lifetime)
        {
            yield return new WaitForSeconds(fireRate);
            Destroy(Instantiate(effect, hit.point, Quaternion.LookRotation(hit.normal)).gameObject, lifetime);
        }

        [ServerRpc]
        public virtual void AnimateWeapon()
        {
            AnimateWeaponClient();
        }
        [ObserversRpc]
        public virtual void AnimateWeaponClient()
        {
            muzzleFlash.Play();
            networkAnimator.SetTrigger("Fire");
            fireSound.Play(audioSource);
        }
    }
}