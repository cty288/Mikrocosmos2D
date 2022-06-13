using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public struct BuffInfo {
        public string Name;
        public string LocalizedDescription;
        public string LocalizedName;
        public float TimeBuffTime;
        public float TimeBuffMaxTime;
        public float UntilBuffTriggerTime;
    }

    public enum BuffUpdateMode {
        Start,
        Stop,
        Update
    }
    public interface IPlayerBuffSystem : ISystem {

    }
    public class PlayerBuffSystem : AbstractNetworkedSystem {
        private IBuffSystem buffSystem;

        private void Awake() {
            buffSystem = GetComponent<IBuffSystem>();
        }

        public override void OnStartServer() {
            base.OnStartServer();
            buffSystem.ServerOnBuffUpdate += OnServerBuffUpdate;
            buffSystem.ServerOnBuffStart += OnServerBuffStart;
            buffSystem.ServerOnBuffStop += OnServerBuffStop;
        }

        public override void OnStopServer() {
            base.OnStopServer();
            buffSystem.ServerOnBuffUpdate -= OnServerBuffUpdate;
            buffSystem.ServerOnBuffStart -= OnServerBuffStart;
            buffSystem.ServerOnBuffStop -= OnServerBuffStop;
        }
        private void OnServerBuffStart(IBuff buff) {
            BuffInfo buffInfo = SetupBuffInfo(buff);
            TargetUpdateBuffInfo(buffInfo, BuffUpdateMode.Start);
        }
        
        private void OnServerBuffStop(IBuff buff) {
            BuffInfo buffInfo = SetupBuffInfo(buff);
            TargetUpdateBuffInfo(buffInfo, BuffUpdateMode.Stop);
        }

        private void OnServerBuffUpdate(IBuff buff) {
            BuffInfo buffInfo = SetupBuffInfo(buff);
            TargetUpdateBuffInfo(buffInfo, BuffUpdateMode.Update);            
        }


        private BuffInfo SetupBuffInfo(IBuff buff) {
            BuffInfo buffInfo = new BuffInfo()
            {
                LocalizedDescription = buff.GetLocalizedDescriptionText(),
                LocalizedName = buff.GetLocalizedName(),
                Name = buff.Name
            };
            if (buff is ITimedBuff timedBuff)
            {
                buffInfo.TimeBuffTime = timedBuff.RemainingTime;
                buffInfo.TimeBuffMaxTime = timedBuff.MaxDuration;
            }

            if (buff is IUntilBuff untilBuff)
            {
                buffInfo.UntilBuffTriggerTime = untilBuff.TotalCanBeTriggeredTime;
            }

            return buffInfo;
        }


        [TargetRpc]
        public void TargetUpdateBuffInfo(BuffInfo buffInfo, BuffUpdateMode updateStatus) {
            this.SendEvent<ClientOnBuffUpdate>(new ClientOnBuffUpdate() {
                BuffInfo = buffInfo,
                UpdateMode = updateStatus
            });
            Debug.Log($"Client Buff Update: UpdateMode - {updateStatus}, BuffName: {buffInfo.Name}");
        }
    }

    public struct ClientOnBuffUpdate {
        public BuffUpdateMode UpdateMode;
        public BuffInfo BuffInfo;
    }
}
