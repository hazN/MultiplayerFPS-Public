using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

namespace RPG.Sounds
{
    [CreateAssetMenu(fileName = "New Sound Effect", menuName = "RPG/New Sound Effect")]
    public class SoundEffectSO : ScriptableObject
    {
        [Required]
        [SerializeField]
        private AudioClip[] clips = null;
        [SerializeField] private AudioMixerGroup mixerGroup = null;
        [BoxGroup("Config")] [MinMaxSlider(0,1)] [SerializeField]
        private Vector2 volume = new Vector2(0.5f, 0.5f);

        [BoxGroup("Config")] [MinMaxSlider(0,3)] [SerializeField]
        private Vector2 pitch = new Vector2(1f, 1f);
        enum SoundClipPlayOrder { random, in_order, reversed }
        [BoxGroup("Config")] [SerializeField] 
        private SoundClipPlayOrder playOrder = SoundClipPlayOrder.random;
        [BoxGroup("Config")][DisplayAsString][SerializeField] private int playIndex = 0;
        public AudioClip GetAudioClip()
        {
            // Grab current clip
            AudioClip clip = clips[playIndex >= clips.Length ? 0 : playIndex];

            // Get next clip
            switch (playOrder)
            {
                case SoundClipPlayOrder.random:
                    playIndex = Random.Range(0, clips.Length);
                    break;
                case SoundClipPlayOrder.in_order:
                    playIndex = (playIndex + 1) % clips.Length;
                    break;
                case SoundClipPlayOrder.reversed:
                    playIndex = (playIndex - 1) % clips.Length;
                    break;
            }

            return clip;
        }

#if UNITY_EDITOR
        private AudioSource previewer;

        private void OnEnable()
        {
            previewer = EditorUtility.CreateGameObjectWithHideFlags("AudioPreview", HideFlags.HideAndDontSave, typeof(AudioSource)).GetComponent<AudioSource>();
        }

        private void OnDisable()
        {
            DestroyImmediate(previewer.gameObject);
        }
        [ButtonGroup("previewControls")]
        [GUIColor(0.4f, 0.8f, 1f)]
        [Button(ButtonSizes.Large)]
        private void PlayPreview()
        {
            Play(previewer);
        }
        [ButtonGroup("previewControls")]
        [GUIColor(1f, 0.4f, 0.4f)]
        [Button(ButtonSizes.Large)]
        [EnableIf("@previewer.isPlaying")]
        private void StopPreview()
        {
            previewer.Stop();
        }

#endif

        public AudioSource Play(AudioSource audioSourceParam = null)
        {
            if (clips.Length == 0)
            {
                Debug.LogWarning($"Missing sound clips for {name}");
                return null;
            }
            var source = audioSourceParam;
            bool destroySource = false;
            if (source == null)
            {
                destroySource = true;
                var _obj = new GameObject("Sound", typeof(AudioSource));
                source = _obj.GetComponent<AudioSource>();
            }
            // Set volume and pitch
            if (mixerGroup != null)
                source.outputAudioMixerGroup = mixerGroup;
            source.clip = GetAudioClip();
            source.volume = Random.Range(volume.x, volume.y);
            source.pitch = Random.Range(pitch.x, pitch.y);

            source.Play();
#if UNITY_EDITOR
            if (source != previewer && destroySource)
            {
                Destroy(source, source.clip.length / source.pitch);
            }
#else
           if (destroySource)
            Destroy(source, source.clip.length / source.pitch);
#endif
            return source;
        }
    }
}