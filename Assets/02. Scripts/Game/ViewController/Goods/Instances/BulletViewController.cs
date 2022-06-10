using System;
using System.Collections;
using System.Collections.Generic;
using Mikrocosmos;
using Mirror;
using UnityEngine;

public class BulletViewController : BasicEntityViewController {
    [HideInInspector]
    public float Force;

    [HideInInspector]
    public Collider2D shooter;

    protected  void Start() {
        base.Awake();
        if (shooter) {
            Physics2D.IgnoreCollision(shooter, GetComponent<Collider2D>(), true);
        }
    }

  

    private void OnCollisionEnter2D(Collision2D collision) {
        if (isServer) {
            if (collision.collider) {
                if (collision.collider.TryGetComponent<IHaveMomentum>(out IHaveMomentum entity)) {
                    collision.collider.GetComponent<Rigidbody2D>()
                        .AddForce(GetComponent<Rigidbody2D>().velocity.normalized * Force, ForceMode2D.Impulse);

                    NetworkServer.Destroy(this.gameObject);
                }
            }
        }
    }
}
