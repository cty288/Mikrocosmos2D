using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using MikroFramework.Event;
using Mirror;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Mikrocosmos
{
    public abstract class AbstractGoodsViewController : AbstractCanCreateShadeEntity, IGoodsViewController {

        private Collider2D collider;
        private ShadowCaster2D shadeCaster;
        
       
        protected override void Awake() {
            base.Awake();
            GoodsModel = GetComponent<IGoods>();
            collider = GetComponent<Collider2D>();
            shadeCaster = GetComponent<ShadowCaster2D>();
        }

        private void Start() {
            if (!GoodsModel.TransactionFinished) {
                collider.isTrigger = true;
                rigidbody.bodyType = RigidbodyType2D.Kinematic;
            }
        }

        protected override void FixedUpdate() {
            base.FixedUpdate();
            if (isServer) {
                if (!GoodsModel.TransactionFinished && FollowingPoint) {
                    transform.position = FollowingPoint.position;
                }
            }
         
        }

        protected override void Update() {
            base.Update();
            if (GoodsModel.TransactionFinished) {
                collider.isTrigger = false;
                shadeCaster.castsShadows = true;
                rigidbody.bodyType = RigidbodyType2D.Dynamic;
            }
            else {
                shadeCaster.castsShadows = false;
               
            }
        }

        public Transform FollowingPoint { get; set; }
        public IGoods GoodsModel { get; private set; }
    }
}
