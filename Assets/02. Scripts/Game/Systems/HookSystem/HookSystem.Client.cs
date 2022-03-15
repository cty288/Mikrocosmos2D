using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using UnityEngine;

namespace Mikrocosmos
{
    public struct OnClientChargePercentChanged {
        public bool IsLocalPlayer;
        public float ChargePercent;
        public float OldChargePercent;
    }
    public partial class HookSystem : AbstractNetworkedSystem, IHookSystem {
        private void OnHookChargePercentChanged(float oldValue, float newValue) {
            this.SendEvent<OnClientChargePercentChanged>(new OnClientChargePercentChanged() {
                ChargePercent = newValue,
                OldChargePercent = oldValue,
                IsLocalPlayer = hasAuthority
            });
        }
    }
}
