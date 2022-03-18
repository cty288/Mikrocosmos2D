using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public struct OnMassChanged
    {
        public float newMass;
    }

    public struct OnEscapeCounterChanged {
        public int newValue;
    }
    public class SpaceshipModel : AbstractBasicEntityModel, ISpaceshipConfigurationModel {
        public override string Name { get; } = "Spaceship";
        private IHookSystem hookSystem;
        #region Server

        [Command]
        private void CmdUnHook() {
            UnHook();
        }
      
        #endregion

        #region Client

        protected override void Awake() {
            base.Awake();
            hookSystem = GetComponent<IHookSystem>();
        }

        public override void OnHooked()
        {

        }

        public override void OnFreed()
        {

        }
        public int EscapeNeedCount { get; } = 10;
        public float EscapeLossTime { get; } = 0.125f;

        public int EscapeCounter { get; private set; }

        private float escapeLossTimer = 0f;

        public override void OnStartServer() {
            base.OnStartServer();
          
        }


        protected override void Update()
        {
            base.Update();
            if (isServer) {
                Acceleration = Mathf.Max(5, InitialAcceleration - GetTotalMass() * AccelerationDecreasePerMass);
            }
            if (hasAuthority)
            {
                escapeLossTimer += Time.deltaTime;
                if (escapeLossTimer >= EscapeLossTime)
                {
                    escapeLossTimer = 0;
                    if (EscapeCounter > 0)
                    {
                        EscapeCounter--;
                        this.SendEvent<OnEscapeCounterChanged>(new OnEscapeCounterChanged() { newValue = EscapeCounter });
                    }

                }
            }

        }

        public void IncreaseEscapeCounter()
        {
            EscapeCounter++;
            escapeLossTimer = 0;
            if (EscapeCounter >= EscapeNeedCount)
            {
                EscapeCounter = 0;
                CmdUnHook();
                
            }
            this.SendEvent<OnEscapeCounterChanged>(new OnEscapeCounterChanged() { newValue = EscapeCounter });
        }

        [field: SerializeField]
        public float InitialAcceleration { get; private set; } = 20;

        [field: SyncVar, SerializeField]
        public override float SelfMass { get; protected set; } = 1;
        [field: SerializeField]
        public float AccelerationDecreasePerMass { get; private set; } = 2;

        public float BackpackMass { get; } = 0;
    
        public float GetConnectedObjectSoleMass() {
            if (hookSystem.HookedNetworkIdentity == null) {
                return 0;
            }
            IHaveMomentum hookingModel = hookSystem.HookedNetworkIdentity.GetComponent<IHaveMomentumViewController>().Model;

            if (hookingModel is ISpaceshipConfigurationModel) {
                ISpaceshipConfigurationModel spaceshipModel = (hookingModel as ISpaceshipConfigurationModel);
                return spaceshipModel.SelfMass + spaceshipModel.BackpackMass +
                       spaceshipModel.GetConnectedObjectSoleMass();
            }
            else {
                return hookingModel.SelfMass;
            }
        }

        

        public override float GetTotalMass() {
            /*
             * hooked by somebody -> hookedBy.GetTotalMass() -> hookedBy.TotalMass()...
             * hooking somebody && !hooked by somebody -> (getRigidbodyMass + backpack) of all hooked player; add together
             * !hooking somebody && !hooked by somebody -> (getRigidbodyMass+backpack_)
             */

            //HookState
            if (HookState == HookState.Hooked) {  //hooked by somebody -> hooked.GetTotalMass() -> hooked.TotalMass()...
                //if hooked by somebody, that hooker must be another spaceship
                return (HookedByIdentity.GetComponent<IHaveMomentumViewController>()).Model.GetTotalMass();
            }
            else {
                if (hookSystem.IsHooking && hookSystem.HookedNetworkIdentity)
                {//return SelfMass + backpack + ...
                    IHaveMomentum hookingModel =  hookSystem.HookedNetworkIdentity.GetComponent<IHaveMomentumViewController>().Model;
                    
                   if (hookingModel is ISpaceshipConfigurationModel) {
                       return GetConnectedObjectSoleMass() + SelfMass + BackpackMass;
                   }else {
                       return SelfMass + BackpackMass + hookingModel.SelfMass;
                   }
                }
            }
            return SelfMass + BackpackMass;
        }


      
        #endregion




      



    }
}
