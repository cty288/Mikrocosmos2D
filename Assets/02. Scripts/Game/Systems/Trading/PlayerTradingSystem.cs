using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using MikroFramework.Event;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{

    public struct OnClientMoneyChange
    {
        public int OldMoney;
        public int NewMoney;
    }

    public struct OnClientMoneyNotEnough {
        
    }
    public interface IPlayerTradingSystem : ISystem
    {
        public int Money { get; }

        public void SpendMoney(int count);

        public void ReceiveMoney(int count);
    }

    public class PlayerTradingSystem : AbstractNetworkedSystem, IPlayerTradingSystem
    {



        [field: SyncVar(hook = nameof(OnClientMoneyChange))]
        public int Money { get; set; } = 50;

        public void SpendMoney(int count) {
            if (Money >= count) {
                Money -= count;
            }
            else
            {
                TargetOnPlayerMoneyNotEnough();
            }
        }

        public void ReceiveMoney(int count) {
            Money += count;
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            this.RegisterEvent<OnServerPlayerMoneyNotEnough>(OnServerPlayerMoneyNotEnough)
                .UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        private void OnServerPlayerMoneyNotEnough(OnServerPlayerMoneyNotEnough e) {
            if (e.PlayerIdentity == netIdentity) {
                TargetOnPlayerMoneyNotEnough();
            }
        }


        [ClientCallback]
        private void OnClientMoneyChange(int oldMoney, int newMoney)
        {
            if (hasAuthority)
            {
                this.SendEvent<OnClientMoneyChange>(new OnClientMoneyChange()
                {
                    OldMoney = oldMoney,
                    NewMoney = newMoney
                });
                if (newMoney > oldMoney) {
                    this.GetSystem<IAudioSystem>().PlaySound("AddMoney", SoundType.Sound2D);
                }
            }
        }

        [TargetRpc]
        private void TargetOnPlayerMoneyNotEnough() {
            this.SendEvent<OnClientMoneyNotEnough>();
        }
    }
}
