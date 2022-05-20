using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public enum HookState
    {
        Freed,
        Hooked
    }

   
    public interface IHookable: IHaveMomentum
    {
        public string Name { get; set; }
        public bool CanBeAddedToInventory { get; set; }
        HookState HookState { get; }

        NetworkIdentity HookedByIdentity { get; }

        Transform HookedByTransform { get; }

        bool Hook(NetworkIdentity hookedBy);

        
        void UnHook(bool isShoot);
    }

    
}
