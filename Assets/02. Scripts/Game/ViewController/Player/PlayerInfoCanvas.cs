using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MikroFramework;
using MikroFramework.Architecture;
using MikroFramework.Event;
using MikroFramework.TimeSystem;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.Networking.Types;
using UnityEngine.UI;

namespace Mikrocosmos
{
    public class PlayerInfoCanvas : AbstractMikroController<Mikrocosmos> {
        [SerializeField] private Text playerNameText;

        private Image playerHealthBar;
        private Image playerHealthBarBG;

        [SerializeField]
        private float healthBarFadeTime = 10f;

        [SerializeField]
        private float healthBarFadeTimer = 0;
        private bool healthBarFadeWaiting = false;

        private Image crimeLevelImage;
        [SerializeField] private List<Sprite> crimeLevelSprites;

        private void Awake() {
        
            this.RegisterEvent<OnClientSpaceshipHealthChange>(OnSpaceshipHealthChange)
                .UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<OnClientSpaceshipCriminalityUpdate>(OnClientSpaceshipCriminalityUpdate)
                .UnRegisterWhenGameObjectDestroyed(gameObject);
            playerHealthBarBG = transform.Find("PlayerHealthBarBG").GetComponent<Image>();
            playerHealthBar = playerHealthBarBG.transform.Find("PlayerHealthBar").GetComponent<Image>();
            crimeLevelImage = transform.Find("CrimeLevelImage").GetComponent<Image>();
        }

        private void OnClientSpaceshipCriminalityUpdate(OnClientSpaceshipCriminalityUpdate e) {
            if (e.SpaceshipIdentity == GetComponentInParent<NetworkIdentity>()) {
                if (e.Criminality > 0) {
                    crimeLevelImage.gameObject.SetActive(true);
                    crimeLevelImage.sprite = crimeLevelSprites[Mathf.Min(e.Criminality - 1, 2)];
                }
                else {
                    crimeLevelImage.gameObject.SetActive(false);
                }
            }
        }

        private void OnSpaceshipHealthChange(OnClientSpaceshipHealthChange e) {
            if (e.Identity == GetComponentInParent<NetworkIdentity>()) {
                if (playerHealthBarBG.color.a < 1)
                {
                    playerHealthBarBG.DOFade(1, 0.3f);
                    playerHealthBar.DOFade(1, 0.3f).OnComplete(() => {
                        float healthPercentage = e.NewHealth / (float)e.MaxHealth;
                        healthPercentage *= 0.75f;
                        DOTween.To(() => playerHealthBar.fillAmount, x => playerHealthBar.fillAmount = x, healthPercentage,
                            0.3f);
                    });
                }
                else
                {
                    float healthPercentage = e.NewHealth / (float)e.MaxHealth;
                    healthPercentage *= 0.75f;
                    DOTween.To(() => playerHealthBar.fillAmount, x => playerHealthBar.fillAmount = x, healthPercentage, 0.3f);
                }
                Debug.Log("HealthBarFadeTimerChanged");
                healthBarFadeTimer = healthBarFadeTime;
                healthBarFadeWaiting = true;
            }
            
        }

        private void Update() {
            healthBarFadeTimer -= Time.deltaTime;
            if (healthBarFadeTimer <= 0 && healthBarFadeWaiting) {
                healthBarFadeWaiting = false;
                playerHealthBarBG.DOFade(0, 0.3f);
                playerHealthBar.DOFade(0, 0.3f);
            }
        }

        private void Start() {
            playerNameText.text = "";
            UntilAction untilAction = UntilAction.Allocate(() => NetworkClient.active);
            untilAction.OnEndedCallback += () => {
                PlayerSpaceship spaceship = GetComponentInParent<PlayerSpaceship>();
                playerNameText.text = spaceship.Name;
                if (spaceship.ThisSpaceshipTeam != NetworkClient.connection.identity
                        .GetComponent<NetworkMainGamePlayer>().ControlledSpaceship.GetComponent<PlayerSpaceship>()
                        .ThisSpaceshipTeam) {
                    playerNameText.color = Color.red;
                }
            };
            untilAction.Execute();
        }
        
    }
}
