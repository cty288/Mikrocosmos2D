using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using MikroFramework.BindableProperty;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public struct OnClientChargePercentChanged {
        public bool IsLocalPlayer;
        public float ChargePercent;
        public float OldChargePercent;
    }

    public struct OnClientHookAnotherSpaceship {
        public NetworkIdentity spaceship;
    }

    public struct OnClientHookIdentityChanged {
        public string NewHookItemName;
    }
    public partial class HookSystem : AbstractNetworkedSystem, IHookSystem {
        private void OnHookChargePercentChanged(float oldValue, float newValue) {
            this.SendEvent<OnClientChargePercentChanged>(new OnClientChargePercentChanged() {
                ChargePercent = newValue,
                OldChargePercent = oldValue,
                IsLocalPlayer = hasAuthority
            });
        }

        [TargetRpc]
        private void TargetOnHookIdentityChanged(string newHookName)
        {
            if (hasAuthority) {
                this.SendEvent<OnClientHookIdentityChanged>(new OnClientHookIdentityChanged()
                {
                    NewHookItemName = newHookName
                });
                ClientHookedItemName.Value = newHookName;
            }
           
        }

        [TargetRpc]        
        private void TargetOnHookSpaceship(NetworkIdentity NewIdentity) {
            this.SendEvent<OnClientHookAnotherSpaceship>(new OnClientHookAnotherSpaceship() {
                spaceship = NewIdentity
            });
        }

      
    }
}
