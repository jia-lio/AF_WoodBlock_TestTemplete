using UnityEngine;
using UnityEngine.EventSystems;
using WoodBlock.Data;
using DG.Tweening;
using System.Collections.Generic;

namespace WoodBlock.Gameplay
{
    /// <summary>
    /// 드래그 가능한 블록
    /// </summary>
    public class Block : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("블록 데이터")]
        public BlockShape blockShape;
        public int slotIndex;

        [Header("비주얼")]
        public SpriteRenderer shadowRenderer;
        private List<SpriteRenderer> cellRenderers = new List<SpriteRenderer>();

        [Header("설정")]
        public Sprite blockCellSprite;
        public Sprite shadowSprite;
        public Color blockColor = new Color(0.6f, 0.4f, 0.2f, 1f);

        private Vector3 originalPosition;
        private Transform originalParent;
        private bool isDragging = false;
        private Camera mainCamera;
        private Canvas canvas;

        public bool IsPlaced { get; private set; } = false;

        private void Awake()
        {
            mainCamera = Camera.main;
        }

        public void Initialize(BlockShape shape, int slot, Sprite cellSprite, Sprite shadowSpr)
        {
            blockShape = shape;
            slotIndex = slot;
            blockCellSprite = cellSprite;
            shadowSprite = shadowSpr;

            CreateVisuals();
            originalPosition = transform.localPosition;
            originalParent = transform.parent;
        }

        private void CreateVisuals()
        {
            // 기존 자식 제거
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }
            cellRenderers.Clear();

            // 그림자 생성
            if (shadowSprite != null)
            {
                GameObject shadowObj = new GameObject("Shadow");
                shadowObj.transform.SetParent(transform);
                shadowObj.transform.localPosition = new Vector3(0.05f, -0.05f, 0);
                shadowRenderer = shadowObj.AddComponent<SpriteRenderer>();
                shadowRenderer.sprite = shadowSprite;
                shadowRenderer.sortingOrder = -1;
                shadowRenderer.color = new Color(0, 0, 0, 0.3f);
            }

            // 블록 셀들 생성
            float offsetX = -(blockShape.width - 1) * 0.5f * Constants.CELL_SIZE;
            float offsetY = -(blockShape.height - 1) * 0.5f * Constants.CELL_SIZE;

            for (int y = 0; y < blockShape.height; y++)
            {
                for (int x = 0; x < blockShape.width; x++)
                {
                    if (blockShape.shape[y, x] == 1)
                    {
                        GameObject cellObj = new GameObject($"Cell_{x}_{y}");
                        cellObj.transform.SetParent(transform);

                        float posX = offsetX + x * Constants.CELL_SIZE;
                        float posY = offsetY + (blockShape.height - 1 - y) * Constants.CELL_SIZE;
                        cellObj.transform.localPosition = new Vector3(posX, posY, 0);

                        SpriteRenderer sr = cellObj.AddComponent<SpriteRenderer>();
                        sr.sprite = blockCellSprite;
                        sr.color = blockColor;
                        sr.sortingOrder = 1;

                        cellRenderers.Add(sr);
                    }
                }
            }

            // 생성 애니메이션
            transform.localScale = Vector3.zero;
            transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (IsPlaced) return;

            isDragging = true;
            originalPosition = transform.position;
            originalParent = transform.parent;

            // 드래그 시 최상위로
            transform.SetParent(transform.root);
            transform.localScale = Vector3.one * 1.2f;

            // 반투명하게
            SetAlpha(0.8f);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!isDragging) return;

            Vector3 worldPos = mainCamera.ScreenToWorldPoint(eventData.position);
            worldPos.z = 0;
            transform.position = worldPos;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!isDragging) return;

            isDragging = false;

            // 보드에 배치 시도
            var boardManager = FindObjectOfType<Core.BoardManager>();
            if (boardManager != null)
            {
                Vector3 worldPos = mainCamera.ScreenToWorldPoint(eventData.position);
                if (boardManager.TryPlaceBlock(this, worldPos))
                {
                    // 배치 성공
                    OnBlockPlaced();

                    // GameManager에 알림
                    var gameManager = FindObjectOfType<Core.GameManager>();
                    if (gameManager != null)
                    {
                        gameManager.OnBlockPlaced(this);
                    }

                    return;
                }
            }

            // 배치 실패 - 원위치로
            ReturnToOriginalPosition();
        }

        private void ReturnToOriginalPosition()
        {
            transform.SetParent(originalParent);
            transform.DOMove(originalPosition, 0.3f).SetEase(Ease.OutQuad);
            transform.DOScale(Vector3.one, 0.3f);
            SetAlpha(1f);
        }

        private void OnBlockPlaced()
        {
            IsPlaced = true;
            // 배치된 블록은 숨김
            gameObject.SetActive(false);
        }

        private void SetAlpha(float alpha)
        {
            foreach (var sr in cellRenderers)
            {
                Color c = sr.color;
                c.a = alpha;
                sr.color = c;
            }

            if (shadowRenderer != null)
            {
                Color sc = shadowRenderer.color;
                sc.a = alpha * 0.3f;
                shadowRenderer.color = sc;
            }
        }

        public List<Vector2Int> GetOccupiedCells(Vector2Int startPos)
        {
            List<Vector2Int> cells = new List<Vector2Int>();

            for (int y = 0; y < blockShape.height; y++)
            {
                for (int x = 0; x < blockShape.width; x++)
                {
                    if (blockShape.shape[y, x] == 1)
                    {
                        cells.Add(new Vector2Int(startPos.x + x, startPos.y + y));
                    }
                }
            }

            return cells;
        }
    }
}
