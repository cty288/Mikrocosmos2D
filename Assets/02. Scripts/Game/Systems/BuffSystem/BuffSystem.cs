using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MikroFramework;
using MikroFramework.ActionKit;
using MikroFramework.Architecture;
using UnityEngine;

namespace Mikrocosmos {

    public class BuffClientMessage {

    }


    public interface IBuff {

        string Name { get; }
        string GetLocalizedDescriptionText();

        string GetLocalizedName();

        BuffClientMessage MessageToClient { get; set; }
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
    public interface IHaveFrequencyBuff : IBuff {
        MikroAction OnActionFrequentTriggered { get; set; }
        float Frequency { get; }
        float FrequencyTimer { get; set; }
    }


    public interface IPermanentRawMaterialBuff : IBuff, IStackableBuff<IPermanentRawMaterialBuff> {
         public int MaxLevel { get; set; }
        
        public List<int> ProgressPerLevel { get; set; }

        public int CurrentProgressInLevel { get; set; }

        public int CurrentLevel { get; set; }
        
    }

    public abstract class PermanentRawMaterialBuff: IPermanentRawMaterialBuff{
        public void OnBuffStacked(IPermanentRawMaterialBuff addedBuff) {
            //Decompose addedBuff to get its total progress
            int totalProgress = 0;
            for (int i = 0; i < addedBuff.CurrentLevel; i++) {
                totalProgress += addedBuff.ProgressPerLevel[i];
            }

            totalProgress += addedBuff.CurrentProgressInLevel;

            
            CurrentProgressInLevel += totalProgress;
            RecalculateLevelAndProgress();
        }

        public abstract void OnLevelUp(int previousLevel, int currentLevel);

        public PermanentRawMaterialBuff(int currentLevel = 0, int currentProgressInLevel = 1) {
            CurrentLevel = currentLevel;
            CurrentProgressInLevel = currentProgressInLevel;
            RecalculateLevelAndProgress();
        }

        //if current progress is greater than the maximum progress for current level, then increase current level until current progress is less than the maximum progress for current level
        private void RecalculateLevelAndProgress() {
            int previousLevel = CurrentLevel;
            while (CurrentProgressInLevel >= ProgressPerLevel[CurrentLevel] && CurrentLevel < MaxLevel) {
                CurrentProgressInLevel -= ProgressPerLevel[CurrentLevel];
                CurrentLevel++;
                if (CurrentLevel == MaxLevel) {
                    CurrentProgressInLevel = 0;
                }
            }
            if (previousLevel < CurrentLevel) {
                OnLevelUp(previousLevel, CurrentLevel);
            }

        }

        public abstract string Name { get; }
        public abstract string GetLocalizedDescriptionText();

        public abstract string GetLocalizedName();

        public abstract   BuffClientMessage MessageToClient { get; set; }
        public abstract  int MaxLevel { get; set; }
        public abstract List<int> ProgressPerLevel { get; set; }
        public int CurrentProgressInLevel { get; set; }
        public int CurrentLevel { get; set; }

      
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
        public  abstract BuffClientMessage MessageToClient { get; set; }
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
        public abstract  BuffClientMessage MessageToClient { get; set; }

        public UntilAction UntilAction { get; set; } 
        public int TotalCanBeTriggeredTime { get;  set; }

        public abstract string Name { get; }
    }



    public enum BuffStatus {
        OnStart,
        OnUpdate,
        OnTriggered,
        OnEnd
    }
    public interface IBuffSystem : ISystem
    {
        GameObject GetOwnerObject();
        void AddBuff<T>(IBuff buff) where T : IBuff;

        bool HasBuff<T>(out T buff) where T : class, IBuff;
        bool HasBuff<T>() where T : class, IBuff;
        void ServerRegisterClientCallback<T,T2>(Action<BuffStatus, T2> callback) where T2: BuffClientMessage;

        Action<IBuff> ServerOnBuffStart { get; set; }

        Action<IBuff> ServerOnBuffUpdate { get; set; }

        Action<IBuff> ServerOnBuffStop { get; set; }
    }


    public class BuffSystem : AbstractNetworkedSystem, IBuffSystem
    {
        private Dictionary<Type, IBuff> buffs = new Dictionary<Type, IBuff>();
        private Dictionary<Type, Action<BuffStatus, BuffClientMessage>> callbacks = new Dictionary<Type, Action<BuffStatus, BuffClientMessage>>();

        
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
                        if (callbacks.ContainsKey(buff.GetType())) {
                            callbacks[repeatableBuff.GetType()]?.Invoke(BuffStatus.OnUpdate, repeatableBuff.MessageToClient);
                        }
                        ServerOnBuffUpdate?.Invoke(repeatableBuff);
                    }else if (buffs[buff.GetType()] is IStackableBuff<IPermanentRawMaterialBuff> rawMaterialBuff) {
                        
                        rawMaterialBuff.OnBuffStacked((IPermanentRawMaterialBuff)buff);
                        if (callbacks.ContainsKey(rawMaterialBuff.GetType()))
                        {
                            callbacks[rawMaterialBuff.GetType()]?.Invoke(BuffStatus.OnUpdate, rawMaterialBuff.MessageToClient);
                        }
                        ServerOnBuffUpdate?.Invoke(rawMaterialBuff);
                    }
                }
            }
        }

        private IEnumerator AddNewBuffToList(Type type, IBuff buff) {
            yield return new WaitForEndOfFrame();
            if (!buffs.ContainsKey(buff.GetType())) {
                buffs.Add(buff.GetType(), buff);
                if (callbacks.ContainsKey(type)) {
                    callbacks[type]?.Invoke(BuffStatus.OnStart, buff.MessageToClient);
                }

                ServerOnBuffStart?.Invoke(buff);
            }
            
        }

        public bool HasBuff<T>(out T buff) where T : class, IBuff {
            IBuff temp = null;
            buffs.TryGetValue(typeof(T), out temp);
            buff = temp as T;
            return buff != null;
        }

        public bool HasBuff<T>() where T : class, IBuff {
            return buffs.ContainsKey(typeof(T));
        }

        public void ServerRegisterClientCallback<T,T2>(Action<BuffStatus, T2> callback) where T2: BuffClientMessage{
            if (callbacks.ContainsKey(typeof(T))) {
                callbacks[typeof(T)] += ((status, message) => callback.Invoke(status, message as T2));
            }
            else {
                callbacks.Add(typeof(T), ((status, message) => callback.Invoke(status, message as T2)));
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
                                callbacks[b.Key]?.Invoke(BuffStatus.OnTriggered, buff.MessageToClient);
                            }

                            ServerOnBuffUpdate?.Invoke(buff);
                        }
                    }

                    if (buff is IUntilBuff untilBuff) {
                        if (untilBuff.UntilAction.Finished) {
                            untilBuff.TotalCanBeTriggeredTime--;
                            callbacks[b.Key]?.Invoke(BuffStatus.OnTriggered, buff.MessageToClient);
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
                            callbacks[x.Key]?.Invoke(BuffStatus.OnEnd, x.Value.MessageToClient);
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

