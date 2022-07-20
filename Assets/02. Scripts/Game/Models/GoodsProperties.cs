using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GoogleSheetsToUnity;
using MikroFramework.Architecture;
using MikroFramework.DataStructures;
using MikroFramework.ResKit;
using Polyglot;
using UnityEngine;
#if UNITY_EDITOR
#endif
namespace Mikrocosmos
{

    public struct OnGoodsPropertiesUpdated {
        public List<GoodsPropertiesItem> allItemProperties;
    }
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

            /*
            List<List<string>> combined = new List<List<string>>();
            foreach (GoodsPropertiesItem goodsData in goodsProperties.GoodsDatas) {
                List<string> entry = new List<string>() {
                    goodsData.Name.ToString(),
                    goodsData.TradingProperties.BasicBuyPrice.ToString(),
                    goodsData.TradingProperties.BasicSellPrice.ToString(),
                    ((int) goodsData.TradingProperties.Rarity).ToString(),
                    (goodsData.UseableProperties.CanBeUsed).ToString(),
                    ((int) goodsData.UseableProperties.UseMode).ToString(),
                    goodsData.UseableProperties.UseFrequency.ToString(),
                    goodsData.UseableProperties.MaxDurability.ToString(),
                    goodsData.Damage.ToString(),
                    goodsData.SelfMass.ToString(),
                    goodsData.AdditionalMassWhenHooked.ToString(),
                    goodsData.DroppableFromBackpack.ToString(),
                    goodsData.CanAbsorbToBackpack.ToString(),
                    goodsData.CanBeAddedToInventory.ToString(),
                };
                combined.Add(entry);
            }

            SpreadsheetManager.Write(
                new GSTU_Search("1Y11EVzCozMt-bg4p36o83lepK25EZ8U7hZdNsabb1Vk", "ItemsConfig", "A2"),
                new ValueRange(combined), null);*/
            //SpreadsheetManager.Read(new GSTU_Search("1Y11EVzCozMt-bg4p36o83lepK25EZ8U7hZdNsabb1Vk", "ItemsConfig"),
                //HotUpdateItemConfig);

            CoroutineRunner.Singleton.RunCoroutine(GoogleDownload.DownloadSheet(
                "1Y11EVzCozMt-bg4p36o83lepK25EZ8U7hZdNsabb1Vk", "1586537549", OnDownloadSheet,
                GoogleDriveDownloadFormat.CSV));
        }

        private void OnDownloadSheet(string text) {
            if (text != null) {
                List<List<string>> rows;
                text = text.Replace("\r\n", "\n");

                rows = CsvReader.Parse(text);

                Dictionary<string, List<string>> elements = new Dictionary<string, List<string>>();
                foreach (List<string> list in rows)
                {
                    elements.Add(list[0], list);
                }

                foreach (GoodsPropertiesItem goodsData in goodsProperties.GoodsDatas)
                {
                    var cells = elements[goodsData.Name];
                    goodsData.TradingProperties.BasicBuyPrice = int.Parse(cells[1]);
                    goodsData.TradingProperties.BasicSellPrice = int.Parse(cells[2]);
                    goodsData.TradingProperties.Rarity = (GoodsRarity)Enum.Parse(typeof(GoodsRarity), cells[3]);
                    goodsData.UseableProperties.CanBeUsed = bool.Parse(cells[4]);
                    goodsData.UseableProperties.UseMode = (ItemUseMode)Enum.Parse(typeof(ItemUseMode), cells[5]);
                    goodsData.UseableProperties.UseFrequency = float.Parse(cells[6]);
                    goodsData.UseableProperties.MaxDurability = int.Parse(cells[7]);
                    goodsData.Damage = int.Parse(cells[8]);
                    goodsData.SelfMass = float.Parse(cells[9]);
                    goodsData.AdditionalMassWhenHooked = float.Parse(cells[10]);
                    goodsData.DroppableFromBackpack = bool.Parse(cells[11]);
                    goodsData.CanAbsorbToBackpack = bool.Parse(cells[12]);
                    goodsData.CanBeAddedToInventory = bool.Parse(cells[13]);
                }

             
            }
            goodsPropertiesTable.Add(goodsProperties.GoodsDatas);
            this.SendEvent<OnGoodsPropertiesUpdated>(new OnGoodsPropertiesUpdated() {
                allItemProperties = goodsPropertiesTable.Items
            });
        }

       

        private void HotUpdateItemConfig(GstuSpreadSheet e) {


            foreach (GoodsPropertiesItem goodsData in goodsProperties.GoodsDatas) {
                var cells = e.rows[goodsData.Name];
                goodsData.TradingProperties.BasicBuyPrice = int.Parse(cells[1].value);
                goodsData.TradingProperties.BasicSellPrice = int.Parse(cells[2].value);
                goodsData.TradingProperties.Rarity = (GoodsRarity) Enum.Parse(typeof(GoodsRarity), cells[3].value);
                goodsData.UseableProperties.CanBeUsed = bool.Parse(cells[4].value);
                goodsData.UseableProperties.UseMode = (ItemUseMode)Enum.Parse(typeof(ItemUseMode), cells[5].value);
                goodsData.UseableProperties.UseFrequency = float.Parse(cells[6].value);
                goodsData.UseableProperties.MaxDurability = int.Parse(cells[7].value);
                goodsData.Damage = int.Parse(cells[8].value);
                goodsData.SelfMass = float.Parse(cells[9].value);
                goodsData.AdditionalMassWhenHooked = float.Parse(cells[10].value);
                goodsData.DroppableFromBackpack = bool.Parse(cells[11].value);
                goodsData.CanAbsorbToBackpack = bool.Parse(cells[12].value);
                goodsData.CanBeAddedToInventory = bool.Parse(cells[13].value);
            }

            goodsPropertiesTable.Add(goodsProperties.GoodsDatas);
        }

        public GoodsPropertiesItem FindGoodsPropertiesByPrefabName(string name) {
            IEnumerable<GoodsPropertiesItem> results = goodsPropertiesTable.NameIndex.Get(name);
            if (results!=null && results.Any()) {

                return results.FirstOrDefault();
            }


            return null;
        }

     
    }
}
