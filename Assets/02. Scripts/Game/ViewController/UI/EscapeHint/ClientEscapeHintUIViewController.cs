using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
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
        private bool startDetectMouseClickSpeed = false;
        private float mouseClickTimer = 0;
        private void Awake() {
            animator = GetComponent<Animator>();
            fastClickingParticle = transform.Find("ClickBG/Particle  - Fast Clicking").GetComponent<ParticleSystem>();
            slowClickingParticle = transform.Find("ClickBG/Particle - Slow Clicking").GetComponent<ParticleSystem>();
            mouseAnimator = transform.Find("ClickBG/Child/Mouse").GetComponent<Animator>();
            progressSlider = transform.Find("ClickBG/Child/Slider").GetComponent<Image>();
        }

        private void Start() {
            this.RegisterEvent<OnClientSpaceshipHooked>(OnClientHooked).UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<OnClientSpaceshipUnHooked>(OnClientUnHooked)
                .UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<OnEscapeCounterChanged>(OnEscapeCounterChanged)
                .UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        private void Update() {
            if (startDetectMouseClickSpeed) {
                //calculate mouse click speed
                mouseClickTimer += Time.deltaTime;
                if (Input.GetMouseButtonDown(1)) {
                    if (mouseClickTimer < 0.2f) {
                        if (!fastClickingParticle.isPlaying) {
                            fastClickingParticle.Play();
                        }

                        slowClickingParticle.Stop();
                        mouseAnimator.speed = 1.5f;
                    }
                    else {
                        if (!slowClickingParticle.isPlaying)
                        {
                            slowClickingParticle.Play();
                        }
                        fastClickingParticle.Stop();
                        mouseAnimator.speed = 1;
                    }
                    mouseClickTimer = 0;
                }

                if (mouseClickTimer > 0.2f)
                {
                    if (!slowClickingParticle.isPlaying)
                    {
                        slowClickingParticle.Play();
                    }
                    fastClickingParticle.Stop();
                    mouseAnimator.speed = 1;
                }
            }
        }

        private void OnEscapeCounterChanged(OnEscapeCounterChanged e) {
            if (progressSlider && e.hasAuthority) {
                progressSlider.DOFillAmount(e.newValue / 10f, 0.1f);
            }
        }

        private void OnClientUnHooked(OnClientSpaceshipUnHooked e) {
            if (e.hasAuthority) {
                animator.SetTrigger("End");
                mouseAnimator.speed = 1;
                startDetectMouseClickSpeed = false;
            }
        
        }

        private void OnClientHooked(OnClientSpaceshipHooked e) {
            if (e.hasAuthority) {
                animator.SetTrigger("Start");
                mouseAnimator.speed = 1;
                progressSlider.fillAmount = 0;
                startDetectMouseClickSpeed = true;
                mouseClickTimer = 0;
            }
        
        }
    }
}
