using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace ChemistryGame.Core
{
    /// <summary>Singleton audio system: 1 source music + pool source SFX. Volume từ PlayerData.Settings.</summary>
    [DefaultExecutionOrder(-100)]
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Mixer (optional)")]
        [SerializeField] private AudioMixer mixer;

        [Header("Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private int sfxPoolSize = 8;

        [Header("Library — assign in inspector or via SO")]
        [SerializeField] private List<NamedClip> clips = new List<NamedClip>();

        private readonly Dictionary<string, AudioClip> _byId = new Dictionary<string, AudioClip>();
        private readonly List<AudioSource> _sfxPool = new List<AudioSource>();
        private int _poolIdx;

        [System.Serializable]
        public class NamedClip
        {
            public string Id;
            public AudioClip Clip;
            [Range(0f, 1f)] public float Volume = 1f;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.loop = true;
                musicSource.playOnAwake = false;
            }

            for (int i = 0; i < sfxPoolSize; i++)
            {
                var s = gameObject.AddComponent<AudioSource>();
                s.loop = false; s.playOnAwake = false;
                _sfxPool.Add(s);
            }

            foreach (var nc in clips)
            {
                if (nc?.Clip == null || string.IsNullOrEmpty(nc.Id)) continue;
                _byId[nc.Id] = nc.Clip;
            }
        }

        private void Start()
        {
            ApplyVolumes();
        }

        public void Register(string id, AudioClip clip)
        {
            if (clip == null || string.IsNullOrEmpty(id)) return;
            _byId[id] = clip;
        }

        public void ApplyVolumes()
        {
            if (SaveSystem.Current?.Settings == null) return;
            var s = SaveSystem.Current.Settings;
            if (musicSource != null) musicSource.volume = s.MusicVol;
            foreach (var sfx in _sfxPool) sfx.volume = s.SfxVol;
        }

        public void PlaySfx(string id, float volScale = 1f)
        {
            if (string.IsNullOrEmpty(id)) return;
            if (!_byId.TryGetValue(id, out var clip) || clip == null) return;
            var src = _sfxPool[_poolIdx];
            _poolIdx = (_poolIdx + 1) % _sfxPool.Count;
            var s = SaveSystem.Current?.Settings;
            src.pitch = 1f;
            src.PlayOneShot(clip, (s?.SfxVol ?? 0.85f) * volScale);
        }

        public void PlaySfx(AudioClip clip, float volScale = 1f)
        {
            if (clip == null) return;
            var src = _sfxPool[_poolIdx];
            _poolIdx = (_poolIdx + 1) % _sfxPool.Count;
            var s = SaveSystem.Current?.Settings;
            src.PlayOneShot(clip, (s?.SfxVol ?? 0.85f) * volScale);
        }

        public void PlayMusic(string id, bool loop = true)
        {
            if (!_byId.TryGetValue(id, out var clip) || clip == null) return;
            PlayMusic(clip, loop);
        }

        public void PlayMusic(AudioClip clip, bool loop = true)
        {
            if (clip == null) return;
            if (musicSource.clip == clip && musicSource.isPlaying) return;
            musicSource.clip = clip;
            musicSource.loop = loop;
            musicSource.Play();
        }

        public void StopMusic()
        {
            musicSource.Stop();
        }

        public void SetMusicVolume(float v)
        {
            if (SaveSystem.Current != null) SaveSystem.Current.Settings.MusicVol = v;
            if (musicSource != null) musicSource.volume = v;
        }

        public void SetSfxVolume(float v)
        {
            if (SaveSystem.Current != null) SaveSystem.Current.Settings.SfxVol = v;
            foreach (var s in _sfxPool) s.volume = v;
        }
    }
}
