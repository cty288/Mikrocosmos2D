using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using MikroFramework.Event;
using Mirror;
using UnityEngine;

public abstract class NetworkedArchitecture<T>: Architecture<T>, IArchitecture where T:Architecture<T>,new() {
    protected override void Init() {
        if (NetworkServer.active) {
#if UNITY_SERVER || UNITY_EDITOR
            SeverInit();
#endif
        }else if (NetworkClient.active) {
            ClientInit();
        }
    }

#if UNITY_SERVER || UNITY_EDITOR
    protected abstract void SeverInit();
#endif

    protected abstract void ClientInit();
}
