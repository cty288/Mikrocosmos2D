using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using MikroFramework.Event;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Mikrocosmos
{
    public class VisionOcclusionDebuffViewController : AbstractMikroController<Mikrocosmos> {
        private Animator animator;


        [SerializeField] private List<GameObject> smallOcclusionPrefabs; 
        private List<Transform> smallOcclusionSpawnPositins;

        private List<Animator> smallOcclusionAnimators;
        private void Awake() {
            this.RegisterEvent<OnClientVisionOcculsionBuff>(OnVisionOcclusionDebuff)
                .UnRegisterWhenGameObjectDestroyed(gameObject);

            animator = GetComponent<Animator>();

            smallOcclusionSpawnPositins = new List<Transform>();
            smallOcclusionAnimators = new List<Animator>();
        }

        private void Start() {
            Transform smallOcclusionParent = transform.Find("SmallOcculasionSpawnPositions");
            for (int i = 0; i < smallOcclusionParent.childCount; i++) {
                Transform child = smallOcclusionParent.GetChild(i);
                smallOcclusionSpawnPositins.Add(child);
            }
            
         
        }

        private void OnVisionOcclusionDebuff(OnClientVisionOcculsionBuff e) {
            if (e.status == BuffStatus.OnStart) {
                animator.SetTrigger("Start");
                smallOcclusionSpawnPositins.Shuffle();
                
            }
            else if (e.status == BuffStatus.OnEnd) {
                animator.SetTrigger("End");
                foreach (Animator smallOcclusionAnimator in smallOcclusionAnimators) {
                    smallOcclusionAnimator.SetTrigger("End");
                }
                smallOcclusionAnimators.Clear();
            }
        }

        public void OnSmallOcclusionStartSpawn() {
            StartCoroutine(SpawnSmallOcclusions());
            
        }

        private IEnumerator SpawnSmallOcclusions() {
            for (int i = 0; i < smallOcclusionPrefabs.Count; i++) {
                GameObject smallOcclusionInstance = Instantiate(smallOcclusionPrefabs[i], smallOcclusionSpawnPositins[i]);
                smallOcclusionAnimators.Add(smallOcclusionInstance.GetComponent<Animator>());
                yield return new WaitForSeconds(Random.Range(0.05f, 0.2f));
            }
        }
    }
}
