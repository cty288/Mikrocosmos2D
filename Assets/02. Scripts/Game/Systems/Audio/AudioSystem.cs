using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Mikrocosmos;
using MikroFramework.Architecture;
using MikroFramework.ResKit;
using MikroFramework.Singletons;
using UnityEngine;

namespace Mikrocosmos
{
    
    public enum SoundType {
        Sound2D,
        Sound3D
    }
    public interface IAudioSystem : ISystem {
        public float MusicVolume { get; set; }
        public float SoundVolume { get; set; }
        void Destroy();
        void PlaySound(AudioClip clip, SoundType soundType, float relativeVolume = 1f);
        void PlaySound(AudioClip clip, AudioSource audioSource, float relativeVolume = 1f);
        void PlaySound(string clipName, SoundType soundType, float relativeVolume = 1f);
        void PlaySound(string clipName, AudioSource audioSource, float relativeVolume = 1f);
        void PlayMusic(string clipPath, float relativeVolume = 1f);
        void PlayMusic(AudioClip clip, AudioSource audioSource, float relativeVolume = 1f);
        void PlayMusic(AudioClip clip, float relativeVolume = 1f);
        
    }
    public class AudioSystem : MonoPersistentMikroSingleton<AudioSystem>, IAudioSystem {
        private AudioSource bgm;
        private AudioSource sound2D;
        private AudioSource sound3D;
        
        private const string MusicVolumeStorageKey = "AudioSysMusicVolume";
        private const string SoundVolumeStorageKey = "AudioSysSoundVolume";

        private Dictionary<string, AudioClip> _soundClipDict;
        private Dictionary<string, AudioClip> _musicClipDict;

        private ResLoader resLoader;

        private void Start() {
            bgm = BGMAudioInstance.Singleton;
            sound2D = SoundAudioInstance2D.Singleton;
            sound3D = SoundAudioInstance3D.Singleton;
            ResLoader.Create((loader => resLoader = loader ));
            Mikrocosmos.Interface.RegisterSystem<IAudioSystem>(this);
            DontDestroyOnLoad(gameObject);
            bgm.volume = MusicVolume;
            sound2D.volume = SoundVolume;
            sound3D.volume = SoundVolume;
#if UNITY_EDITOR
            Debug.Log(string.Format("Current BGM and Sound Volume£º{0}£¬{1}", bgm.volume, sound2D.volume));
#endif
            _soundClipDict = new Dictionary<string, AudioClip>();
            _musicClipDict = new Dictionary<string, AudioClip>();
            
        }


        private IEnumerator _playMusicCoroutine;
        private IEnumerator PlayMusicCoroutine(AudioClip newClip, float relativeVolume)
        {
            float maxVolume = bgm.volume;
            float minVolume = 0f;
            float duration = 1f;
            float timer = 0f;
            while (timer < duration)
            {
                bgm.volume = Mathf.Lerp(maxVolume, minVolume, timer / duration);
                timer += Time.deltaTime;
                yield return null;
            }

            bgm.volume = minVolume;
            bgm.clip = newClip;
            bgm.Play();

            maxVolume = MusicVolume * relativeVolume;
            timer = 0f;
            while (timer < duration)
            {
                bgm.volume = Mathf.Lerp(minVolume, maxVolume, timer / duration);
                timer += Time.deltaTime;
                yield return null;
            }

        }
        private void StartPlayMusicCoroutine(AudioClip newClip, float relativeVolume)
        {
            _playMusicCoroutine = PlayMusicCoroutine(newClip, relativeVolume);
            StartCoroutine(_playMusicCoroutine);
        }
        private void StopPlayMusicCoroutine()
        {
            if (_playMusicCoroutine != null)
            {
                StopCoroutine(_playMusicCoroutine);
                _playMusicCoroutine = null;
            }
        }

        protected override void OnBeforeDestroy() {
            base.OnBeforeDestroy();
            resLoader.ReleaseAllAssets();
        }

        private void Play(string clipName, Dictionary<string, AudioClip> dict, AudioSource audioSource, float relativeVolume, Action<AudioClip, AudioSource, float> action)
        {
            if (!dict.ContainsKey(clipName)) {
                var clip = resLoader.LoadSync<AudioClip>("audio", clipName); 
                if (clip != null)
                {
                    dict.Add(clipName, clip);
#if UNITY_EDITOR
                    Debug.Log("Added Audio£º" + clipName);
#endif
                }
            }

            if (dict.ContainsKey(clipName))
            {
                action?.Invoke(dict[clipName], audioSource, relativeVolume);
            }
        }

        public IArchitecture GetArchitecture() {
            return Mikrocosmos.Interface;
        }

        private IArchitecture architecture;
        public void SetArchitecture(IArchitecture architecture) {
            this.architecture = architecture;
        }

        public void Init() {
          
        }

        [SerializeField] private bool debugMode = true;

        public float MusicVolume {
            get
            {
                if (debugMode) {
                    return 0.3f;
                }
                return ES3.Load<float>(MusicVolumeStorageKey, 0.3f);
            }
            set
            {
                value = Mathf.Clamp01(value);
                bgm.volume = value;
                if (debugMode) {
                    return;
                }
                ES3.Save<float>(MusicVolumeStorageKey, value);
            }
        }
        public float SoundVolume {
            get
            {
                if (debugMode)
                {
                    return 0.5f;
                }
                return ES3.Load<float>(SoundVolumeStorageKey, 0.5f);
            }
            set
            {
                value = Mathf.Clamp01(value);
                sound2D.volume = value;
                sound3D.volume = value;
                if (debugMode)
                {
                    return;
                }
                ES3.Save<float>(SoundVolumeStorageKey, value);
            }
        }
        public void Destroy() {
            _soundClipDict.Clear();
            _soundClipDict = null;

            _musicClipDict.Clear();
            _musicClipDict = null;
            resLoader.ReleaseAllAssets();
        }

        public void PlaySound(AudioClip clip, SoundType soundType, float relativeVolume = 1f){
            AudioSource source = soundType == SoundType.Sound2D ? sound2D : sound3D;
            PlaySound(clip, source, relativeVolume);
        }

        public void PlaySound(AudioClip clip, AudioSource audioSource, float relativeVolume = 1f){
        
            audioSource.volume = SoundVolume;
            audioSource.PlayOneShot(clip, relativeVolume);
        }

        public void PlaySound(string clipName, SoundType soundType, float relativeVolume = 1f){
            AudioSource source = soundType == SoundType.Sound2D ? sound2D : sound3D;
            PlaySound(clipName, source, relativeVolume);
        }

        public void PlaySound(string clipName, AudioSource audioSource, float relativeVolume = 1f) {
            audioSource.volume = SoundVolume;
            Play(clipName, _soundClipDict, audioSource, relativeVolume, PlaySound);
        }



        public void PlayMusic(string clipPath, float relativeVolume = 1f) {
            Play(clipPath, _musicClipDict, bgm, relativeVolume, PlayMusic);
        }

        public void PlayMusic(AudioClip clip, AudioSource audioSource, float relativeVolume = 1f) {
            PlayMusic(clip, relativeVolume);
        }

        public void PlayMusic(AudioClip clip, float relativeVolume = 1f) {
            StopPlayMusicCoroutine();

            if (bgm.clip == null) {
                bgm.clip = clip;
                bgm.loop = true;
                bgm.volume = MusicVolume * relativeVolume;
            }
            else
            {
                StartPlayMusicCoroutine(clip, relativeVolume);
            }
        }
    }
}
