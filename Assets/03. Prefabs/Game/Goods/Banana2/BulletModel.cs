using System.Collections;
using System.Collections.Generic;
using Mikrocosmos;
using Mirror;
using UnityEngine;

public class BulletModel : AbstractBasicEntityModel {
    [field: SyncVar, SerializeField]
    public override float SelfMass { get; protected set; }

    [field: SyncVar, SerializeField] public override string Name { get; set; } = "Bullet";

    [ServerCallback]
    protected override bool ServerCheckCanHook(NetworkIdentity hookedBy) {
        return false;
    }

    public override void OnClientHooked() {
        
    }

    public override void OnClientFreed() {
       
    }
}
