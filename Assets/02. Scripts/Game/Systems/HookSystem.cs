using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public interface IHookSystem : ISystem {
        IHookableViewController HookedItem { get; set; }

        bool IsHooking { get; }
    }
    public class HookSystem : AbstractNetworkedSystem, IHookSystem
    {
        [field: SerializeField]
        public IHookableViewController HookedItem { get; set; }

        [field: SyncVar]
        public bool IsHooking { get; private set; }


        private void Update() {
            if (isServer) {
                IsHooking = HookedItem != null;
            }
        }
    }
}
