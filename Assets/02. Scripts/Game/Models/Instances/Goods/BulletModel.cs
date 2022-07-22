using System.Collections;
using System.Collections.Generic;
using Mikrocosmos;
using MikroFramework.Architecture;
using MikroFramework.Utilities;
using Mirror;
using UnityEngine;



public interface IBulletModel : IModel, ICanDealDamage {
    public int Damage { get;  set; }
    public bool DamageReducedBySpeed { get; set; }
    
}
public class BulletModel : AbstractBasicEntityModel, IBulletModel {
    [field: SyncVar, SerializeField]
    public override float SelfMass { get;  set; }
    
    [field: SyncVar, SerializeField]
    public int Damage { get;  set; }

    [field: SerializeField] public bool DamageReducedBySpeed { get; set; } = true;

    private IGoodsConfigurationModel configurationModel;
    [field: SyncVar, SerializeField] public override string Name { get; set; } = "Bullet";

    [ServerCallback]
    protected override bool ServerCheckCanHook(NetworkIdentity hookedBy) {
        return false;
    }

    protected override void Awake() {
        base.Awake();
        configurationModel = this.GetModel<IGoodsConfigurationModel>();
        gameObject.name = CommonUtility.DeleteCloneName(gameObject);
        int index = gameObject.name.IndexOf('(');
        if (index != -1)
        {
            gameObject.name = gameObject.name.Substring(0, index);
        }
        
        if (NetworkServer.active) {
            if (configurationModel.FindGoodsPropertiesByPrefabName(name) != null) {
                Name = configurationModel.FindGoodsPropertiesByPrefabName(name).Name;
                Damage = configurationModel.FindGoodsPropertiesByPrefabName(name).Damage;
                SelfMass = configurationModel.FindGoodsPropertiesByPrefabName(name).SelfMass;
            }
           
        }
    }

    protected override void OnEnable()
    {

    }
    public override void OnClientHooked() {
        
    }

    public override void OnClientFreed() {
       
    }
}
