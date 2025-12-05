using UnityEngine;
using DG.Tweening;

namespace WoodBlock.Gameplay
{
    /// <summary>
    /// 게임 보드의 개별 셀
    /// </summary>
    public class Cell : MonoBehaviour
    {
        [Header("셀 정보")]
        public int gridX;
        public int gridY;
        public bool isOccupied = false;

        [Header("비주얼")]
        public SpriteRenderer cellRenderer;
        public SpriteRenderer blockRenderer;
        public GameObject matchEffect;
        public GameObject clearEffect;

        [Header("색상")]
        public Color normalColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        public Color highlightColor = new Color(1f, 1f, 0.7f, 1f);
        public Color occupiedColor = new Color(0.6f, 0.4f, 0.2f, 1f);
        public Color matchColor = new Color(1f, 0.8f, 0.3f, 1f);

        private void Awake()
        {
            if (cellRenderer == null)
                cellRenderer = GetComponent<SpriteRenderer>();

            if (blockRenderer == null)
            {
                GameObject blockObj = new GameObject("Block");
                blockObj.transform.SetParent(transform);
                blockObj.transform.localPosition = Vector3.zero;
                blockRenderer = blockObj.AddComponent<SpriteRenderer>();
                blockRenderer.sortingOrder = cellRenderer.sortingOrder + 1;
                blockRenderer.enabled = false;
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
            cellRenderer.color = normalColor;
        }

        public void SetHighlightState(bool canPlace)
        {
            cellRenderer.color = canPlace ? highlightColor : Color.red;
        }

        public void PlaceBlock(Sprite blockSprite)
        {
            isOccupied = true;
            blockRenderer.sprite = blockSprite;
            blockRenderer.enabled = true;
            blockRenderer.color = occupiedColor;

            // 배치 애니메이션
            blockRenderer.transform.localScale = Vector3.zero;
            blockRenderer.transform.DOScale(Vector3.one, Constants.BLOCK_PLACE_DURATION)
                .SetEase(Ease.OutBack);
        }

        public void RemoveBlock()
        {
            isOccupied = false;
            blockRenderer.enabled = false;
            blockRenderer.sprite = null;
        }

        public void ShowMatchEffect()
        {
            // 매칭 표시 애니메이션
            blockRenderer.DOColor(matchColor, 0.2f)
                .SetLoops(2, LoopType.Yoyo);
        }

        public Tween ClearWithAnimation(System.Action onComplete = null)
        {
            // 클리어 애니메이션
            Sequence clearSequence = DOTween.Sequence();
            clearSequence.Append(blockRenderer.transform.DOScale(Vector3.zero, Constants.LINE_CLEAR_DURATION)
                .SetEase(Ease.InBack));
            clearSequence.AppendCallback(() =>
            {
                RemoveBlock();
                onComplete?.Invoke();
            });

            return clearSequence;
        }

        private void OnMouseEnter()
        {
            if (!isOccupied)
            {
                cellRenderer.color = highlightColor;
            }
        }

        private void OnMouseExit()
        {
            if (!isOccupied)
            {
                SetNormalState();
            }
        }
    }
}
