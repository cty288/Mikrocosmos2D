using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using MikroFramework.Event;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Mikrocosmos
{

    public interface IMapPointerViewController : IController {
        public string Name { get; set; }
        public GameObject BindedGameObject { get; set; }

        public void SetPointerActive(bool active);

        public Sprite PointerSprite { get; }
    }
    public class MapPointerViewController : AbstractMikroController<Mikrocosmos>, IMapPointerViewController
    {
        private IPlanetTradingSystem targetPlanet;
        private Transform targetPlanetTransform;
        private Image progressImage;
        private TMP_Text affinityText;
        private TMP_Text distanceText;
        private Sprite pointerSprite;
        private Transform pointer;

        public Sprite PointerSprite => pointerSprite;
        [SerializeField]
        private int team;

        [SerializeField] private Sprite[] teamSprites;

        private Transform controlledSpaceship;
        
        public float Time;

        public float timer;
        
        private void Awake() {
            pointer = transform.Find("Pointer");
            progressImage = pointer.Find("Pointer_Progress").GetComponent<Image>();
            affinityText = pointer.Find("AffinityText").GetComponent<TMP_Text>();
            distanceText = pointer.Find("DistanceText").GetComponent<TMP_Text>();
            // pointerBG = transform.Find("Pointer/PointerBG").GetComponent<Image>();
            this.RegisterEvent<OnClientPlanetAffinityWithTeam1Changed>(OnAffinityWithTeam1Changed)
                .UnRegisterWhenGameObjectDestroyed(gameObject);
            BindedGameObject = gameObject;
        }

        private void OnEnable() {
          
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


        public void SetPointerActive(bool active) {
            pointer.gameObject.SetActive(active);
        }
        private void Update() {
            if (targetPlanet != null) {
                timer -= UnityEngine.Time.deltaTime;

                progressImage.fillAmount = timer / Time;

                if (distanceText && controlledSpaceship && targetPlanetTransform) {
                    distanceText.text = Mathf.RoundToInt(Vector2
                        .Distance(controlledSpaceship.transform.position, targetPlanetTransform.position)) + " ly";
                }
                
            }
        }
        private void OnAffinityWithTeam1Changed(OnClientPlanetAffinityWithTeam1Changed e)
        {
            if (e.PlanetIdentity.GetComponent<IPlanetTradingSystem>() == targetPlanet) {
                if (e.NewAffinity >= 0.5)
                {
                    pointerSprite = teamSprites[0];
                }
                else {
                    pointerSprite = teamSprites[1];
                }

                if (team == 1) {
                    affinityText.text = Mathf.RoundToInt((e.NewAffinity * 100)).ToString();
                }
                else {
                    affinityText.text = Mathf.RoundToInt(((1-e.NewAffinity) * 100)).ToString();
                }
                
            }
        }
        
        private void UpdateAffinitySpriteText() {
            float affinity = targetPlanet.GetAffinityWithTeam(1);
            if (affinity >= 0.5)
            {
                pointerSprite = teamSprites[0];
            }
            else
            {
                pointerSprite = teamSprites[1];
            }
            if (team == 1)
            {
                affinityText.text = Mathf.RoundToInt((affinity * 100)).ToString();
            }
            else
            {
                affinityText.text = Mathf.RoundToInt(((1 - affinity) * 100)).ToString();
            }
        }

        public string Name { get; set; }
        public GameObject BindedGameObject { get; set; }
    }
}
