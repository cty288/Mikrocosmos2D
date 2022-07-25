using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MikroFramework.Architecture;
using MikroFramework.Event;
using MikroFramework.TimeSystem;
using MikroFramework.Utilities;
using Mirror;
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

        public void OnEntitySwitched(bool switchTo, float switchedToWaitTime = 0f);
    }

    public interface IHaveMomentumViewController {
        public IHaveMomentum Model { get; }
    }

    public interface ICanBeShotViewController: IHookableViewController, IHaveMomentumViewController {
        public ICanBeShot Model { get; }
    }

    public interface IEntityViewController :  ICanBeShotViewController {
        IEntity Model { get; }

        void ResetViewController();
    }

    public interface ICanBeUsedHookableViewController: IHookableViewController {
        public ICanBeUsed Model { get; }
    }

    public interface IDamagableViewController : IHaveMomentumViewController { 
        public IDamagable DamagableModel { get; }
        public Vector2 DamageTextSpawnOffset { get; }

    }



    public abstract class BasicEntityViewController: AbstractNetworkedController<Mikrocosmos>, IEntityViewController {
        private Vector3 originalScale;
        public  IEntity Model { get; protected set; }

        protected Collider2D collider;
        public void ResetViewController() {
            OnReset();
        }

        public virtual void OnReset() {

        }
        

        [field: SerializeField]
        public Vector2 HookedPositionOffset { get; protected set; }

        [field: SerializeField]
        public float HookedRotationZOffset { get; protected set; }

        [ServerCallback]
        public void OnEntitySwitched(bool switchTo, float switchedToWaitTime  = 0f) {
            if (switchTo) {
                transform.DOKill(false);
                transform.localScale = Vector3.zero;
                if (switchedToWaitTime > 0) {
                    this.GetSystem<ITimeSystem>().AddDelayTask(switchedToWaitTime, () => {
                        if (this) {
                            transform.DOScale(originalScale, 0.3f);
                        }
                       
                    });
                }
                else {
                    transform.DOScale(originalScale, 0.3f);
                }
               

            }
            else {
                NetworkServer.UnSpawn(gameObject);
                gameObject.SetActive(false);
               // transform.DOScale(Vector3.zero, 0.3f).OnComplete(() =>
               // {
                    
                //});
            }
        }


        protected Rigidbody2D rigidbody;

        protected NetworkTransform networkTransform;
        protected NetworkRigidbody2D networkRigidbody;


     

        protected virtual void Awake() {
            Model = GetComponent<IEntity>();
            rigidbody = GetComponent<Rigidbody2D>();
            networkTransform = GetComponent<NetworkTransform>();
            networkRigidbody = GetComponent<NetworkRigidbody2D>();
            originalScale = transform.localScale;
            collider = GetComponent<Collider2D>();
        }

        public override void OnStartServer() {
            base.OnStartServer();
            this.RegisterEvent<OnItemShot>(OnItemShot).UnRegisterWhenGameObjectDestroyed(gameObject, true);
        }

        private void OnItemShot(OnItemShot e) {
            if (e.TargetShotItem == this as ICanBeShotViewController) {
                rigidbody.velocity = e.BindedVelocity;
                rigidbody.AddForce(e.Force, ForceMode2D.Impulse);
            }
        }


        protected Transform hookedByTr;
        protected virtual void FixedUpdate() {

            if (isServer && Model!=null) {
                if (Model.HookState == HookState.Hooked)
                {
                     hookedByTr = Model.HookedByTransform;
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
                            rigidbody.position = hookedByTr.position;
                            transform.rotation = Quaternion.Euler(hookedByTr.rotation.eulerAngles +
                                                                  new Vector3(0, 0, HookedRotationZOffset));
                        }
                    }
                }
            }

            /*
            if (isClientOnly) {
                if (Model.HookState == HookState.Hooked && Model.HookedByIdentity.hasAuthority) {
                    networkRigidbody.syncVelocity = false;
                    networkTransform.syncPosition = false;
                    networkTransform.syncRotation = false;

                    Transform hookedByTr = Model.HookedByIdentity.transform.Find("HookTransform");
                    
                    if (hookedByTr) {
                        hookedByTr.localPosition = HookedPositionOffset;
                       
                       //rigidbody.MovePosition(Vector2.Lerp(transform.position, hookedByTr.position, 20 * Time.fixedDeltaTime));
                       transform.position = hookedByTr.position;
                        transform.rotation = Quaternion.Euler(hookedByTr.rotation.eulerAngles +
                                                                  new Vector3(0, 0, HookedRotationZOffset));
                    }
                }
                else
                {
                    networkRigidbody.syncVelocity = true;
                    networkTransform.syncPosition = true;
                    networkTransform.syncRotation = true;
                }
            }*/

        

        }

        
        //TODO: 让对方启动动量计算
        //对方Unhooked：正常
        //对方Hooked && 对方MoveByPhysics：手动算速度

        //对于星球：
        //对方UnHooked：正常
        //对方Hooked ：手动算速度
        protected virtual void OnCollisionEnter2D(Collision2D collision) {
            if (Model.canDealMomentumDamage && isServer) {
                if (collision.collider && !collider.isTrigger)
                {
                    if (collision.collider.TryGetComponent<IDamagable>(out IDamagable model))
                    {
                        if (model is IHookable hookable)
                        {
                            if (hookable.HookState == HookState.Hooked)
                            {
                                if (hookable.MoveMode == MoveMode.ByTransform)
                                {
                                    return;
                                }

                                StartCoroutine(NonPhysicsForceCalculation(model, collision.collider.GetComponent<Rigidbody2D>()));
                                return;
                            }
                        }
                        //normal
                        // StartCoroutine(NonPhysicsForceCalculation(model, collision.collider.GetComponent<Rigidbody2D>()));
                        StartCoroutine(PhysicsForceCalculation(model, collision.collider.GetComponent<Rigidbody2D>()));
                    }
                }
            }
            
        }

        
        IEnumerator NonPhysicsForceCalculation(IDamagable targetModel,Rigidbody2D targetRigidbody) {
            float waitTime = 0.02f;
            Vector2 offset = Vector2.zero;
            if (targetModel is ISpaceshipConfigurationModel)
            {
                targetRigidbody.GetComponent<PlayerSpaceship>().CanControl = false;
                targetRigidbody.GetComponent<PlayerSpaceship>().RecoverCanControl(waitTime);
            }
            Vector2 speed1 = targetRigidbody.velocity;
            
            yield return new WaitForSeconds(waitTime);
            if (targetRigidbody) {
                Vector2 speed2 = targetRigidbody.velocity;

                Vector2 acceleration = (speed2 - speed1) / waitTime;
                Debug.Log($"Speed1: {speed1}, Speed 2: {speed2}, Acceleration: {acceleration}. " +
                          $"Fixed Dealta Time : {Time.fixedDeltaTime}");
                if (targetModel != null) {
                    
                    Vector2 force = acceleration * Mathf.Sqrt(targetModel.GetTotalMass());
                    if (targetModel is ISpaceshipConfigurationModel model) {
                        force *= speed2.magnitude / model.MaxSpeed;
                    }
                    force = new Vector2(Mathf.Sign(force.x) * Mathf.Log(Mathf.Abs(force.x)), Mathf.Sign(force.y) * Mathf.Log(Mathf.Abs(force.y), 2));
                    force *= 2;
                    float excessiveMomentum = targetModel.TakeRawMomentum(force.magnitude, 0);
                    targetModel.OnReceiveExcessiveMomentum(excessiveMomentum);
                    targetModel.TakeRawDamage(targetModel.GetDamageFromExcessiveMomentum(excessiveMomentum), netIdentity);
                        //    NetworkRoomManager.singleton.
                }
            }
           
           
            
        }

        protected  virtual IEnumerator PhysicsForceCalculation(IDamagable targetModel, Rigidbody2D targetRigidbody) {
            float waitTime = 0.02f;
            Vector2 offset = Vector2.zero;
            if (targetModel is ISpaceshipConfigurationModel) {
                targetRigidbody.GetComponent<PlayerSpaceship>().CanControl = false;
                targetRigidbody.GetComponent<PlayerSpaceship>().RecoverCanControl(waitTime);
            }

            Vector2 speed1 = targetRigidbody.velocity;
            yield return new WaitForSeconds(waitTime);
            if (targetRigidbody) {
                Vector2 speed2 = targetRigidbody.velocity;

                Vector2 acceleration = (speed2 - speed1) / waitTime;
                Debug.Log($"Speed1: {speed1}, Speed 2: {speed2}, Acceleration: {acceleration}. " +
                          $"Fixed Dealta Time : {Time.fixedDeltaTime}");
                if (targetModel != null) {
                    Vector2 force = acceleration * Mathf.Sqrt(targetModel.GetTotalMass());
                    if (targetModel is ISpaceshipConfigurationModel model) {
                        force *= speed2.magnitude / model.MaxSpeed;
                    }

                    force = new Vector2(Mathf.Sign(force.x) * Mathf.Log(Mathf.Abs(force.x)),
                        Mathf.Sign(force.y) * Mathf.Log(Mathf.Abs(force.y), 2));
                    force *= 2;
                    float excessiveMomentum = targetModel.TakeRawMomentum(force.magnitude, 0);
                    targetModel.OnReceiveExcessiveMomentum(excessiveMomentum);
                    targetModel.TakeRawDamage(targetModel.GetDamageFromExcessiveMomentum(excessiveMomentum), netIdentity);
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
                if (Model.Frozen) {
                    rigidbody.velocity = Vector2.zero;
                }
                else {
                    rigidbody.velocity = Vector2.ClampMagnitude(rigidbody.velocity, Model.MaxSpeed);
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

        IHaveMomentum IHaveMomentumViewController.Model {
            get {
                return Model as IHaveMomentum;
            }
        }
    }
}
