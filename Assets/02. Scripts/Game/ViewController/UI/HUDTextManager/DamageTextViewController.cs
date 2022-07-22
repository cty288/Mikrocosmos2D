using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MikroFramework.ResKit;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Mikrocosmos
{
    public class DamageTextViewController : MonoBehaviour {
        private DefaultPoolableGameObject pool;
        private Text text;

        [SerializeField] private float duration = 0.3f;
        
        private void Awake() {
            pool = GetComponent<DefaultPoolableGameObject>();
            text = GetComponentInChildren<Text>();
        }

        public void StartAnimate(int damage) {
            Color targetColor = new Color(0.6226415f, 0.6226415f, 0.6226415f,1);
            float maxDamage = 30f;
            if (damage > 0) {
                targetColor = new Color(1, 1f - damage / 20f, 1f- damage / 20f,1);
            }

            float targetSize = Mathf.Min((0.069f * damage + 0.93f), 3f);
            Vector2 targetPos = transform.localPosition + new Vector3(Random.Range(-0.5f,0.5f), Random.Range(2f, 4f), 0);
            float targetTime = duration * (-0.5f * Mathf.Clamp((damage / maxDamage),0,1f) + 1);
            text.text = damage.ToString();
            text.color = targetColor;
            text.DOFade(0, targetTime).OnComplete(() => {
                pool.RecycleToCache();
            });

            transform.localScale = new Vector3(targetSize, targetSize, targetSize);
            transform.DOScale(new Vector3(0.3f, 0.3f, 0.3f), targetTime);

            transform.DOLocalMove(targetPos, targetTime);
        }
        
        public void OnRecycled() {
            text.color = Color.white;
        }
    }
}
