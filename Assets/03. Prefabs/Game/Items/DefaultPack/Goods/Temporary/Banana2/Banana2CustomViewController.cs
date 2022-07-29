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


     

        

        GameObject bullet = Instantiate(bananaPrefab, transform.position + transform.up* 3, Quaternion.identity);
        // banana.GetComponent<ICanBeUsed>().MaxDurability = 100;
        bullet.GetComponent<BasicBulletViewController>().SetShotoer(Model.HookedByIdentity, GetComponent<Collider2D>(), null);
        bullet.GetComponent<Rigidbody2D>().AddForce(-transform.right * ShootForce, ForceMode2D.Impulse);
        bullet.transform.rotation = transform.rotation;

        NetworkServer.Spawn(bullet);

    }
}
