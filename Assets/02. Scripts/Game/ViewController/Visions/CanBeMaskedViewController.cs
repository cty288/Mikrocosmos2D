using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public interface ICanBeMaskedViewController {
        public bool CanBeMasked { get; }

        public bool AlsoMaskedOnMap { get; }
        void ServerTurnOn();
        void ServerTurnOff();
    }
    public class CanBeMaskedViewController : AbstractNetworkedController<Mikrocosmos>, ICanBeMaskedViewController
    {
        [SerializeField] protected Material defaultSpriteLitMaterial;
        [SerializeField] protected Material visionEntityMaterial;
        [SerializeField]
        protected SpriteRenderer[] visionAffectedSprites;

        [SerializeField]
        protected SpriteRenderer[] visionAffectedSpritesOnMap;
        
        [field: SyncVar(hook = nameof(OnCanBeMaskedChanged)), SerializeField] 
        public bool CanBeMasked { get; protected set; } = true;

        [field: SerializeField]
        public bool AlsoMaskedOnMap { get; protected set; }

        public void ServerTurnOn() {
            CanBeMasked = true;
        }

        public void ServerTurnOff() {
            CanBeMasked = false;
        }



        private void OnCanBeMaskedChanged(bool oldValue, bool newValue) {
            ClientUpdateCanBeMasked();
        }


        public override void OnStartClient() {
            base.OnStartClient();
            ClientUpdateCanBeMasked();
        }


        [ClientCallback]
        protected virtual void ClientUpdateCanBeMasked() {
            Debug.Log("Client Update Can Be Masked");
            Material mat;
            if (!CanBeMasked) {
                mat = Material.Instantiate(defaultSpriteLitMaterial);
            }
            else {
                mat = Material.Instantiate(visionEntityMaterial);
            }

            foreach (SpriteRenderer sprite in visionAffectedSprites) {
                sprite.material = mat;
            }

            if (AlsoMaskedOnMap) {
                foreach (SpriteRenderer sprite in visionAffectedSpritesOnMap) {
                    sprite.material = mat;
                }
            }
        }
    }
}
