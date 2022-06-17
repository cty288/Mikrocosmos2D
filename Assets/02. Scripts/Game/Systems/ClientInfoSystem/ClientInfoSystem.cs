using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using Mirror.FizzySteam;
using UnityEngine;

namespace Mikrocosmos
{
    
    public struct ClientInfoMessage {
        public InfoType InfoType;
        public string Name;
        public string Description;
        public string Title;
        public float RemainingTime;
    }

    public struct OnInfoStartOrUpdate {
        public ClientInfoMessage Info;
    }

    public struct OnInfoStop {
        public string InfoName;
    }
    public enum InfoType {
        LongInfo,
        ShortInfo,
        LongWarning,
        ShortWarning
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
