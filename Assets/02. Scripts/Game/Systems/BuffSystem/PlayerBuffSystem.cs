using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework;
using MikroFramework.Architecture;
using MikroFramework.Event;
using Mirror;
using Polyglot;
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
        [SerializeField]
        private Language clientLanguage;
        
        private void Awake() {
            buffSystem = GetComponent<IBuffSystem>();
        }

        public override void OnStartServer() {
            base.OnStartServer();
            buffSystem.ServerOnBuffUpdate += OnServerBuffUpdate;
            buffSystem.ServerOnBuffStart += OnServerBuffStart;
            buffSystem.ServerOnBuffStop += OnServerBuffStop;

          //  buffSystem.ServerRegisterCallback<PermanentSpeedBuff, BuffClientMessage>(OnServerPermanentSpeedBuff);

            buffSystem.ServerRegisterCallback<VisionExpansionBuff, OnVisionExpansion>(TargetOnVisionExpand);
            buffSystem.ServerRegisterCallback<PermanentVisionExpansionBuff, OnPermanentVisionExpansion>(TargetOnVisionPermenantExpand);
            //buffSystem.ServerRegisterClientCallback<PermanentAffinityBuff, OnPermanentAffinityAddition>(TargetOnPermanentAffinityBuff);
            clientLanguage = connectionToClient.identity.GetComponent<NetworkMainGamePlayer>().ClientLanguage;
            this.RegisterEvent<OnServerSpaceshipOverweight>(OnServerSpaceshipOverweight)
                .UnRegisterWhenGameObjectDestroyed(gameObject);
            
        }

     

        private void OnServerSpaceshipOverweight(OnServerSpaceshipOverweight e) {
            if (e.Spaceship == gameObject) {
                buffSystem.AddBuff<OverweightDeBuff>(new OverweightDeBuff(1,
                    UntilAction.Allocate((() =>
                        Math.Abs(e.SpaceshipModel.Acceleration - e.MinimumAcceleration) >= e.Tolerance))));
            }
        }


        #region MessageSenders

        public override void OnStopServer()
        {
            base.OnStopServer();
            buffSystem.ServerOnBuffUpdate -= OnServerBuffUpdate;
            buffSystem.ServerOnBuffStart -= OnServerBuffStart;
            buffSystem.ServerOnBuffStop -= OnServerBuffStop;
        }
        private void OnServerBuffStart(IBuff buff)
        {
            BuffInfo buffInfo = SetupBuffInfo(buff, clientLanguage);
            TargetUpdateBuffInfo(buffInfo, BuffUpdateMode.Start);
        }

        private void OnServerBuffStop(IBuff buff)
        {
            BuffInfo buffInfo = SetupBuffInfo(buff, clientLanguage);
            TargetUpdateBuffInfo(buffInfo, BuffUpdateMode.Stop);
        }

        private void OnServerBuffUpdate(IBuff buff)
        {
            BuffInfo buffInfo = SetupBuffInfo(buff,clientLanguage);
            TargetUpdateBuffInfo(buffInfo, BuffUpdateMode.Update);
        }


        private BuffInfo SetupBuffInfo(IBuff buff, Language language)
        {
            BuffInfo buffInfo = new BuffInfo()
            {
                LocalizedDescription = buff.GetLocalizedDescriptionText(language),
                LocalizedName = buff.GetLocalizedName(language),
                Name = buff.Name
            };

            if (buff is ITimedBuff timedBuff)
            {
                buffInfo.TimeBuffInfo = new TimeBuffInfo()
                {
                    TimeBuffTime = timedBuff.RemainingTime,
                    TimeBuffMaxTime = timedBuff.MaxDuration,
                };
            }

            if (buff is IUntilBuff untilBuff)
            {
                buffInfo.UntilBuffInfo = new UntilBuffInfo()
                {
                    UntilBuffTriggerTime = untilBuff.TotalCanBeTriggeredTime
                };
            }

            if (buff is IPermanentRawMaterialBuff permanentRawMaterialBuff)
            {
                int maxProgress;
                if (permanentRawMaterialBuff.CurrentLevel == permanentRawMaterialBuff.MaxLevel)
                {
                    maxProgress = 1;
                }
                else
                {
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
        public void TargetUpdateBuffInfo(BuffInfo buffInfo, BuffUpdateMode updateStatus)
        {
            this.SendEvent<ClientOnBuffUpdate>(new ClientOnBuffUpdate()
            {
                BuffInfo = buffInfo,
                UpdateMode = updateStatus
            });
            Debug.Log($"Client Buff Update: UpdateMode - {updateStatus}, BuffName: {buffInfo.Name}");
        }


        #endregion


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
