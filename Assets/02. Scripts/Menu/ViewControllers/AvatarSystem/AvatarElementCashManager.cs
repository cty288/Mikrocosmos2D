using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.ResKit;
using MikroFramework.Singletons;
using UnityEngine;
using UnityEngine.U2D;

namespace Mikrocosmos
{
    public class AvatarElementCashManager : MonoPersistentMikroSingleton<AvatarElementCashManager> {
        private ResLoader resLoader;
        public bool IsReady = false;

        private Dictionary<int, Sprite> cashedSprites = new Dictionary<int, Sprite>();
        private void Start() {
            ResLoader.Create(loader => {
                resLoader = loader;
                IsReady = true;
            });
        }

        protected override void OnBeforeDestroy() {
            base.OnBeforeDestroy();
            resLoader.ReleaseAllAssets();
        }

        public Sprite GetSpriteElementFromAsset(int assetIndex) {
            if (cashedSprites.ContainsKey(assetIndex)) {
                if (!cashedSprites[assetIndex]) {
                    cashedSprites.Clear();
                }
                else {
                    return cashedSprites[assetIndex];
                }
            }
            SpriteAtlas atlas = resLoader.LoadSync<SpriteAtlas>("profile", $"AvatarAtlas");
            Sprite sprite = atlas.GetSprite($"profile{assetIndex}");
            //sprite.packingMode = SpritePackingMode.Rectangle;
            if (sprite)
            {
                Debug.Log($"Packing Mode: {sprite.packingMode}");
                cashedSprites.Add(assetIndex, sprite);
                return sprite;
            }

            return null;
        }
    }
}
