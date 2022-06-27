using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MikroFramework.Architecture;
using MikroFramework.Event;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

namespace Mikrocosmos
{
    public class PlayerInfoEscapeIndicator : AbstractMikroController<Mikrocosmos> {
        private NetworkIdentity thisSpaceship;
        private Animator animator;
        private ParticleSystem fastClickingParticle;
        private Image progressSlider;

        private bool showOnClient = false;
        private void Awake() {
            thisSpaceship = GetComponentInParent<NetworkIdentity>();
            animator = GetComponent<Animator>();
            fastClickingParticle = transform.Find("Particle  - Fast Clicking").GetComponent<ParticleSystem>();
            progressSlider = transform.Find("Child/Slider").GetComponent<Image>();
        }

        private void Start() {
            this.RegisterEvent<OnClientHookAnotherSpaceship>(OnClientHookAnotherSpaceship).UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<OnEscapeCounterChanged>(OnEscapeCounterChanged)
                .UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<OnClientSpaceshipUnHooked>(OnSpaceshipUnHooked)
                .UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        private void OnSpaceshipUnHooked(OnClientSpaceshipUnHooked e) {
            if (showOnClient && e.identity == thisSpaceship && !e.hasAuthority) {
                animator.SetTrigger("End");
                showOnClient = false;
                fastClickingParticle.Stop();
            }
        }

        private void OnEscapeCounterChanged(OnEscapeCounterChanged e) {
            if (showOnClient && e.identity == thisSpaceship) {
                progressSlider.DOFillAmount(e.newValue / 10f, 0.1f);
            }
        }

        private void OnClientHookAnotherSpaceship(OnClientHookAnotherSpaceship e) {
            if (e.spaceship == thisSpaceship) {
                animator.SetTrigger("Start");
                progressSlider.fillAmount = 0;
                showOnClient = true;
                if (!fastClickingParticle.isPlaying) {
                    fastClickingParticle.Play();
                }
            }
        }
    }
}
