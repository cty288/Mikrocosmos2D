using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using MikroFramework.Event;
using UnityEngine;
using UnityEngine.UI;

namespace Mikrocosmos
{
    public class GameProgressBarViewController : AbstractMikroController<Mikrocosmos> {
        private GameProgressElement ongoingElement = null;
        private LayoutGroup layoutGroup;
        [SerializeField] private Color[] progressBarColors = new Color[3];
        [SerializeField] private GameObject gameProgressElement = null;
        private void Awake() {
            this.RegisterEvent<OnClientNextCountdown>(OnNewCountdownGenerated)
                .UnRegisterWhenGameObjectDestroyed(gameObject);
            layoutGroup = GetComponentInChildren<LayoutGroup>(true);
        }

        private void OnNewCountdownGenerated(OnClientNextCountdown e) {
            if (ongoingElement) {
                Color color = e.ShowAffinityForLastTime
                    ? (e.Team1Affinity >= 0.5f ? progressBarColors[1] : progressBarColors[2])
                    : progressBarColors[0];

                ongoingElement.EndProgress(color, () => {
                    ongoingElement = Instantiate(gameProgressElement, layoutGroup.transform).GetComponent<GameProgressElement>();
                    ongoingElement.transform.SetAsLastSibling();
                    ongoingElement.StartProgress(e.remainingTime);
                });
            }
            else {
                ongoingElement = Instantiate(gameProgressElement, layoutGroup.transform).GetComponent<GameProgressElement>();
                ongoingElement.transform.SetAsLastSibling();
                ongoingElement.StartProgress(e.remainingTime);
            }
        
        }
    }
}
