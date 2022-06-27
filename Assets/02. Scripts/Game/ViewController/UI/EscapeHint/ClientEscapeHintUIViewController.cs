using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using MikroFramework.Event;
using UnityEngine;
using UnityEngine.UI;

namespace Mikrocosmos
{
    public class ClientEscapeHintUIViewController : AbstractMikroController<Mikrocosmos> {
        private Animator animator;
        private ParticleSystem fastClickingParticle;
        private ParticleSystem slowClickingParticle;
        private Animator mouseAnimator;
        private Image progressSlider;

        private void Awake() {
            animator = GetComponent<Animator>();
            fastClickingParticle = transform.Find("ClickBG/Particle  - Fast Clicking").GetComponent<ParticleSystem>();
            slowClickingParticle = transform.Find("ClickBG/Particle - Slow Clicking").GetComponent<ParticleSystem>();
            mouseAnimator = transform.Find("ClickBG/Child/Mouse").GetComponent<Animator>();
            progressSlider = transform.Find("ClickBG/Child/Slider").GetComponent<Image>();
        }

        private void Start() {
            this.RegisterEvent<OnClientSpaceshipHooked>(OnClientHooked).UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        private void OnClientHooked(OnClientSpaceshipHooked e) {
            animator.SetTrigger("Start");
            mouseAnimator.speed = 1;
            progressSlider.fillAmount = 0;
        }
    }
}
