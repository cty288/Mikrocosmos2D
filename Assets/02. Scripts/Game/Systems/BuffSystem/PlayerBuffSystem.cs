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

            buffSystem.ServerRegisterClientCallback<VisionExpansionBuff, OnVisionExpansion>(TargetOnVisionExpand);
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


        [TargetRpc]
        private void TargetOnVisionExpand(BuffStatus e, OnVisionExpansion message) {
            Debug.Log("Vision Expansion");
            OnVisionExpansion buff = message;
            if (e == BuffStatus.OnStart || e == BuffStatus.OnUpdate ) {
                this.SendEvent<OnVisionRangeChange>(new OnVisionRangeChange() {
                    InnerAddition = buff.VisionRangeChangeEvent.InnerAddition,
                    OuterAddition = buff.VisionRangeChangeEvent.OuterAddition
                });
                
                this.SendEvent<OnCameraViewChange>(new OnCameraViewChange() {
                    RadiusAddition = buff.CameraViewChangeEvent.RadiusAddition
                });
            }

            if (e == BuffStatus.OnEnd) {
                this.SendEvent<OnVisionRangeChange>(new OnVisionRangeChange()
                {
                    InnerAddition =- buff.VisionRangeChangeEvent.InnerAddition,
                    OuterAddition =- buff.VisionRangeChangeEvent.OuterAddition
                });

                this.SendEvent<OnCameraViewChange>(new OnCameraViewChange()
                {
                    RadiusAddition =- buff.CameraViewChangeEvent.RadiusAddition
                });
            }
            
            
            Debug.Log($"Vision Expansion: {buff.VisionRangeChangeEvent.InnerAddition}");
        }
    }

    public struct ClientOnBuffUpdate {
        public BuffUpdateMode UpdateMode;
        public BuffInfo BuffInfo;
    }
}
