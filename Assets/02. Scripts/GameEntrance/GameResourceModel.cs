using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MikroFramework.Architecture;
using MikroFramework.ResKit;
using MikroFramework.Utilities;
using Mirror;
using UnityEngine;
using Object = UnityEngine.Object;

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
            foreach (string itemBundle in allItemBundles)
            {
                AssetBundleRequest request = null;

                AssetBundle bundle = resLoader.LoadSync<AssetBundle>(itemBundle);
                if (bundle != null)
                {
                    request = bundle.LoadAllAssetsAsync<GameObject>();
                }


                while (request != null && !request.isDone && !ResManager.IsSimulationModeLogic)
                {
                    yield return null;
                }

                List<GameObject> allGameObjects = null;
#if UNITY_EDITOR
                if (ResManager.IsSimulationModeLogic)
                {
                    string[] assetPaths =
                        UnityEditor.AssetDatabase.GetAssetPathsFromAssetBundle(itemBundle);

                    if (assetPaths.Length > 0)
                    {
                        List<Object> allAssets = assetPaths.Select((o) => {
                            return UnityEditor.AssetDatabase.LoadAssetAtPath<Object>(o);
                        }).ToList();


                        allGameObjects = allAssets.Where((o => o is GameObject)).Select((o => o as GameObject))
                            .ToList();
                    }
                }
#endif
                if (!ResManager.IsSimulationModeLogic)
                {
                    allGameObjects = request.allAssets.Select((o => o as GameObject)).ToList();
                }


                result.AddRange((allGameObjects.Where((o => o.GetComponent<NetworkIdentity>() != null))).Select((o =>
                    resLoader.LoadSync<GameObject>(itemBundle, o.name))));
            }

            onFinished?.Invoke(result);
        }

        private IEnumerator LoadPlanetPrefabs(Action<List<GameObject>> onFinished) {
            List<GameObject> result = new List<GameObject>();
            foreach (string planetBundle in allPlanetBundles) {
                AssetBundleRequest request = null;

                AssetBundle bundle = resLoader.LoadSync<AssetBundle>(planetBundle);
                if (bundle != null) {
                    request = bundle.LoadAllAssetsAsync<GameObject>();
                }
               

                while (request!=null && !request.isDone && !ResManager.IsSimulationModeLogic) {
                    yield return null;
                }

                List<GameObject> allGameObjects = null;
#if UNITY_EDITOR
                if (ResManager.IsSimulationModeLogic) {
                    string[] assetPaths =
                        UnityEditor.AssetDatabase.GetAssetPathsFromAssetBundle(planetBundle);

                    if (assetPaths.Length > 0) {
                        List<Object> allAssets = assetPaths.Select((o) => {
                            return UnityEditor.AssetDatabase.LoadAssetAtPath<Object>(o);
                        }).ToList();


                        allGameObjects = allAssets.Where((o => o is GameObject)).Select((o => o as GameObject))
                            .ToList();
                    }
                }
#endif
                if (!ResManager.IsSimulationModeLogic) {
                    allGameObjects = request.allAssets.Select((o => o as GameObject)).ToList();
                }

                result.AddRange((allGameObjects.Where((o => o.GetComponent<NetworkIdentity>() != null))).Select((o =>
                    resLoader.LoadSync<GameObject>(planetBundle, o.name))));
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
