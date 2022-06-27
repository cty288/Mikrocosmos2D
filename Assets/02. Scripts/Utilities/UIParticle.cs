using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mikrocosmos
{
   // [ExecuteInEditMode]
    public class UIParticle : MonoBehaviour
    {
        private List<ScaleData> scaleDatas = null;
        void Awake()
        {
            scaleDatas = new List<ScaleData>();
            foreach (ParticleSystem p in transform.GetComponentsInChildren<ParticleSystem>(true))
            {
                scaleDatas.Add(new ScaleData() { transform = p.transform, beginScale = p.transform.localScale });
            }
        }

        void Start()
        {
            float designWidth = 1920;
            float designHeight = 1080;
            float designScale = designWidth / designHeight;
            float scaleRate = (float)Screen.width / (float)Screen.height;

            foreach (ScaleData scale in scaleDatas)
            {
                if (scale.transform != null)
                {
                    if (scaleRate < designScale)
                    {
                        float scaleFactor = scaleRate / designScale;
                        scale.transform.localScale = scale.beginScale * scaleFactor;
                    }
                    else
                    {
                        scale.transform.localScale = scale.beginScale;
                    }
                }
            }
        }

        private float lastwidth = 0f;
        private float lastheight = 0f;

        void Update () {
            if (lastwidth != Screen.width || lastheight != Screen.height)
            {
                lastwidth = Screen.width;
                lastheight = Screen.height;
                Start();
            }
           
        }

        class ScaleData
        {
            public Transform transform;
            public Vector3 beginScale = Vector3.one;
        }
    }
}
