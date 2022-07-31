using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.ResKit;
using MikroFramework.Singletons;
using Polyglot;
using UnityEngine;
using UnityEngine.U2D;

namespace Mikrocosmos
{
    public  class DescriptionFactory : MonoMikroSingleton<DescriptionFactory> {

        private ResLoader resLoader;
        private void Awake() {
            ResLoader.Create((loader => resLoader = loader));
        }

        public DescriptionItem GetGoodsDescriptionItem(string prefabAssetName, GoodsRarity rarity, string goodsName, string description, string hintGameObjectPrefabAssetName) {
            GameObject spawnedDescriptionPrefab = null;
           
            spawnedDescriptionPrefab = resLoader.LoadSync<GameObject>("description", prefabAssetName);
           
            GameObject descriptionLayout = DescriptionLayoutFinder.GetLayout();
            if (descriptionLayout)
            {
                SpriteAtlas atlas = resLoader.LoadSync<SpriteAtlas>("assets/goods_inventory", $"ItemInventoryAtlas");
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
                   
                    return descriptionItem;
                }

                return null;

            }
            return null;
        }
    }
}
