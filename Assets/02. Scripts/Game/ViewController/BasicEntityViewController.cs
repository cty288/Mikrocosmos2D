using System;
using System.Collections;
using System.Collections.Generic;
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

    public interface IEntityViewController<T> : IHookableViewController, ICanBeShotViewController where T:IEntity{
        T Model { get; }
    }


    public abstract class BasicEntityViewController<T> : AbstractNetworkedController<Mikrocosmos>, IEntityViewController<T> where T:IEntity {
        public T Model { get; protected set; }
        protected Rigidbody2D rigidbody;



        protected virtual void Awake() {
            Model = GetComponent<T>();
            rigidbody = GetComponent<Rigidbody2D>();
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
