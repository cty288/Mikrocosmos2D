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
    public class SpaceshipModel : AbstractBasicEntityModel, ISpaceshipConfigurationModel {
        public override string Name { get; } = "Spaceship";
        private IHookSystem hookSystem;
        

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


        public override float SelfMass { get; } = 1;
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
                return hookingModel.GetTotalMass();
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
                if (hookSystem.IsHooking)
                {//return SelfMass + backpack + ...
                    IHaveMomentum hookingModel =  hookSystem.HookedNetworkIdentity.GetComponent<IHaveMomentumViewController>().Model;
                   if (hookingModel is ISpaceshipConfigurationModel) {
                       return GetConnectedObjectSoleMass() + SelfMass + BackpackMass;
                   }else {
                       return SelfMass + BackpackMass + hookingModel.GetTotalMass();
                   }
                }
            }
            return SelfMass;
        }

        #endregion




        #region Server
      

       
        #endregion


       
    }
}
