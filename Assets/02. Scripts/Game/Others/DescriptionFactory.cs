using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using MikroFramework.Event;
using MikroFramework.ResKit;
using MikroFramework.Singletons;
using Polyglot;
using UnityEngine;
using UnityEngine.U2D;

namespace Mikrocosmos
{
    public  class DescriptionFactory : MonoMikroSingleton<DescriptionFactory>, IController {

        private ResLoader resLoader;
        protected string lastGoodsName = "";
        private SpriteAtlas atlas;
        private void Awake() {
            ResLoader.Create((loader => resLoader = loader));
            this.RegisterEvent<OnDescriptionRemoved>(OnDescriptionRemoved).UnRegisterWhenGameObjectDestroyed(gameObject);
            atlas = resLoader.LoadSync<SpriteAtlas>("assets/goods_inventory", $"ItemInventoryAtlas");
            resLoader.LoadSync<GameObject>("description", "DescriptionPanel_General");
            resLoader.LoadSync<GameObject>("description", "DescriptionPanel_Raw");
        }
        
        private void Start() {
            
        }

        private void OnDescriptionRemoved(OnDescriptionRemoved e) {
            if (e.Type == DescriptionType.Item) {
                lastGoodsName = "";
            }
        }

        protected override void OnBeforeDestroy() {
            base.OnBeforeDestroy();
            resLoader.ReleaseAllAssets();
        }

        public DescriptionItem GetGoodsDescriptionItem(string prefabAssetName, GoodsRarity rarity, string goodsName, string description, string hintGameObjectPrefabAssetName) {
            if (lastGoodsName == goodsName) {
                return null;
            }
            GameObject spawnedDescriptionPrefab = null;
           
            spawnedDescriptionPrefab = resLoader.LoadSync<GameObject>("description", prefabAssetName);
           
            GameObject descriptionLayout = DescriptionLayoutFinder.GetLayout();
            if (descriptionLayout)
            {
              
                Sprite sprite = atlas.GetSprite($"{goodsName}Sprite");
                string nameLocalized = Localization.Get($"NAME_{goodsName}");
                string descriptionLocalized = description;

                if (sprite != null && nameLocalized != Localization.KeyNotFound &&
                    descriptionLocalized != Localization.KeyNotFound)
                {
                    DefaultGoodsDescriptionPanel descriptionPanel = GameObject
                        .Instantiate(spawnedDescriptionPrefab, descriptionLayout.transform).GetComponent<DefaultGoodsDescriptionPanel>();

                    GameObject hintGameObject = null;
                    if (!String.IsNullOrEmpty(hintGameObjectPrefabAssetName)) {
                        hintGameObject = resLoader.LoadSync<GameObject>("description", hintGameObjectPrefabAssetName);
                    }

                    descriptionPanel.SetInfo(sprite, nameLocalized, descriptionLocalized, hintGameObject);
                    DescriptionItem descriptionItem = new DescriptionItem()
                        { DescriptionType = DescriptionType.Item, SpawnedDescriptionPanel = descriptionPanel };
                    lastGoodsName = goodsName;
                    return descriptionItem;
                    
                }

                return null;

            }
            return null;
        }

        public IArchitecture GetArchitecture() {
            return Mikrocosmos.Interface;
        }
    }
}
