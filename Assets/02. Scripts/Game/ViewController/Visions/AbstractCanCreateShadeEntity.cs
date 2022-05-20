using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using MikroFramework.Event;
using MikroFramework.TimeSystem;
using Mirror;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Mikrocosmos
{
    public abstract class AbstractCanCreateShadeEntity : BasicEntityViewController, ICanCreateShadeVisionControllor
    {
        [field: SyncVar(hook = nameof(OnMaskableChanged)), SerializeField]
        public bool IsMaskable { get; set; } = true;

       
        private ShadowCaster2D shaderCaster;

        
        protected override void Awake()
        {
            base.Awake();
            shaderCaster = GetComponent<ShadowCaster2D>();
            this.RegisterEvent<OnServerObjectHookStateChanged>(OnServerObjectHookStateChanged)
                .UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        

        private void OnServerObjectHookStateChanged(OnServerObjectHookStateChanged e)
        {
            if (e.Identity == netIdentity && e.HookedByIdentity != null)
            { 
                TargetHookerClientShadeControl(e.HookedByIdentity.connectionToClient, e.HookState);
            }
        }

        protected override void Update()
        {
            base.Update();
            shaderCaster.selfShadows = false;
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            if (Model.HookState != HookState.Hooked) {
                shaderCaster.enabled = IsMaskable;
            }

            this.GetSystem<ITimeSystem>().AddDelayTask(0.1f, () => {
                if (this) {
                    OnServerObjectHookStateChanged(new OnServerObjectHookStateChanged()
                    {
                        HookedByIdentity = Model.HookedByIdentity,
                        HookState = Model.HookState,
                        Identity = netIdentity
                    });
                }
               
            });

        }
        private IMeteorModel GetModel()
        {
            return GetModel<IMeteorModel>();
        }





        [ServerCallback]
        public void ServerSetClientAlwaysUnMaskable(NetworkConnectionToClient connection)
        {

        }
        [ServerCallback]
        public void ServerTurnOffClientAlwaysUnMaskable(NetworkConnectionToClient connection)
        {

        }



        protected bool clientAlwaysUnmaskable = false;
        public override void OnStartClient()
        {
            base.OnStartClient();
            if (Model.HookState != HookState.Hooked) {
                shaderCaster.enabled = IsMaskable;
            }
         
        }

        [TargetRpc]
        private void TargetHookerClientShadeControl(NetworkConnection conn, HookState hookState)
        {
            if (hookState == HookState.Hooked)
            {
                shaderCaster.enabled = false;
            }
            else
            {
                shaderCaster.enabled = true;
            }

            Debug.Log($"Target Hook State: {shaderCaster.enabled}");
        }

        private void OnMaskableChanged(bool oldMaskable, bool currentMaskable)
        {
            if (Model.HookState != HookState.Hooked)
            {
                shaderCaster.enabled = IsMaskable;
            }
            if (currentMaskable)
            {
                if (!clientAlwaysUnmaskable)
                {
                    OnClientMaskable();
                }
            }
            else
            {
                if (!clientAlwaysUnmaskable)
                {
                    OnClientUnMaskable();
                }
            }
        }



        protected virtual void OnClientMaskable() { }

        protected virtual void OnClientUnMaskable() { }
    }
}
