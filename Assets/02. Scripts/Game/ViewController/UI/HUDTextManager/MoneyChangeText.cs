using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MikroFramework.ResKit;
using UnityEngine;
using UnityEngine.UI;

namespace Mikrocosmos
{
    public class MoneyChangeText : MonoBehaviour {
        private DefaultPoolableGameObject pool;
        private Text text;

        [SerializeField] private float duration = 0.3f;

        private void Awake() {
            pool = GetComponent<DefaultPoolableGameObject>();
            text = GetComponentInChildren<Text>();
        }
        
        public void StartAnimate(int money) {
            Color minBlue = new Color(0.2836477f, 0.653459f, 1f);
            Color maxBlue = new Color(0, 0.5987263f, 1f);
            float minRed = 0.6666666f;


            Color targetColor = minBlue;
            float maxMoney = 100;
            int moneyAbs = Mathf.Abs(money);

            if (money > 0) {
                targetColor = Color.Lerp(minBlue, maxBlue, moneyAbs / maxMoney);
            }
            else {
                targetColor = new Color(1f, minRed - minRed * (moneyAbs / maxMoney),
                    minRed - minRed * (moneyAbs / maxMoney), 1);
            }
          
            

            float targetSize = Mathf.Min(((moneyAbs/maxMoney) * moneyAbs + 1f), 2f);
            Vector3 moveDir = new Vector3(Random.Range(-0.5f,0.5f), Random.Range(2f, 4f), 0);
            Vector2 targetPos = transform.position + moveDir * (money >= 0 ? 1 : -1);
            float targetTime = duration * (-0.5f * Mathf.Clamp((moneyAbs / maxMoney), 0, 1f) + 1);
            string targetText = money > 0 ? $"+${money}" : $"-${money}";
            text.text = targetText;
            text.color = targetColor;
            text.DOFade(0, targetTime).OnComplete(() => {
                pool.RecycleToCache();
            });

            transform.localScale = new Vector3(targetSize, targetSize, targetSize);
            transform.DOScale(new Vector3(0.5f, 0.5f, 0.5f), targetTime);

            transform.DOMove(targetPos, targetTime);
        }

        public void OnRecycled()
        {
            text.color = Color.white;
        }
    }
}
