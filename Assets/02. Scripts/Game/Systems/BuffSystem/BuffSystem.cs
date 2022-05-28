using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MikroFramework.ActionKit;
using MikroFramework.Architecture;
using UnityEngine;

namespace Mikrocosmos
{
    public interface IBuff
    {
        float MaxDuration { get; }
        float RemainingTime { get; set; }
        float Frequency { get; }
        float FrequencyTimer { get; set; }
        MikroAction OnAction { get; }
    }

    public interface IRepeatableBuff<T> : IBuff where T : IBuff
    {
        void OnBuffRepeated(T addedBuff);
    }


    [Serializable]
    public abstract class Buff : IBuff
    {
        [field: SerializeField]
        public float MaxDuration { get; protected set; }

        [field: SerializeField]
        public float RemainingTime { get; set; }

        [field: SerializeField]
        public float Frequency { get; protected set; }


        [field: SerializeField]
        public float FrequencyTimer { get; set; }


        public MikroAction OnAction { get; protected set; }

        protected IBuffSystem targetBuffOwner;

        public Buff(float maxDuration, float frequency, IBuffSystem buffOwner)
        {
            MaxDuration = maxDuration;
            RemainingTime = maxDuration;
            Frequency = frequency;
            targetBuffOwner = buffOwner;
            FrequencyTimer = frequency;
        }
    }




    public interface IBuffSystem : ISystem
    {
        GameObject GetOwnerObject();
        void AddBuff<T>(IBuff buff) where T : IBuff;
    }


    public class BuffSystem : AbstractNetworkedSystem, IBuffSystem
    {
        private Dictionary<Type, IBuff> buffs = new Dictionary<Type, IBuff>();
        private float timer;
        public GameObject GetOwnerObject()
        {
            return gameObject;
        }

        public void AddBuff<T>(IBuff buff) where T : IBuff
        {
            if (isServer) {
                if (!buffs.ContainsKey(buff.GetType()))
                {
                    buffs.Add(buff.GetType(), buff);
                }
                else
                {
                    if (buffs[buff.GetType()] is IRepeatableBuff<T> repeatableBuff)
                    {
                        repeatableBuff.OnBuffRepeated((T)buff);
                    }
                }
            }
          
        }

        private void Update()
        {
            if (isServer) {
                foreach (Type key in buffs.Keys)
                {
                    IBuff buff = buffs[key];
                    buff.RemainingTime -= Time.deltaTime;
                    buff.FrequencyTimer -= Time.deltaTime;

                    if (buff.FrequencyTimer <= 0)
                    {
                        buff.FrequencyTimer += buff.Frequency;
                        buff.OnAction.Execute();
                    }
                }

                buffs.Where(x => x.Value.RemainingTime <= 0).ToList().
                    ForEach(x => buffs.Remove(x.Key));
            }
            
        }
    }
}

