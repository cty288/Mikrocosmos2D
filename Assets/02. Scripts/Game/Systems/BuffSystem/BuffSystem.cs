using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MikroFramework;
using MikroFramework.ActionKit;
using MikroFramework.Architecture;
using UnityEngine;

namespace Mikrocosmos
{

    
    public interface IBuff {

        string Name { get; }
        string GetLocalizedDescriptionText();

        string GetLocalizedName();
    }

    /// <summary>
    /// Give a buff some "time limit", otherwise, a buff would last unlimited amount of time
    /// </summary>
    public interface ITimedBuff : IBuff {
      
        
        float MaxDuration { get; }
        float RemainingTime { get; set; }
        MikroAction OnTimedActionEnd { get; set; }
    }


    /// <summary>
    /// This is a type of buff that is triggered by an UntilAction
    /// </summary>
    public interface IUntilBuff : IBuff {
        UntilAction UntilAction { get; set; }
        int TotalCanBeTriggeredTime { get; set; }
    }

    /// <summary>
    /// This is a type of buff that can be stacked (added repeatably)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IStackableBuff<T> : IBuff where T : IBuff {
        void OnBuffStacked(T addedBuff);
    }

    /// <summary>
    /// This is a type of buff that can be triggered under a frequency
    /// </summary>
    public interface IHaveFrequencyBuff: IBuff  {
        MikroAction OnActionFrequentTriggered { get; set; }
        float Frequency { get; }
        float FrequencyTimer { get; set; }
    }




    
    [Serializable]
    public abstract class TimedFrequentBuff : ITimedBuff, IHaveFrequencyBuff
    {
       

        [field: SerializeField]
        public float MaxDuration { get; protected set; }

        [field: SerializeField]
        public float RemainingTime { get; set; }

        public MikroAction OnTimedActionEnd { get;  set; } = CallbackAction.Allocate(() => { });

        public MikroAction OnActionFrequentTriggered { get; set; }

        [field: SerializeField]
        public float Frequency { get; protected set; }


        [field: SerializeField]
        public float FrequencyTimer { get; set; }


       

        public TimedFrequentBuff(float maxDuration, float frequency,
            MikroAction onFrequencyActionTriggered)
        {
            MaxDuration = maxDuration;
            RemainingTime = maxDuration;
            Frequency = frequency;
           
            FrequencyTimer = frequency;
            OnActionFrequentTriggered = onFrequencyActionTriggered;
            OnActionFrequentTriggered.SetAutoRecycle(false);
        }

        public abstract string Name { get; }
        public abstract string GetLocalizedDescriptionText();
        public abstract string GetLocalizedName();
    }


    [Serializable]
    public abstract class UntilBuff : IUntilBuff
    {
       
        public UntilBuff(int canBeTriggeredTime, UntilAction untilAction) {
            UntilAction = untilAction;
            untilAction.SetAutoRecycle(false);
            TotalCanBeTriggeredTime = canBeTriggeredTime;
        }

        public MikroAction OnAction {
            get {
                return UntilAction;
            }
        }

        public abstract string GetLocalizedDescriptionText();
        public abstract string GetLocalizedName();

        public UntilAction UntilAction { get; set; } 
        public int TotalCanBeTriggeredTime { get;  set; }

        public abstract string Name { get; }
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

        Action<IBuff> ServerOnBuffStart { get; set; }

        Action<IBuff> ServerOnBuffUpdate { get; set; }

        Action<IBuff> ServerOnBuffStop { get; set; }
    }


    public class BuffSystem : AbstractNetworkedSystem, IBuffSystem
    {
        private Dictionary<Type, IBuff> buffs = new Dictionary<Type, IBuff>();
        private Dictionary<Type, Action<BuffStatus>> callbacks = new Dictionary<Type, Action<BuffStatus>>();

        
        public GameObject GetOwnerObject()
        {
            return gameObject;
        }

        public void AddBuff<T>(IBuff buff) where T : IBuff {

            if (isServer) {
                if (!buffs.ContainsKey(buff.GetType())) {
                    StartCoroutine(AddNewBuffToList(typeof(T), buff));
                }
                else {
                    if (buffs[buff.GetType()] is IStackableBuff<T> repeatableBuff) {
                        repeatableBuff.OnBuffStacked((T)buff);
                        ServerOnBuffUpdate?.Invoke(buff);
                    }
                }
            }
        }

        private IEnumerator AddNewBuffToList(Type type, IBuff buff) {
            yield return new WaitForEndOfFrame();
            if (!buffs.ContainsKey(buff.GetType())) {
                buffs.Add(buff.GetType(), buff);
                if (callbacks.ContainsKey(type)) {
                    callbacks[type]?.Invoke(BuffStatus.OnStart);
                }

                ServerOnBuffStart?.Invoke(buff);
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

        public Action<IBuff> ServerOnBuffStart { get; set; }
        public Action<IBuff> ServerOnBuffUpdate { get; set; }
        public Action<IBuff> ServerOnBuffStop { get; set; }


        private void Update()
        {
            if (isServer) {
                foreach (KeyValuePair<Type, IBuff> b in buffs) {
                    IBuff buff = b.Value;
                    
                    if (buff is ITimedBuff timedBuff) {
                        timedBuff.RemainingTime -= Time.deltaTime;
                    }

                    if (buff is IHaveFrequencyBuff frequencyBuff) {
                        frequencyBuff.FrequencyTimer -= Time.deltaTime;
                        if (frequencyBuff.FrequencyTimer <= 0)
                        {
                            frequencyBuff.FrequencyTimer += frequencyBuff.Frequency;
                            
                            frequencyBuff.OnActionFrequentTriggered.Execute();
                            if (callbacks.ContainsKey(b.Key)) {
                                callbacks[b.Key]?.Invoke(BuffStatus.OnTriggered);
                            }

                            ServerOnBuffUpdate?.Invoke(buff);
                        }
                    }

                    if (buff is IUntilBuff untilBuff) {
                        if (untilBuff.UntilAction.Finished) {
                            untilBuff.TotalCanBeTriggeredTime--;
                            callbacks[b.Key]?.Invoke(BuffStatus.OnTriggered);
                            ServerOnBuffUpdate?.Invoke(buff);                            
                        }
                    }
                    
                }
              
              
                buffs.Where(x => {
                        if (x.Value is ITimedBuff timedBuff) {
                            if (timedBuff.RemainingTime <= 0) {
                                timedBuff.OnTimedActionEnd.Execute();
                                return true;
                            }

                            return false;
                        }

                        if (x.Value is IUntilBuff untilBuff) {
                            if (untilBuff.UntilAction.Finished) {
                                if (untilBuff.TotalCanBeTriggeredTime > 0) {
                                    untilBuff.UntilAction.Reset();
                                    untilBuff.UntilAction.Execute();
                                    return false;
                                }
                               
                                return true;
                            }
                            return false;
                        }
                        return false;
                    }).ToList().
                    ForEach(x => {
                        if (callbacks.ContainsKey(x.Key)) {
                            callbacks[x.Key]?.Invoke(BuffStatus.OnEnd);
                        }

                        if (x.Value is IUntilBuff untilBuff) {
                            untilBuff.UntilAction.RecycleToCache();
                        }
                        if (x.Value is IHaveFrequencyBuff frequencyBuff)
                        {
                            frequencyBuff.OnActionFrequentTriggered.RecycleToCache();
                        }

                        ServerOnBuffStop?.Invoke(x.Value);
                        buffs.Remove(x.Key);
                    });
            }
            
        }
    }
}

