using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MikroFramework.Architecture;
using MikroFramework.ResKit;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Mikrocosmos
{
    public class AvatarElementTypeLayout : AbstractMikroController<Mikrocosmos>, ICanSendEvent {
        [SerializeField] private Vector2Int elementIndexRange;
        [SerializeField] private float elementLayoutPageWidth = 1260;
        [SerializeField] private GameObject avatarSelectionElementPrefab;
        [SerializeField] private int elementLayer;
        public Vector2Int ElementIndexRange => elementIndexRange;

        private List<AvatarSingleElement> allElements = new List<AvatarSingleElement>();

        private Transform elementLayout;
        private Button nextPageButton;
        private Button lastPageButton;

       
        private int selectedIndex = -1;

        [SerializeField] private int itemPerPage = 6;
        private int currentPage;
        private int maxPage;
        private void Awake() {
            elementLayout = transform.Find("Mask/ElementLayout");
            nextPageButton = transform.Find("NextPageButton").GetComponent<Button>();
            lastPageButton = transform.Find("LastPageButton").GetComponent<Button>();
        }

        private void Start() {
            nextPageButton.onClick.AddListener(OnNextPageClicked);
            lastPageButton.onClick.AddListener(OnLastPageClicked);
        }

        private void OnLastPageClicked() {
            TurnToPage(currentPage-1);
        }

        private void OnNextPageClicked() {
            TurnToPage(currentPage + 1);
        }

        public void FillElements(Action onFinished) {
            if (allElements.Count > 0) {
                return;
            }
            StartCoroutine(FillElementsUntilResLoaderReady(onFinished));
            
        }

        public void RandomSelect() {
            SelectElement(allElements[Random.Range(0, allElements.Count)].AssetIndex,true);
        }
        public void SelectElement(int index, bool refreshShowcase) {
            if (index >= elementIndexRange.y || index < elementIndexRange.x || index == selectedIndex) {
                return;
            }

            this.SendEvent<OnAvatarSingleElementSelected>(new OnAvatarSingleElementSelected() {
                Index = index,
                ReplacedIndex = selectedIndex,
                IsBase = elementIndexRange.y <= 100,
                ElementLayer = elementLayer,
                RefreshShowcase = refreshShowcase
            });

            allElements[index - elementIndexRange.x].SetSelection(true);
            if (selectedIndex >= elementIndexRange.x && selectedIndex < elementIndexRange.y) {
                allElements[selectedIndex - elementIndexRange.x].SetSelection(false);
            }
            selectedIndex = index;
            TurnToPage((index - ElementIndexRange.x) / itemPerPage);
        }

        private void TurnToPage(int page) {
            if (page < 0 || page >= maxPage) {
                return;
            }
            currentPage = page;
            elementLayout.GetComponent<RectTransform>()
                .DOAnchorPos(new Vector2(-elementLayoutPageWidth * currentPage, 0), 0.3f);

            nextPageButton.gameObject.SetActive( true);
            lastPageButton.gameObject.SetActive(true);
            if (currentPage == maxPage - 1) {
                nextPageButton.gameObject.SetActive(false);
            }

            if (currentPage == 0) {
                lastPageButton.gameObject.SetActive(false);
            }
        }

        private IEnumerator FillElementsUntilResLoaderReady(Action onFinished) {
            while (true) {
                yield return null;
                if (AvatarElementCashManager.Singleton.IsReady) {
                    break;
                }
            }
            for (int i = elementIndexRange.x; i < elementIndexRange.y; i++) {
                Sprite sprite = AvatarElementCashManager.Singleton.GetSpriteElementFromAsset(i);
                if (sprite)
                {
                    GameObject selectionPrefab = Instantiate(avatarSelectionElementPrefab, elementLayout);
                    AvatarSingleElement element = selectionPrefab.GetComponent<AvatarSingleElement>();
                    element.SetElement(i, sprite); 
                    allElements.Add(element);
                }
            }
            maxPage = Mathf.CeilToInt(allElements.Count / (float)itemPerPage);
            onFinished?.Invoke();

           
        }
        
    }
}
