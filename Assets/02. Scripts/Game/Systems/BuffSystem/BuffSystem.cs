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



    public enum BuffStatus {
        OnStart,
        OnTriggered,
        OnEnd
    }
    public interface IBuffSystem : ISystem
    {
        GameObject GetOwnerObject();
        void AddBuff<T>(IBuff buff) where T : IBuff;

        bool HasBuff<T>() where T : IBuff;

        void ServerRegisterClientCallback<T>(Action<BuffStatus> callback);
    }


    public class BuffSystem : AbstractNetworkedSystem, IBuffSystem
    {
        private Dictionary<Type, IBuff> buffs = new Dictionary<Type, IBuff>();
        private Dictionary<Type, Action<BuffStatus>> callbacks = new Dictionary<Type, Action<BuffStatus>>();

        private float timer;
        public GameObject GetOwnerObject()
        {
            return gameObject;
        }

        public void AddBuff<T>(IBuff buff) where T : IBuff
        {
            if (isServer) {
                if (!buffs.ContainsKey(buff.GetType())) {
                    StartCoroutine(AddNewBuffToList(typeof(T), buff));
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

        private IEnumerator AddNewBuffToList(Type type, IBuff buff) {
            yield return new WaitForEndOfFrame();
            if (!buffs.ContainsKey(buff.GetType())) {
                buffs.Add(buff.GetType(), buff);
                if (callbacks.ContainsKey(type))
                {
                    callbacks[type]?.Invoke(BuffStatus.OnStart);
                }
            }
            
        }

        public bool HasBuff<T>() where T : IBuff {
            return buffs.ContainsKey(typeof(T));
        }

        public void ServerRegisterClientCallback<T>(Action<BuffStatus> callback) {
            if (callbacks.ContainsKey(typeof(T))) {
                callbacks[typeof(T)] += callback;
            }
            else {
                callbacks.Add(typeof(T), callback);
            }
        }


        private void Update()
        {
            if (isServer) {
                foreach (KeyValuePair<Type, IBuff> b in buffs) {
                    IBuff buff = b.Value;
                    
                    buff.RemainingTime -= Time.deltaTime;
                    buff.FrequencyTimer -= Time.deltaTime;

                    if (buff.FrequencyTimer <= 0)
                    {
                        buff.FrequencyTimer += buff.Frequency;
                        buff.OnAction.Execute();
                        if (callbacks.ContainsKey(b.Key))
                        {
                            callbacks[b.Key]?.Invoke(BuffStatus.OnTriggered);
                        }
                    }
                }
              

                buffs.Where(x => x.Value.RemainingTime <= 0).ToList().
                    ForEach(x => {
                        if (callbacks.ContainsKey(x.Key)) {
                            callbacks[x.Key]?.Invoke(BuffStatus.OnEnd);
                        }
                        buffs.Remove(x.Key);
                    });
            }
            
        }
    }
}

