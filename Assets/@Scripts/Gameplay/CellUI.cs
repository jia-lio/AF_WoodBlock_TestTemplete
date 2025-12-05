using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace WoodBlock.Gameplay
{
    /// <summary>
    /// 게임 보드의 개별 셀 (UI Image 버전)
    /// </summary>
    public class CellUI : MonoBehaviour
    {
        [Header("셀 정보")]
        public int gridX;
        public int gridY;
        public bool isOccupied = false;

        [Header("비주얼")]
        public Image cellImage;
        public Image blockImage;
        public GameObject matchEffect;
        public GameObject clearEffect;

        [Header("색상")]
        public Color normalColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        public Color highlightColor = new Color(1f, 1f, 0.7f, 1f);
        public Color occupiedColor = new Color(0.6f, 0.4f, 0.2f, 1f);
        public Color matchColor = new Color(1f, 0.8f, 0.3f, 1f);

        private void Awake()
        {
            if (cellImage == null)
                cellImage = GetComponent<Image>();

            if (blockImage == null)
            {
                GameObject blockObj = new GameObject("Block");
                blockObj.transform.SetParent(transform);

                RectTransform rt = blockObj.AddComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.sizeDelta = Vector2.zero;
                rt.anchoredPosition = Vector2.zero;

                blockImage = blockObj.AddComponent<Image>();
                blockImage.raycastTarget = false;
                blockImage.enabled = false;
            }
        }

        public void Initialize(int x, int y)
        {
            gridX = x;
            gridY = y;
            isOccupied = false;
            SetNormalState();
        }

        public void SetNormalState()
        {
            if (cellImage != null)
                cellImage.color = normalColor;
        }

        public void SetHighlightState(bool canPlace)
        {
            if (cellImage != null)
                cellImage.color = canPlace ? highlightColor : Color.red;
        }

        public void PlaceBlock(Sprite blockSprite)
        {
            isOccupied = true;
            blockImage.sprite = blockSprite;
            blockImage.enabled = true;
            blockImage.color = occupiedColor;

            // 배치 애니메이션
            blockImage.transform.localScale = Vector3.zero;
            blockImage.transform.DOScale(Vector3.one, Constants.BLOCK_PLACE_DURATION)
                .SetEase(Ease.OutBack);
        }

        public void RemoveBlock()
        {
            isOccupied = false;
            blockImage.enabled = false;
            blockImage.sprite = null;
        }

        public void ShowMatchEffect()
        {
            // 매칭 표시 애니메이션
            if (blockImage != null)
            {
                blockImage.DOColor(matchColor, 0.2f)
                    .SetLoops(2, LoopType.Yoyo);
            }
        }

        public Tween ClearWithAnimation(System.Action onComplete = null)
        {
            // 클리어 애니메이션
            Sequence clearSequence = DOTween.Sequence();
            clearSequence.Append(blockImage.transform.DOScale(Vector3.zero, Constants.LINE_CLEAR_DURATION)
                .SetEase(Ease.InBack));
            clearSequence.AppendCallback(() =>
            {
                RemoveBlock();
                onComplete?.Invoke();
            });

            return clearSequence;
        }
    }
}
