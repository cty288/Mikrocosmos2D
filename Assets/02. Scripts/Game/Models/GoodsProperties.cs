using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MikroFramework.Architecture;
using MikroFramework.DataStructures;
using MikroFramework.ResKit;
using UnityEngine;

namespace Mikrocosmos
{
    [Serializable]
    public struct ItemTradeProperties {
        public int BasicBuyPrice;
        public int BasicSellPrice;
        public GoodsRarity Rarity;

        public ItemTradeProperties(int basicBuyPrice, int basicSellPrice, GoodsRarity rarity)
        {
            BasicBuyPrice = basicBuyPrice;
            BasicSellPrice = basicSellPrice;
            Rarity = rarity;
        }
    }
    [Serializable]
    public struct UseableItemProperties {
        public bool CanBeUsed;
        public ItemUseMode UseMode;
        public float UseFrequency;
        public int MaxDurability;

        public UseableItemProperties(bool canbeUsed, ItemUseMode useMode, float frequency, int durability) {
            CanBeUsed = canbeUsed;
            UseMode = useMode;
            UseFrequency = frequency;
            MaxDurability = durability;
        }
    }
    [Serializable]
    public class GoodsPropertiesItem {
        public string Name;
        public GameObject GoodsPrefab;
        public ItemTradeProperties TradingProperties;
        public UseableItemProperties UseableProperties;
        public int Damage;
        public float SelfMass;
        public float AdditionalMassWhenHooked;
        public bool DroppableFromBackpack;
        public bool CanAbsorbToBackpack;
        public bool CanBeAddedToInventory;
    }
    
    [CreateAssetMenu(fileName = "GoodsProperties")]
    public class GoodsProperties : ScriptableObject {
        public List<GoodsPropertiesItem> GoodsDatas;
    }


    public interface IGoodsConfigurationModel : IModel {
        GoodsPropertiesItem FindGoodsPropertiesByPrefabName(string name);
        List<GoodsPropertiesItem> GetAllGoodProperties();
    }


    public class GoodsPropertiesTable : Table<GoodsPropertiesItem> {
        public TableIndex<string, GoodsPropertiesItem> NameIndex { get; private set; }

        public GoodsPropertiesTable() {
            NameIndex = new TableIndex<string, GoodsPropertiesItem>(item => item.GoodsPrefab.name);
        }
        protected override void OnClear() {
            NameIndex.Clear();
        }

        public override void OnAdd(GoodsPropertiesItem item) { 
            NameIndex.Add(item);
        }

        public override void OnRemove(GoodsPropertiesItem item) {
            NameIndex.Remove(item);
        }
    }

    
    public class GoodsConfigurationModel : AbstractModel, IGoodsConfigurationModel {
        private ResLoader resLoader;
        private GoodsProperties goodsProperties;
        private GoodsPropertiesTable goodsPropertiesTable;
        
        protected override void OnInit() {

            goodsProperties = Resources.Load<GoodsProperties>("GoodsProperties");
            goodsPropertiesTable = new GoodsPropertiesTable();
          
            goodsPropertiesTable.Add(goodsProperties.GoodsDatas);
            

        }

        public GoodsPropertiesItem FindGoodsPropertiesByPrefabName(string name) {
            IEnumerable<GoodsPropertiesItem> results = goodsPropertiesTable.NameIndex.Get(name);
            if (results!=null && results.Any()) {

                return results.FirstOrDefault();
            }


            return null;
        }

        public List<GoodsPropertiesItem> GetAllGoodProperties() {
            return goodsPropertiesTable.Items;
        }
    }
}
