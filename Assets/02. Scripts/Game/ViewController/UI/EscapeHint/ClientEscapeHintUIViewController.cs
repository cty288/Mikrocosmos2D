using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mikrocosmos
{
    public class ClientEscapeHintUIViewController : MonoBehaviour {
        private Animator animator;
        private ParticleSystem fastClickingParticle;
        private ParticleSystem slowClickingParticle;
        private Animator mouseAnimator;

        private void Awake() {
            animator = GetComponent<Animator>();
            fastClickingParticle = transform.Find("ClickBG/Particle  - Fast Clicking").GetComponent<ParticleSystem>();
            slowClickingParticle = transform.Find("ClickBG/Particle  - Slow Clicking").GetComponent<ParticleSystem>();
            mouseAnimator = transform.Find("ClickBG/Child/Mouse").GetComponent<Animator>();
        }

        
    }
}
