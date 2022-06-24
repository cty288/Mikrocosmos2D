using System.Collections;
using System.Collections.Generic;
using Mikrocosmos;
using MikroFramework.Architecture;
using Mirror;
using UnityEngine;



public interface IBulletModel : IModel {
    public int Damage { get;  set; }
}
public class BulletModel : AbstractBasicEntityModel, IBulletModel {
    [field: SyncVar, SerializeField]
    public override float SelfMass { get;  set; }
    
    [field: SyncVar, SerializeField]
    public int Damage { get;  set; }

    [field: SyncVar, SerializeField] public override string Name { get; set; } = "Bullet";

    [ServerCallback]
    protected override bool ServerCheckCanHook(NetworkIdentity hookedBy) {
        return false;
    }
    protected override void OnEnable()
    {

    }
    public override void OnClientHooked() {
        
    }

    public override void OnClientFreed() {
       
    }
}
