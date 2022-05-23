using System.Collections;
using System.Collections.Generic;
using Mikrocosmos;
using Mirror;
using UnityEngine;

public class Banana2CustomViewController : BasicGoodsViewController {
    [SerializeField] private GameObject bananaPrefab;
    [SerializeField] private float ShootForce;

    protected override void OnServerItemUsed() {
        base.OnServerItemUsed();
        GameObject banana = Instantiate(bananaPrefab, transform.position + transform.up* 3, Quaternion.identity);
        // banana.GetComponent<ICanBeUsed>().MaxDurability = 100;
        banana.GetComponent<BulletViewController>().shooter = GetComponent<Collider2D>();
        banana.GetComponent<Rigidbody2D>()
            .AddForce(Model.HookedByIdentity.transform.up.normalized * 50,
                ForceMode2D.Impulse);
        banana.GetComponent<BulletViewController>().Force = ShootForce;
        
        NetworkServer.Spawn(banana);

    }
}
