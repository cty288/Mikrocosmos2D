using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{

    public struct TimeBuffInfo {
        public float TimeBuffTime;
        public float TimeBuffMaxTime;
    }

    public struct UntilBuffInfo {
        public float UntilBuffTriggerTime;
    }

    public struct PermanentRawMaterialBuffInfo {
        public int MaxLevel;

        public int CurrentProgressInLevel;

        public int MaxProgressForCurrentLevel;

        public int CurrentLevel;
    }
    public struct BuffInfo {
        public string Name;
        public string LocalizedDescription;
        public string LocalizedName;

        public TimeBuffInfo TimeBuffInfo;
        public UntilBuffInfo UntilBuffInfo;
        public PermanentRawMaterialBuffInfo PermanentRawMaterialBuffInfo;


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
            buffSystem.ServerRegisterClientCallback<PermanentVisionExpansionBuff, OnPermanentVisionExpansion>(TargetOnVisionPermenantExpand);
            //buffSystem.ServerRegisterClientCallback<PermanentAffinityBuff, OnPermanentAffinityAddition>(TargetOnPermanentAffinityBuff);
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
            
            if (buff is ITimedBuff timedBuff) {
                buffInfo.TimeBuffInfo = new TimeBuffInfo() {
                    TimeBuffTime = timedBuff.RemainingTime,
                    TimeBuffMaxTime = timedBuff.MaxDuration,
                };
            }

            if (buff is IUntilBuff untilBuff) {
                buffInfo.UntilBuffInfo = new UntilBuffInfo() {
                    UntilBuffTriggerTime = untilBuff.TotalCanBeTriggeredTime
                };
            }

            if (buff is IPermanentRawMaterialBuff permanentRawMaterialBuff) {
                int maxProgress;
                if (permanentRawMaterialBuff.CurrentLevel == permanentRawMaterialBuff.MaxLevel) {
                    maxProgress = 1;
                }
                else {
                    maxProgress = permanentRawMaterialBuff.ProgressPerLevel[permanentRawMaterialBuff.CurrentLevel];
                }
                buffInfo.PermanentRawMaterialBuffInfo = new PermanentRawMaterialBuffInfo()
                {
                    MaxLevel = permanentRawMaterialBuff.MaxLevel,
                    CurrentProgressInLevel = permanentRawMaterialBuff.CurrentProgressInLevel,
                    CurrentLevel = permanentRawMaterialBuff.CurrentLevel,
                    MaxProgressForCurrentLevel = maxProgress
                };
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


        [TargetRpc]
        private void TargetOnVisionPermenantExpand(BuffStatus e, OnPermanentVisionExpansion message)
        {
            OnPermanentVisionExpansion buff = message;
            if (e == BuffStatus.OnStart || e == BuffStatus.OnUpdate) {
                this.SendEvent(buff.VisionChangeEvent);
            }
        }

       
    }

    public struct ClientOnBuffUpdate {
        public BuffUpdateMode UpdateMode;
        public BuffInfo BuffInfo;
    }
}
