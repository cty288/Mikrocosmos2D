using System.Collections;
using System.Collections.Generic;
using GoogleSheetsToUnity;
using MikroFramework.Architecture;
using MikroFramework.TimeSystem;
using Mirror.FizzySteam;
using UnityEngine;

namespace Mikrocosmos
{
    public class Mikrocosmos : NetworkedArchitecture<Mikrocosmos> {
        protected override void Init() {
            this.RegisterModel<ILocalPlayerInfoModel>(new LocalPlayerInfoModel());
            this.RegisterModel<IClientAvatarModel>(new ClientAvatarModel());
            this.RegisterModel<IGoodsConfigurationModel>(new GoodsConfigurationModel());
            // this.RegisterModel<ISpaceshipConfigurationModel>(new SpaceshipConfigurationModel());
            // this.RegisterModel<ISpaceshipModel>(new SpaceshipModel());
            this.RegisterSystem<ITimeSystem>(new TimeSystem());
            this.RegisterSystem<IClientInfoSystem>(new ClientInfoSystem());
            

        }
#if UNITY_EDITOR
        [UnityEditor.MenuItem("Mikrocosmos/UpdateGoodSheetByGoodsProperties", false, 1)]
        public static void UploadGoodsPropertities()
        {
            var goodsProperties = Resources.Load<GoodsProperties>("GoodsProperties");
            List<List<string>> combined = new List<List<string>>();
            foreach (GoodsPropertiesItem goodsData in goodsProperties.GoodsDatas)
            {
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
                new GSTU_Search("1Y11EVzCozMt-bg4p36o83lepK25EZ8U7hZdNsabb1Vk", "ItemsConfig", "A3"),
                new ValueRange(combined), null);
        }

#endif


        protected override void SeverInit() {
            
        }

        protected override void ClientInit() {
           
        }
    }
}
