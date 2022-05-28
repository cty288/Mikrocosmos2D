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
        [SerializeField]
        private int team;

        private Transform controlledSpaceship;
        private void Awake()
        {
            progressImage = transform.Find("Pointer/Pointer_Progress").GetComponent<Image>();
            affinityText = transform.Find("Pointer/AffinityText").GetComponent<TMP_Text>();
            distanceText = transform.Find("Pointer/DistanceText").GetComponent<TMP_Text>();
           
        }

        private void Start() {
            targetPlanet = GetComponent<Window_Pointer>().target.GetComponent<IPlanetTradingSystem>();
            targetPlanetTransform = GetComponent<Window_Pointer>().target.transform;
            if (NetworkClient.active) {
                team = NetworkClient.connection.identity.GetComponent<NetworkMainGamePlayer>().matchInfo.Team;
                controlledSpaceship = NetworkClient.connection.identity.GetComponent<NetworkMainGamePlayer>()
                    .ControlledSpaceship.transform;
            }
        }

        private void Update() {
            if (targetPlanet != null) {
                progressImage.fillAmount = (targetPlanet.BuyItemTimer) / (float) targetPlanet.BuyItemMaxTimeThisTime;
                    
                
                affinityText.text = Mathf.RoundToInt((targetPlanet.GetAffinityWithTeam(team)*100)).ToString();
                distanceText.text =Mathf.RoundToInt( Vector2
                    .Distance(controlledSpaceship.transform.position, targetPlanetTransform.position)) + " ly";
            }
        }
    }
}
