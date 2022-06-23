using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using Mirror;
using TMPro;
using UnityEngine;

namespace Mikrocosmos
{
    public class MissionPointerViewController : AbstractMikroController<Mikrocosmos>, IMapPointerViewController {
        private Transform pointer;
        public string Name { get; set; }
        public GameObject BindedGameObject { get; set; }
        private Transform controlledSpaceship;
        private Transform target;

        private TMP_Text distanceText;
        private void Awake()
        {
            pointer = transform.Find("Pointer");

            BindedGameObject = gameObject;

            distanceText = pointer.Find("DistanceText").GetComponent<TMP_Text>();
        }

        private void Start() {
            target = GetComponent<Window_Pointer>().target;
            controlledSpaceship = NetworkClient.connection.identity.GetComponent<NetworkMainGamePlayer>()
                .ControlledSpaceship.transform;
        }

        private void Update() {
            if (target != null)
            {
                distanceText.text = Mathf.RoundToInt(Vector2
                    .Distance(controlledSpaceship.transform.position, target.position)) + " ly";
            }
        }

        public void SetPointerActive(bool active) {
            pointer.gameObject.SetActive(active);
        }

        [field:SerializeField]
        public Sprite PointerSprite { get; protected set; }
    }
}
