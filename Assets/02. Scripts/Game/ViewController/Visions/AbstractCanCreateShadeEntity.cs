using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using MikroFramework.Event;
using Mirror;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Mikrocosmos
{
    public abstract class AbstractCanCreateShadeEntity : BasicEntityViewController, ICanCreateShadeVisionControllor
    {
        [field: SyncVar(hook = nameof(OnMaskableChanged)), SerializeField]
        public bool IsMaskable { get; set; } = true;

        public override IEntity Model { get; protected set; }
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
                Debug.Log(e.HookedByIdentity == null);
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
            shaderCaster.enabled = IsMaskable;
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
            shaderCaster.enabled = IsMaskable;
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
        }

        private void OnMaskableChanged(bool oldMaskable, bool currentMaskable)
        {
            shaderCaster.enabled = currentMaskable;
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
