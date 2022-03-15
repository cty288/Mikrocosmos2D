using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using MikroFramework.Event;
using MikroFramework.Utilities;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{

    public interface IHookableViewController
    {
        public IHookable Model { get; }

    }

    public interface ICanBeShotViewController {
        public ICanBeShot Model { get; }

       
    }

    public interface IEntityViewController : IHookableViewController, ICanBeShotViewController {
        IEntity Model { get; }
    }


    public abstract class BasicEntityViewController: AbstractNetworkedController<Mikrocosmos>, IEntityViewController {
        public abstract IEntity Model { get; protected set; }
      


        protected Rigidbody2D rigidbody;



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
                Debug.Log(netIdentity.connectionToClient.identity.gameObject.name);
                TargetOnShot(netIdentity.connectionToClient, e.Force);
            }
        }

        [TargetRpc]
        protected virtual void TargetOnShot(NetworkConnection conn, Vector2 force) {
            Debug.Log($"Target On shot {force}");
            rigidbody.AddForce(force, ForceMode2D.Impulse);
        }
        
      
        
        protected virtual void FixedUpdate() {
            
            if (isClient) {
                if (Model.HookState == HookState.Hooked) {
                    Transform hookedByTr = Model.ClientHookedByTransform;
                    if (hookedByTr) {
                        if (!hasAuthority) {
                            GetComponent<NetworkTransform>().syncPosition = false;
                        }
                        rigidbody.MovePosition(Vector2.Lerp(transform.position, hookedByTr.position, 0.5f));
                        transform.rotation = hookedByTr.rotation;
                        // rigidbody.velocity = hookedByTr.parent.GetComponent<Rigidbody2D>().velocity;
                    }
                   
                }
                else {
                    //if (!hasAuthority) {
                        GetComponent<NetworkTransform>().syncPosition = true;
                    //}
                }
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
    }
}
