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
        HookState HookState { get; }

        NetworkIdentity HookedByIdentity { get; }

        Transform ClientHookedByTransform { get; }

        bool Hook(NetworkIdentity hookedBy);

        
        void UnHook(bool isShoot);
    }

    
}
