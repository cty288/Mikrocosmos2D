using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MikroFramework;
using MikroFramework.ActionKit;
using MikroFramework.Architecture;
using MikroFramework.Event;
using MikroFramework.TimeSystem;
using Mirror;
using Polyglot;
using UnityEngine;

namespace Mikrocosmos {

    public class BuffClientMessage {

    }


    public interface IBuff {

        string Name { get; }
        string GetLocalizedDescriptionText(Language languege);

        string GetLocalizedName(Language languege);

        BuffClientMessage MessageToClient { get; set; }

        public IBuffSystem Owner { get; set; }

        public NetworkIdentity OwnerIdentity { get; set; }
        void OnBuffAdded();
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

        public int CurrentLevel { get; }

        public int ReadyToAddLevel { get; }

        public int ReadyToAddProgress { get; }

        public int ReduceProgress(int amount);

    }

    public abstract class PermanentRawMaterialBuff: IPermanentRawMaterialBuff {
        public IBuffSystem Owner { get; set; }
        public NetworkIdentity OwnerIdentity { get; set; }

       
        
        public void OnBuffAdded() {
            
            RecalculateLevelAndProgress();
        }
        
        public void OnBuffStacked(IPermanentRawMaterialBuff addedBuff) {
            //Decompose addedBuff to get its total progress
            /*
            int totalProgress = 0;
            for (int i = 0; i < addedBuff.CurrentLevel; i++) {
                totalProgress += addedBuff.ProgressPerLevel[i];
            }

            totalProgress += addedBuff.CurrentProgressInLevel;

            
            CurrentProgressInLevel += totalProgress;*/
            
            ReadyToAddLevel = addedBuff.ReadyToAddLevel;
            ReadyToAddProgress = addedBuff.ReadyToAddProgress;
            
            RecalculateLevelAndProgress();
        }

        public abstract void OnLevelUp(int previousLevel, int currentLevel);

        public PermanentRawMaterialBuff(int currentLevel = 0, int currentProgressInLevel = 1) {
            /*
            CurrentLevel = 0;
            int totalProgress = 0;
            for (int i = 0; i < currentLevel; i++)
            {
                totalProgress += ProgressPerLevel[i];
            }
            
            
            CurrentProgressInLevel =totalProgress +  currentProgressInLevel;*/
            ReadyToAddLevel = currentLevel;
            ReadyToAddProgress = currentProgressInLevel;

        }

        public int ReduceProgress(int amount) {
            //reduce progress in current level, if it is less than 0, then decrease current level until it is greater than 0
            int previousLevel = CurrentLevel;
            CurrentProgressInLevel -= amount;
            while (CurrentProgressInLevel<0) {
                int remaining = Mathf.Abs(CurrentProgressInLevel);
                CurrentLevel--;
                if (CurrentLevel > 0) {
                    CurrentProgressInLevel = ProgressPerLevel[CurrentLevel] - remaining;
                }else {
                   // Owner.RemoveBuff(this);
                    CurrentProgressInLevel = 0;
                    break;
                }
            }

            OnLevelProgressDecrease(previousLevel, CurrentLevel);
            return CurrentLevel;
        }

        protected abstract void OnLevelProgressDecrease(int previousLevel, int currentLevel);


        //if current progress is greater than the maximum progress for current level, then increase current level until current progress is less than the maximum progress for current level
        protected void RecalculateLevelAndProgress() {
            int previousLevel = CurrentLevel;
            int totalProgress = 0;
            for (int i = 0; i < ReadyToAddLevel; i++) {
                if (i >= ProgressPerLevel.Count) {
                    totalProgress += ProgressPerLevel[^1];
                }
                else {
                    totalProgress += ProgressPerLevel[i];
                }
               
            }
            totalProgress += ReadyToAddProgress;


            CurrentProgressInLevel += totalProgress;

            ReadyToAddLevel = 0;
            ReadyToAddProgress = 0;
            
            while (CurrentLevel < MaxLevel && CurrentProgressInLevel >= ProgressPerLevel[CurrentLevel]) {
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
        public abstract string GetLocalizedDescriptionText(Language languege);

        public abstract string GetLocalizedName(Language languege);

        public abstract   BuffClientMessage MessageToClient { get; set; }
      

        public abstract  int MaxLevel { get; set; }
        public abstract List<int> ProgressPerLevel { get; set; }
        public int CurrentProgressInLevel { get; set; }
        public int CurrentLevel { get; protected set; }
        public int ReadyToAddLevel { get; private set; }
        public int ReadyToAddProgress { get; private set; }

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
        public abstract string GetLocalizedDescriptionText(Language languege);
        public abstract string GetLocalizedName(Language languege);
        public  abstract BuffClientMessage MessageToClient { get; set; }
        public IBuffSystem Owner { get; set; }
        public NetworkIdentity OwnerIdentity { get; set; }

        public virtual void OnBuffAdded() {
            
        }
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

        public abstract string GetLocalizedDescriptionText(Language languege);
        public abstract string GetLocalizedName(Language languege);
        public abstract  BuffClientMessage MessageToClient { get; set; }
        public IBuffSystem Owner { get; set; }
        public NetworkIdentity OwnerIdentity { get; set; }

        public virtual void OnBuffAdded() {
            
        }

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

        void ForceTriggerUpdateBuff(IBuff buff);

        void RawMaterialProgressDecrease(Type type, int progress);

        void RawMaterialLevelDecrease(Type type, int level);

        void RemoveBuff(IBuff buff);
        void RemoveBuff<T>() where T:class, IBuff;
        List<IPermanentRawMaterialBuff> GetAllPermanentRawMaterialBuffs();
        
        /// <summary>
        /// Note: Register callback in this way will not be able to call the callback on clients because IBuff is not serializable by Mirror, use ServerRegisterCallback<T, T2>(Action<BuffStatus, T2> callback)
        /// instead if you want to call the callback on clients.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="callback"></param>
        void ServerRegisterCallback<T,T2>(Action<T, BuffStatus, T2> callback) where T2: BuffClientMessage where T: class, IBuff;

        void ServerRegisterCallback<T, T2>(Action<BuffStatus, T2> callback) where T2 : BuffClientMessage;

        Action<IBuff> ServerOnBuffStart { get; set; }

        Action<IBuff> ServerOnBuffUpdate { get; set; }

        Action<IBuff> ServerOnBuffStop { get; set; }
    }


    public class BuffSystem : AbstractNetworkedSystem, IBuffSystem
    {
        private Dictionary<Type, IBuff> buffs = new Dictionary<Type, IBuff>();
        private Dictionary<Type, Action<BuffStatus, BuffClientMessage>> callbacks = new Dictionary<Type, Action<BuffStatus, BuffClientMessage>>();
        [SerializeField] private int permanentLevelDeduceWhenDie = 1;
        [SerializeField] private int minimumPermanentBuffLevelToDeduct = 3;
        private IGameProgressSystem progressSystem;
        public override void OnStartServer() {
            base.OnStartServer();
            this.RegisterEvent<OnPlayerDie>(OnPlayerDie).UnRegisterWhenGameObjectDestroyed(gameObject);
            this.GetSystem<ITimeSystem>().AddDelayTask(0.2f, () => {
                progressSystem = this.GetSystem<IGameProgressSystem>();
            });
        }

        private void OnPlayerDie(OnPlayerDie e) {
            if (e.SpaceshipIdentity == netIdentity) {
                List<IPermanentRawMaterialBuff> rawMaterialBuffs = GetAllPermanentRawMaterialBuffs();

                foreach (IPermanentRawMaterialBuff buff in rawMaterialBuffs) {
                    if (buff.CurrentLevel < minimumPermanentBuffLevelToDeduct) {
                        continue;
                    }
                    int progressDeduce = 0;
                    int levelDecrease = permanentLevelDeduceWhenDie;
                    for (int i = buff.CurrentLevel; i >= 1; i--) {
                        if (levelDecrease <= 0) {
                            break;
                        }

                        if (i == buff.MaxLevel) {
                            progressDeduce += 1;
                        }
                        else {
                            progressDeduce += buff.ProgressPerLevel[i];
                        }

                        levelDecrease--;
                    }

                    RawMaterialProgressDecrease(buff.GetType(), progressDeduce);

                }
            }
        }
        public void RawMaterialProgressDecrease(Type type, int progressDeduce) {
            if (buffs.ContainsKey(type)) {
                IPermanentRawMaterialBuff buff = buffs[type] as IPermanentRawMaterialBuff;
                int resultLevel = buff.ReduceProgress(progressDeduce);
                
                ForceTriggerUpdateBuff(buff);
                if (resultLevel <= 0) {
                    RemoveBuff(buff);
                }
            }
            
        }

        public void RawMaterialLevelDecrease(Type type, int level) {
            int progressDeduce = 0;
            int levelDecrease = level;
            if (buffs.ContainsKey(type) && buffs[type] is IPermanentRawMaterialBuff) {
                IPermanentRawMaterialBuff rawMaterial = buffs[type] as IPermanentRawMaterialBuff;

                for (int i = rawMaterial.CurrentLevel; i >= 1; i--)
                {
                    if (levelDecrease <= 0)
                    {
                        break;
                    }

                    if (i == rawMaterial.MaxLevel)
                    {
                        progressDeduce += 1;
                    }
                    else
                    {
                        progressDeduce += rawMaterial.ProgressPerLevel[i];
                    }

                    levelDecrease--;
                }

                RawMaterialProgressDecrease(type, progressDeduce);
            }
          
        }

        public GameObject GetOwnerObject()
        {
            return gameObject;
        }

        
        public void AddBuff<T>(IBuff buff) where T : IBuff {

            if (isServer) {
                if (progressSystem == null || progressSystem.GameState != GameState.InGame)
                {
                    return;
                }
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
                buff.Owner = this;
                buff.OwnerIdentity = netIdentity;
                
                buff.OnBuffAdded();

                if (buff is IUntilBuff untilBuff) {
                    untilBuff.UntilAction.Execute();
                }
                
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

        public void ForceTriggerUpdateBuff(IBuff buff) {
            if (callbacks.ContainsKey(buff.GetType())) {
                callbacks[buff.GetType()]?.Invoke(BuffStatus.OnUpdate, buff.MessageToClient);
            }
            ServerOnBuffUpdate?.Invoke(buff);
        }


        

        public List<IPermanentRawMaterialBuff> GetAllPermanentRawMaterialBuffs() {
            List<IPermanentRawMaterialBuff> rawMaterialBuffs = new List<IPermanentRawMaterialBuff>();
            foreach (var buff in buffs) {
                if (buff.Value is IPermanentRawMaterialBuff value) {
                    rawMaterialBuffs.Add(value);
                }
            }
            return rawMaterialBuffs;
        }

        public void ServerRegisterCallback<T,T2>(Action<T, BuffStatus, T2> callback) where T2: BuffClientMessage where T: class, IBuff{
            if (callbacks.ContainsKey(typeof(T))) {
                callbacks[typeof(T)] += ((status, message) => callback.Invoke(buffs[typeof(T)] as T, status, message as T2));
            }
            else {
                callbacks.Add(typeof(T), ((status, message) => callback.Invoke(buffs[typeof(T)] as T , status, message as T2)));
            }
        }

        public void ServerRegisterCallback<T, T2>(Action<BuffStatus, T2> callback) where T2 : BuffClientMessage {
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

        public void RemoveBuff(IBuff buff) {
            StartCoroutine(RemoveBuffFromList(buff.GetType()));
        }
        public void RemoveBuff<T>() where T : class, IBuff {
            StartCoroutine(RemoveBuffFromList(typeof(T)));
        }
        private IEnumerator RemoveBuffFromList(Type type)
        {
            yield return new WaitForEndOfFrame();
            if (isServer) {
                if (buffs.ContainsKey(type)) {
                    IBuff buff = buffs[type];
                    if (callbacks.ContainsKey(type)) {
                        callbacks[type]?.Invoke(BuffStatus.OnEnd, buff.MessageToClient);
                    }
                    
                    if (buff is IUntilBuff untilBuff) {
                        untilBuff.UntilAction.RecycleToCache();
                    }
                    if (buff is IHaveFrequencyBuff frequencyBuff) {
                        frequencyBuff.OnActionFrequentTriggered.RecycleToCache();
                    }

                    ServerOnBuffStop?.Invoke(buff);
                    buffs.Remove(buff.GetType());
                }
            }

        }
        
        private void Update()
        {
            if (isServer) {
                if (progressSystem== null ||  progressSystem.GameState != GameState.InGame) {
                    return;
                }
                
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
                            if (callbacks.ContainsKey(b.Key)) {
                                callbacks[b.Key]?.Invoke(BuffStatus.OnTriggered, buff.MessageToClient);
                            }
                          
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

