using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;

using UnityEngine;

namespace Mikrocosmos {

    public struct ClientInfoMessage {
        public string Name;
        public string InfoElementPrefabAssetName;
        public string InfoElementIconAssetName;
        public string InfoContainerSpriteAssetName;
        public string InfoContainerSliderAssetName;
        public string Description;
        public string Title;
        public float RemainingTime;
        public bool AutoDestroyWhenTimeUp;
        public bool ShowRemainingTime;
    }

    public struct OnInfoStartOrUpdate {
        public ClientInfoMessage Info;
    }

    public struct OnInfoStop {
        public string InfoName;
    }

    public static class InfoIconNames {
        public const string ICON_GREEN = "InfoIcon_Green";
        public const string ICON_PINK = "InfoIcon_Pink";
        public const string ICON_BLUE = "InfoIcon_Blue";
        public const string ICON_YELLOW = "InfoIcon_Yellow";
    }

    public static class InfoElementPrefabNames {
        public const string ICON_INFO_NORMAL = "MissionInfoElement";
        public const string ICON_WARNING_NORMAL = "WarningInfoElement";
        
    }

    public interface IClientInfoSystem : ISystem {
        void AddOrUpdateInfo(ClientInfoMessage message);
        void StopInfo(string messageName);
    }
    public class ClientInfoSystem : AbstractSystem, IClientInfoSystem {
        protected override void OnInit() {
            
        }

        public void AddOrUpdateInfo(ClientInfoMessage message) {
            this.SendEvent<OnInfoStartOrUpdate>(new OnInfoStartOrUpdate() {
                Info = message
            });
        }

        public void StopInfo(string messageName) {
           this.SendEvent<OnInfoStop>(new OnInfoStop() {
               InfoName = messageName
           });
        }
    }
}
