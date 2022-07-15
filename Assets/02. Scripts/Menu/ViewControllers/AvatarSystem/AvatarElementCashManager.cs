using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.ResKit;
using MikroFramework.Singletons;
using UnityEngine;

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

        public Sprite GetSpriteElementFromAsset(int assetIndex) {
            if (cashedSprites.ContainsKey(assetIndex)) {
                return cashedSprites[assetIndex];
            }
            Texture2D texture = resLoader.LoadSync<Texture2D>("profile", $"profile{assetIndex}");
            
            if (texture) {
                Sprite sprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f), 100.0f, 1, SpriteMeshType.FullRect);
                
                cashedSprites.Add(assetIndex, sprite);
                return sprite;
            }

            return null;
        }
    }
}
