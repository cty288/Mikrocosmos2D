using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Mikrocosmos
{
    public class MapPointerViewController : AbstractMikroController<Mikrocosmos>
    {
        private IPlanetTradingSystem targetPlanet;
        private Transform targetPlanetTransform;
        private Image progressImage;
        private TMP_Text affinityText;
        private TMP_Text distanceText;
        private Sprite pointerSprite;

        public Sprite PointerSprite => pointerSprite;
        [SerializeField]
        private int team;

        [SerializeField] private Sprite[] teamSprites;

        private Transform controlledSpaceship;
        private void Awake()
        {
            progressImage = transform.Find("Pointer/Pointer_Progress").GetComponent<Image>();
            affinityText = transform.Find("Pointer/AffinityText").GetComponent<TMP_Text>();
            distanceText = transform.Find("Pointer/DistanceText").GetComponent<TMP_Text>();
            // pointerBG = transform.Find("Pointer/PointerBG").GetComponent<Image>();
        }

        private void Start() {
            targetPlanet = GetComponent<Window_Pointer>().target.GetComponent<IPlanetTradingSystem>();
            targetPlanetTransform = GetComponent<Window_Pointer>().target.transform;
            if (NetworkClient.active) {
                team = NetworkClient.connection.identity.GetComponent<NetworkMainGamePlayer>().matchInfo.Team;
                controlledSpaceship = NetworkClient.connection.identity.GetComponent<NetworkMainGamePlayer>()
                    .ControlledSpaceship.transform;
                UpdateAffinitySpriteText();
            }
        }

        private void Update() {
            if (targetPlanet != null) {
                progressImage.fillAmount = (targetPlanet.BuyItemTimer) / (float) targetPlanet.BuyItemMaxTimeThisTime;

                UpdateAffinitySpriteText();
                distanceText.text =Mathf.RoundToInt( Vector2
                    .Distance(controlledSpaceship.transform.position, targetPlanetTransform.position)) + " ly";
            }
        }

        private void UpdateAffinitySpriteText() {
            float affinity = targetPlanet.GetAffinityWithTeam(team);
            if (affinity >= 0.5)
            {
                pointerSprite = teamSprites[team - 1];
            }
            else
            {
                pointerSprite = team == 1 ? teamSprites[0] : teamSprites[1];
            }
            affinityText.text = Mathf.RoundToInt((affinity * 100)).ToString();
        }
    }
}
