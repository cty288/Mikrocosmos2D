using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MikroFramework.Architecture;
using MikroFramework.ResKit;
using UnityEngine;

namespace Mikrocosmos
{
    public interface IGameResourceModel : IModel {
        void LoadNecessaryResources(Action onFinished);

        List<GameObject> GetAllPlanetPrefabs();
        List<GameObject> GetAllItemPrefabs();
    }
    public class GameResourceModel : AbstractModel, IGameResourceModel {
        private ResLoader resLoader;
        private List<AssetBundleData> allLoadedBundles;
        private List<string> allPlanetBundles;
        private List<string> allItemBundles;

        private List<GameObject> allPlanetPrefabs;
        private List<GameObject> allItemPrefabs;
        protected override void OnInit() {
            ResLoader.Create((loader => resLoader = loader));
        }

        public void LoadNecessaryResources(Action onFinished) {
            CoroutineRunner.Singleton.StartCoroutine(LoadResLoader((() => {
                allLoadedBundles = ResData.Singleton.AssetBundleDatas;
                allPlanetBundles = allLoadedBundles.FindAll((data => data.Name.StartsWith("assets/planets")))
                    .Select((data => data.Name)).ToList();
                allItemBundles = allLoadedBundles.FindAll((data => data.Name.StartsWith("assets/goods")))
                    .Select((data => data.Name)).ToList();

                CoroutineRunner.Singleton.RunCoroutine(LoadPlanetPrefabs((list => {
                    allPlanetPrefabs = list;
                    CoroutineRunner.Singleton.RunCoroutine(LoadItemPrefabs((items => {
                        allItemPrefabs = items;
                        onFinished?.Invoke();
                    })));
                })));

            })));
        }

        private IEnumerator LoadItemPrefabs(Action<List<GameObject>> onFinished) {
            List<GameObject> result = new List<GameObject>();
            foreach (string itemBundle in allItemBundles) {
                AssetBundle bundle = resLoader.LoadSync<AssetBundle>(itemBundle);
                AssetBundleRequest request = bundle.LoadAllAssetsAsync<GameObject>();
                while (!request.isDone) {
                    yield return null;
                }

                List<GameObject> allGameObjects = request.allAssets.Select((o => o as GameObject)).ToList();
                result.AddRange(allGameObjects.Where((o => o.GetComponent<IGoods>() != null)));
            }

            onFinished?.Invoke(result);
        }

        private IEnumerator LoadPlanetPrefabs(Action<List<GameObject>> onFinished) {
            List<GameObject> result = new List<GameObject>();
            foreach (string planetBundle in allPlanetBundles) {
                AssetBundle bundle = resLoader.LoadSync<AssetBundle>(planetBundle);
                AssetBundleRequest request = bundle.LoadAllAssetsAsync<GameObject>();
                while (!request.isDone) {
                    yield return null;
                }

                List<GameObject> allGameObjects = request.allAssets.Select((o => o as GameObject)).ToList();
                result.AddRange(allGameObjects.Where((o => o.GetComponent<IPlanetModel>() != null)));
            }

            onFinished?.Invoke(result);
        }

        public List<GameObject> GetAllPlanetPrefabs() {
            return allPlanetPrefabs;
        }

        public List<GameObject> GetAllItemPrefabs() {
            return allItemPrefabs;
        }

        public List<string> GetAllPlanetBundles() {
            return allPlanetBundles;
        }

        public List<string> GetAllItemBundles() {
            return allItemBundles;
        }

        private IEnumerator LoadResLoader(Action onFinished) {
            while (resLoader==null || !resLoader.IsReady) {
                yield return null;
            }
            onFinished?.Invoke();
        }
    }
}
