using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.ResKit;
using MikroFramework.Singletons;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Mikrocosmos
{
    public class ImageEffectController : MonoMikroSingleton<ImageEffectController> {
        private ResLoader resLoader;
        private Renderer2DData render2DData;
        private void Awake() {
            ResLoader.Create((loader => resLoader = loader));
            StartCoroutine(LoadRenderData());
        }

        private IEnumerator LoadRenderData() {
            while (resLoader==null || !resLoader.IsReady) {
                yield return null;
            }
            render2DData = resLoader.LoadSync<Renderer2DData>("resources://MainRendererData");
        }

        public Material GetScriptableRendererFeatureMaterial(int index) {
            CustomRenderPassFeature feature = render2DData.rendererFeatures[index] as CustomRenderPassFeature;
            return feature.settings.material;
        }
        public Material TurnOnScriptableRendererFeature(int index) {
            CustomRenderPassFeature feature = render2DData.rendererFeatures[index] as CustomRenderPassFeature;
            feature.SetActive(true);
            return feature.settings.material;
        }

        public void TurnOffScriptableRendererFeature(int index) {
            render2DData.rendererFeatures[index].SetActive(false);
        }

        
        protected override void OnApplicationQuit() {
            var features = render2DData.rendererFeatures;
            foreach (ScriptableRendererFeature feature in features) {
                feature.SetActive(false);
            }
            base.OnApplicationQuit();
        }
    }
}
