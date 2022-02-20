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

            SeverInit();

        }else if (NetworkClient.active) {
            ClientInit();
        }
    }


    protected abstract void SeverInit();


    protected abstract void ClientInit();
}
