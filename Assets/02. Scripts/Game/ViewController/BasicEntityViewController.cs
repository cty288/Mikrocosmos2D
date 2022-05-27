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

        //TODO: 让对方启动动量计算
        //对方Unhooked：正常
        //对方Hooked && 对方MoveByPhysics：手动算速度

        //对于星球：
        //对方UnHooked：正常
        //对方Hooked ：手动算速度
        private void OnCollisionEnter2D(Collision2D collision) {
            if (collision.collider) {
                if (collision.collider.TryGetComponent<IDamagable>(out IDamagable model)) {
                    if (model is IHookable hookable) {
                        if (hookable.HookState == HookState.Hooked) {
                            if (hookable.MoveMode == MoveMode.ByTransform) {
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

        IEnumerator NonPhysicsForceCalculation(IDamagable targetModel,Rigidbody2D targetRigidbody) {
            float waitTime = 0.01f;
            if (targetModel is ISpaceshipConfigurationModel)
            {
                targetRigidbody.GetComponent<PlayerSpaceship>().CanControl = false;
            }
            Vector2 pos1 = targetRigidbody.transform.position;
            yield return new WaitForSeconds(waitTime);
            Vector2 pos2 = targetRigidbody.transform.position;
            Vector2 speed1 = (pos2 - pos1) / waitTime;
            yield return new WaitForSeconds(waitTime);

            Vector2 pos3 = targetRigidbody.transform.position;
            Vector2 speed2 = (pos3 - pos2) / waitTime;

            Vector2 acceleration = (speed2 - speed1) / waitTime;
            if (targetModel != null)
            {
                Vector2 force = acceleration * Mathf.Sqrt(targetModel.GetTotalMass());
                if (targetModel is ISpaceshipConfigurationModel model)
                {
                    targetRigidbody.GetComponent<PlayerSpaceship>().CanControl = true;
                    force *= speed2.magnitude / model.MaxSpeed;
                }
                force = new Vector2(Mathf.Sign(force.x) * Mathf.Log(Mathf.Abs(force.x)), Mathf.Sign(force.y) * Mathf.Log(Mathf.Abs(force.y), 2));
                force *= 2;
                float excessiveMomentum = targetModel.TakeRawMomentum(force.magnitude, 0);
                targetModel.OnReceiveExcessiveMomentum(excessiveMomentum);
                targetModel.TakeRawDamage(targetModel.GetDamageFromExcessiveMomentum(excessiveMomentum));
            }
        }

        IEnumerator PhysicsForceCalculation(IDamagable targetModel, Rigidbody2D targetRigidbody) {
            float waitTime = 0.01f;
            Vector2 offset = Vector2.zero;
            if (targetModel is ISpaceshipConfigurationModel)
            {
                targetRigidbody.GetComponent<PlayerSpaceship>().CanControl = false;
            }
            Vector2 speed1 = targetRigidbody.velocity;
            yield return new WaitForSeconds(waitTime);
            Vector2 speed2 = targetRigidbody.velocity;

            Vector2 acceleration = (speed2 - speed1) / waitTime;
            Debug.Log($"Speed1: {speed1}, Speed 2: {speed2}, Acceleration: {acceleration}. " +
                      $"Fixed Dealta Time : {Time.fixedDeltaTime}");
            if (targetModel != null)
            {
                Vector2 force = acceleration * Mathf.Sqrt(targetModel.GetTotalMass());
                if (targetModel is ISpaceshipConfigurationModel model)
                {
                    targetRigidbody.GetComponent<PlayerSpaceship>().CanControl = true;
                    force *= speed2.magnitude / model.MaxSpeed;
                }
                force = new Vector2(Mathf.Sign(force.x) * Mathf.Log(Mathf.Abs(force.x)), Mathf.Sign(force.y) * Mathf.Log(Mathf.Abs(force.y), 2));
                force *= 2;
                float excessiveMomentum = targetModel.TakeRawMomentum(force.magnitude, 0);
                targetModel.OnReceiveExcessiveMomentum(excessiveMomentum);
                targetModel.TakeRawDamage(targetModel.GetDamageFromExcessiveMomentum(excessiveMomentum));

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
