using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.Singletons;
using UnityEngine;

namespace Mikrocosmos
{
    public class AudioInstance<T> : MonoBehaviour, ISingleton where T : AudioInstance<T> {

        private AudioSource audioSource;
        public static AudioSource Singleton {
            get {
                return SingletonProperty<AudioInstance<T>>.Singleton.audioSource;
            }
        }

        private void Awake() {
            audioSource = GetComponent<AudioSource>();
            DontDestroyOnLoad(gameObject);
        }

        public void OnSingletonInit() {
            
        }
    }
}
