using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MikroFramework.Architecture;
using MikroFramework.DataStructures;
using UnityEngine;

namespace Mikrocosmos
{
    
    [Serializable]
    public class PlanetProperties
    {
        public string Name;
        public int ID;
        public PlanetTypeEnum PlanetType;
        public Type PlanetModelConcreteType;
        public List<GoodsConfigure> BuyItemList = new List<GoodsConfigure>();
        public List<GoodsConfigure> SellItemList = new List<GoodsConfigure>();
        public float Rarity;

        public PlanetProperties(string name, int id, PlanetTypeEnum planetType, Type planetModelConcreteType,  float rarity, List<GoodsConfigure> buyItems,
            List<GoodsConfigure> sellItems) {
            this.Name = name;
            this.ID = id;
            this.PlanetType = planetType;
            this.Rarity = rarity;
            this.BuyItemList = buyItems;
            this.SellItemList = sellItems;
            this.PlanetModelConcreteType = planetModelConcreteType;
          
        }
        public int GetID()
        {
            return ID;
        }

    }
  

    public class PlanetTable<T> : Table<PlanetProperties> where T : IPlanetModel
    {
        public TableIndex<int, PlanetProperties> IDIndex { get; private set; }
        public TableIndex<Type, PlanetProperties> TypeIndex { get; private set; }

        public PlanetTable()
        {
            IDIndex = new TableIndex<int, PlanetProperties>(item => item.GetID());
            TypeIndex = new TableIndex<Type, PlanetProperties>(item => item.PlanetModelConcreteType);
        }
        protected override void OnClear()
        {
            IDIndex.Clear();
            TypeIndex.Clear();
        }

        public override void OnAdd(PlanetProperties item)
        {
            IDIndex.Add(item);
            TypeIndex.Add(item);
        }

        public override void OnRemove(PlanetProperties item)
        {
            IDIndex.Remove(item);
            TypeIndex.Remove(item);
        }

        public PlanetProperties GetByID(int id)
        {
            return IDIndex.Get(id).FirstOrDefault();
        }
        public PlanetProperties GetByType(Type type)
        {
            return TypeIndex.Get(type).FirstOrDefault();
        }
    }


}
