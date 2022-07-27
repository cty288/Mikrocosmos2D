using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using MikroFramework.Event;
using MikroFramework.ResKit;
using UnityEngine;
using UnityEngine.U2D;

namespace Mikrocosmos
{
    public class GameInfoPanel : AbstractMikroController<Mikrocosmos> {
        [SerializeField] private List<GameObject> InfoElementPrefabs;

        private Transform layoutGroup;

        private Dictionary<string, InfoElement> infoNameToElement = new Dictionary<string, InfoElement>();
        private ResLoader resLoader;
        
        
        private void Awake() {
            this.RegisterEvent<OnInfoStartOrUpdate>(OnInfoStartOrUpdate).UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<OnInfoStop>(OnInfoStop).UnRegisterWhenGameObjectDestroyed(gameObject);
            layoutGroup = transform.Find("List/LayoutGroup");
            ResLoader.Create((loader => resLoader = loader ));
        }

        private void OnDestroy() {
            resLoader.ReleaseAllAssets();
        }

        private void OnInfoStop(OnInfoStop e) {
            if (infoNameToElement.ContainsKey(e.InfoName)) {
                InfoElement infoElement = infoNameToElement[e.InfoName];
                if (infoElement) {
                    infoElement.StopInfo();
                }
                infoNameToElement.Remove(e.InfoName);
            }
        }

        private void OnInfoStartOrUpdate(OnInfoStartOrUpdate e) {
            Sprite iconSprite = null;
            Sprite infoContainerSpriteName = null;
            Sprite infoSliderSprite = null;
            SpriteAtlas atlas = resLoader.LoadSync<SpriteAtlas>("info", $"InfoSpriteAtlas");
        
            if (!string.IsNullOrEmpty(e.Info.InfoElementIconAssetName)) {
                iconSprite = atlas.GetSprite($"{e.Info.InfoElementIconAssetName}");
            }

            if (!string.IsNullOrEmpty(e.Info.InfoContainerSpriteAssetName)) {
                infoContainerSpriteName = atlas.GetSprite($"{e.Info.InfoContainerSpriteAssetName}");
            }

            if (!string.IsNullOrEmpty(e.Info.InfoContainerSliderAssetName)) {
                infoSliderSprite = atlas.GetSprite($"{e.Info.InfoContainerSliderAssetName}");
            }

            if (!infoNameToElement.ContainsKey(e.Info.Name)) {

                // GameObject elementPrefab = resLoader.LoadSync<GameObject>("info_elements", e.Info.InfoElementPrefabAssetName);
                GameObject elementPrefab =
                    resLoader.LoadSync<GameObject>($"resources://{e.Info.InfoElementPrefabAssetName}");
                
                InfoElement spawnedInfoElement =  Instantiate(elementPrefab, layoutGroup)
                    .GetComponent<InfoElement>();
                spawnedInfoElement.transform.SetAsFirstSibling();

                this.GetSystem<IAudioSystem>().PlaySound("InfoUpcoming", SoundType.Sound2D);

                infoNameToElement.Add(e.Info.Name, spawnedInfoElement);
                spawnedInfoElement.SetInfo(new Info() {
                    AutoDestroyWhenTimeUp =e.Info.AutoDestroyWhenTimeUp,
                    Description =  e.Info.Description,
                    InfoIconSprite = iconSprite,
                    InfoContainerSprite = infoContainerSpriteName,
                    Name = e.Info.Name,
                    RemainingTime = e.Info.RemainingTime,
                    ShowRemainingTime = e.Info.ShowRemainingTime,
                    Title = e.Info.Title,
                    InfoSliderSprite = infoSliderSprite
                }, false);
            }
            else {
                InfoElement infoElement = infoNameToElement[e.Info.Name];
                if (infoElement) {
                    infoElement.SetInfo(new Info()
                    {
                        AutoDestroyWhenTimeUp = e.Info.AutoDestroyWhenTimeUp,
                        Description = e.Info.Description,
                        InfoIconSprite = iconSprite,
                        Name = e.Info.Name,
                        RemainingTime = e.Info.RemainingTime,
                        ShowRemainingTime = e.Info.ShowRemainingTime,
                        InfoContainerSprite = infoContainerSpriteName,
                        Title = e.Info.Title,
                        InfoSliderSprite = infoSliderSprite                        
                    }, true);
                }
                else {
                    infoNameToElement.Remove(e.Info.Name);
                }
            }
        }

        public void InfoElementSelfDestroy(string name) {
            if (infoNameToElement.ContainsKey(name)) {
                InfoElement infoElement = infoNameToElement[name];
                if (infoElement) {
                    infoElement.StopInfo();
                }
                infoNameToElement.Remove(name);
            }
        }

      
    }
}
