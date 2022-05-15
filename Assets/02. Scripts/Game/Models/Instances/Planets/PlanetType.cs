using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using UnityEngine;

namespace Mikrocosmos
{
    public enum PlanetTypeEnum {
        GoldPlanetType,
        WoodPlanetType,
        WaterPlanetType,
        FirePlanetType,
        SoilPlanetType
    }

    public interface IPlanetTypeConfigModel : IModel {
        PlanetType GetPlanetType(PlanetTypeEnum planetType);
    }

    public class PlanetTypeConfigModel : AbstractModel, IPlanetTypeConfigModel, ICanGetModel {
        private Dictionary<PlanetTypeEnum, PlanetType> planets;
        protected override void OnInit() {
            planets = new Dictionary<PlanetTypeEnum, PlanetType>();
            RegisterPlanetTypes();
        }

        private void RegisterPlanetTypes() {
            
        }

       
        public PlanetType GetPlanetType(PlanetTypeEnum planetType) {
            return planets[planetType];
        }
    }
    public  class PlanetType {
        public  PlanetTypeEnum Type { get; }
        public  List<GoodsConfigure> TypicalBuyItemList { get; }
        public  List<GoodsConfigure> TypicalSellItemList { get; }

        public PlanetType(PlanetTypeEnum type, List<GoodsConfigure> typicalBuyItemList, List<GoodsConfigure>
            typicalSellItemList) {
            this.Type = type;
            this.TypicalSellItemList = typicalBuyItemList;
            this.TypicalSellItemList = typicalSellItemList;
        }
    }
}
