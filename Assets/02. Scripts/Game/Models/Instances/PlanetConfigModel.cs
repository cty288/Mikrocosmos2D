using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MikroFramework.Architecture;
using MikroFramework.DataStructures;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Mikrocosmos
{

    [Serializable]
    public class PlanetProperties {
        public string Name;
        public int ID;
        public string ScriptType;
     
        public int GetID() {
            return ID;
        }

        public Type GetConcreteType()
        {
            return Type.GetType($"MainGame.{ScriptType}");
        }
    }
    [CreateAssetMenu(fileName = "CardProperties")]
    public class PlanetBasicProperties : ScriptableObject {
        public List<PlanetProperties> CardDatas;
    }


    public class PlanetTable<T> : Table<PlanetProperties> where T : IPlanetModel
    {
        public TableIndex<int, PlanetProperties> IDIndex { get; private set; }

        public PlanetTable()
        {
            IDIndex = new TableIndex<int, PlanetProperties>(item => item.GetID());
        }
        protected override void OnClear()
        {
            IDIndex.Clear();
        }

        public override void OnAdd(PlanetProperties item)
        {
            IDIndex.Add(item);
        }

        public override void OnRemove(PlanetProperties item)
        {
            IDIndex.Remove(item);
        }

        public PlanetProperties GetByID(int id)
        {
            return IDIndex.Get(id).FirstOrDefault();
        }
    }

    public interface IPlanetConfigModel : IModel
    {
        IPlanetModel GetNewPlanetModelFromID(int id);
    }
    public class PlanetConfigModel : AbstractModel, IPlanetConfigModel
    {
        public PlanetTable<IPlanetModel> AllPlanetInfos = new PlanetTable<IPlanetModel>();
        protected override void OnInit() {
            
        }

        public IPlanetModel GetNewPlanetModelFromID(int id) {
            PlanetProperties planetPropertities = AllPlanetInfos.GetByID(id);
            return Activator.CreateInstance(planetPropertities.GetConcreteType(), planetPropertities) as IPlanetModel;
        }
    }
}
