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
    public interface IHookable
    {
        HookState HookState { get; }

        NetworkIdentity HookedByIdentity { get; }

        Transform ClientHookedByTransform { get; }

        void Hook(NetworkIdentity hookedBy);
        void UnHook();
    }
}
