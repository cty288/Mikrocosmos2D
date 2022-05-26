using System.Collections;
using System.Collections.Generic;
using Mikrocosmos;
using Mirror;
using UnityEngine;

public class JellyGunViewController : BasicGoodsViewController {
    [SerializeField] private GameObject bullet;
    [SerializeField] private float shootForce;

    private Transform shootPos;
    private NetworkAnimator animator;

    protected override void Awake() {
        base.Awake();
        shootPos = transform.Find("ShootPosition");
        animator = GetComponent<NetworkAnimator>();
    }

    protected override void OnServerItemUsed() {
        base.OnServerItemUsed();
        if (animator.animator.GetCurrentAnimatorStateInfo(0).IsName("Idle")) {
            animator.SetTrigger("Shoot");
        }
    }

    
    public void OnBulletShoot() {
        if (isServer) {
            GameObject bullet = Instantiate(this.bullet, shootPos.transform.position, Quaternion.identity);
            bullet.GetComponent<BasicBulletViewController>().shooter = GetComponent<Collider2D>();
            bullet.GetComponent<Rigidbody2D>().AddForce(-transform.right * shootForce, ForceMode2D.Impulse);
            bullet.transform.rotation = transform.rotation;
            NetworkServer.Spawn(bullet);
        }
    }

    public void OnShootAnimationEnds() {
        if (isServer) {
            GoodsModel.ReduceDurability(1);
        }
    }
}
