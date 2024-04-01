using RPG.Sounds;
using System.Collections;
using UnityEngine;

namespace FPS.Animations
{
    public class FootAnimationEvents : MonoBehaviour
    {
        public SoundEffectSO footstepSound;
        [SerializeField] private AudioSource footstepSoundSource;
        private PlayerController playerController;
        private void Awake()
        {
            playerController = GetComponentInParent<PlayerController>();
        }
        void FootL()
        {
            Footstep();
        }
        void FootR()
        {
            Footstep();
        }
        void Footstep()
        {
            // Check if player is on ground by raycasting down
            RaycastHit hit;
            if (Physics.Raycast(transform.position + new Vector3(0,0.2f,0), Vector3.down, out hit, 1.1f))
            {
                footstepSound.Play(footstepSoundSource);
            }
        }
    }
}