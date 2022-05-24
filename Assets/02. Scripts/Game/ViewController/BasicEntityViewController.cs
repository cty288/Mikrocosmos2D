using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using MikroFramework.Event;
using MikroFramework.Utilities;
using Mirror.Experimental;
using UnityEngine;
using NetworkTransform = Mirror.NetworkTransform;

namespace Mikrocosmos
{
  

    /// <summary>
    /// Can't be added to inventory
    /// </summary>
    public interface IHookableViewController: IHaveMomentumViewController
    {
        public IHookable Model { get; }

        public Vector2 HookedPositionOffset { get; }
        public float HookedRotationZOffset { get; }
    }

    public interface IHaveMomentumViewController {
        public IHaveMomentum Model { get; }
    }

    public interface ICanBeShotViewController: IHookableViewController, IHaveMomentumViewController {
        public ICanBeShot Model { get; }
    }

    public interface IEntityViewController :  ICanBeShotViewController {
        IEntity Model { get; }
    }

    public interface ICanBeUsedHookableViewController: IHookableViewController {
        public ICanBeUsed Model { get; }
    }

    public interface IDamagableViewController : IHaveMomentumViewController { 
        public IDamagable DamagableModel { get; }

    }



    public abstract class BasicEntityViewController: AbstractNetworkedController<Mikrocosmos>, IEntityViewController {
        public  IEntity Model { get; protected set; }

        [field: SerializeField]
        public Vector2 HookedPositionOffset { get; protected set; }

        [field: SerializeField]
        public float HookedRotationZOffset { get; protected set; }


        protected Rigidbody2D rigidbody;

        protected Transform hookedTrReference;


        protected virtual void Awake() {
            Model = GetComponent<IEntity>();
            rigidbody = GetComponent<Rigidbody2D>();
          
        }

        public override void OnStartServer() {
            base.OnStartServer();
            this.RegisterEvent<OnItemShot>(OnItemShot).UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        private void OnItemShot(OnItemShot e) {
            if (e.TargetShotItem == this as ICanBeShotViewController) {
                rigidbody.velocity = Model.HookedByIdentity.GetComponent<Rigidbody2D>().velocity;
                rigidbody.AddForce(e.Force, ForceMode2D.Impulse);
            }
        }

     
      
        
        protected virtual void FixedUpdate() {

            if (isServer)
            {
                if (Model.HookState == HookState.Hooked)
                {
                    Transform hookedByTr = Model.HookedByTransform;
                    if (hookedByTr) {


                        hookedByTr.localPosition = HookedPositionOffset;
                        // rigidbody.bodyType = RigidbodyType2D.Kinematic;
                        if (Model.MoveMode == MoveMode.ByPhysics)
                        {
                            rigidbody.MovePosition(Vector2.Lerp(transform.position, hookedByTr.position , 20 * Time.fixedDeltaTime));

                            transform.rotation = Quaternion.Euler(hookedByTr.rotation.eulerAngles +
                                                                  new Vector3(0, 0, HookedRotationZOffset));
                        }
                        else {
                            transform.position = hookedByTr.position;
                            transform.rotation = Quaternion.Euler(hookedByTr.rotation.eulerAngles +
                                                                  new Vector3(0, 0, HookedRotationZOffset));
                        }


                    }
                }
                else
                {
                    rigidbody.bodyType = RigidbodyType2D.Dynamic;
                }

            }

        

        }
        private void OnCollisionEnter2D(Collision2D collision) {
            if (collision.collider) {
                if (collision.collider.TryGetComponent<IDamagable>(out IDamagable model)) {
                    float excessiveMomentum =  model.TakeRawMomentum(this.gameObject,0);
                    model.OnReceiveExcessiveMomentum(excessiveMomentum);
                    model.TakeRawDamage(model.GetDamageFromExcessiveMomentum(excessiveMomentum));
                }
            }
        }

        protected virtual void Update() {
            if (isServer) {
                OnServerUpdate();
            }
        }

        protected virtual void OnServerUpdate() {
            if (Model.HookState == HookState.Freed) {
                rigidbody.velocity = Vector2.ClampMagnitude(rigidbody.velocity, Model.MaxSpeed);
            }
        }

        IHookable IHookableViewController.Model
        {
            get
            {
                return Model as IHookable;
            }
        }

        ICanBeShot ICanBeShotViewController.Model
        {
            get
            {
                return Model as ICanBeShot;
            }
        }

        IHaveMomentum IHaveMomentumViewController.Model {
            get {
                return Model as IHaveMomentum;
            }
        }
    }
}
