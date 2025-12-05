using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using WoodBlock.Gameplay;
using WoodBlock.Data;
using Cysharp.Threading.Tasks;

namespace WoodBlock.Core
{
    /// <summary>
    /// 게임 보드 관리 (UI 버전 - Canvas 내부)
    /// </summary>
    public class BoardManagerUI : MonoBehaviour
    {
        [Header("보드 설정")]
        public RectTransform boardRoot;
        public Sprite cellSprite;
        public Sprite blockSprite;

        [Header("보드 UI 설정")]
        public float cellSize = 100f;
        public float cellSpacing = 10f;

        private CellUI[,] cells;
        private bool[,] gridState;

        private void Awake()
        {
            cells = new CellUI[Constants.BOARD_SIZE, Constants.BOARD_SIZE];
            gridState = new bool[Constants.BOARD_SIZE, Constants.BOARD_SIZE];
        }

        public void InitializeBoard()
        {
            CreateBoard();
            ClearBoard();
        }

        private void CreateBoard()
        {
            if (boardRoot == null)
            {
                GameObject rootObj = new GameObject("BoardRoot");
                boardRoot = rootObj.AddComponent<RectTransform>();
                boardRoot.SetParent(transform);
                boardRoot.anchoredPosition = Vector2.zero;

                // GridLayoutGroup 추가
                GridLayoutGroup grid = rootObj.AddComponent<GridLayoutGroup>();
                grid.cellSize = new Vector2(cellSize, cellSize);
                grid.spacing = new Vector2(cellSpacing, cellSpacing);
                grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                grid.constraintCount = Constants.BOARD_SIZE;
                grid.childAlignment = TextAnchor.MiddleCenter;
            }

            for (int y = 0; y < Constants.BOARD_SIZE; y++)
            {
                for (int x = 0; x < Constants.BOARD_SIZE; x++)
                {
                    GameObject cellObj = new GameObject($"Cell_{x}_{y}");
                    cellObj.transform.SetParent(boardRoot);

                    // RectTransform 설정
                    RectTransform rt = cellObj.AddComponent<RectTransform>();
                    rt.sizeDelta = new Vector2(cellSize, cellSize);

                    // Image 추가
                    Image cellImage = cellObj.AddComponent<Image>();
                    cellImage.sprite = cellSprite;
                    cellImage.raycastTarget = true;

                    // CellUI 스크립트 추가
                    CellUI cell = cellObj.AddComponent<CellUI>();
                    cell.cellImage = cellImage;
                    cell.Initialize(x, y);

                    cells[x, y] = cell;
                    gridState[x, y] = false;
                }
            }
        }

        public void ClearBoard()
        {
            for (int y = 0; y < Constants.BOARD_SIZE; y++)
            {
                for (int x = 0; x < Constants.BOARD_SIZE; x++)
                {
                    cells[x, y].RemoveBlock();
                    gridState[x, y] = false;
                }
            }
        }

        public bool TryPlaceBlock(Block block, Vector2 screenPosition)
        {
            // 스크린 좌표를 그리드 좌표로 변환
            Vector2Int gridPos = ScreenToGrid(screenPosition);

            // 배치 가능 여부 확인
            if (!CanPlaceBlock(block.blockShape, gridPos))
            {
                return false;
            }

            // 블록 배치
            PlaceBlock(block.blockShape, gridPos);

            return true;
        }

        private Vector2Int ScreenToGrid(Vector2 screenPos)
        {
            // RectTransform 내부 좌표로 변환
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                boardRoot,
                screenPos,
                null,
                out Vector2 localPoint
            );

            // GridLayoutGroup 고려하여 그리드 인덱스 계산
            float totalCellSize = cellSize + cellSpacing;

            // 보드 시작 위치 계산 (중앙 정렬)
            float boardWidth = Constants.BOARD_SIZE * totalCellSize - cellSpacing;
            float startX = -boardWidth / 2f;
            float startY = boardWidth / 2f;

            int gridX = Mathf.RoundToInt((localPoint.x - startX) / totalCellSize);
            int gridY = Mathf.RoundToInt((startY - localPoint.y) / totalCellSize);

            return new Vector2Int(gridX, gridY);
        }

        public bool CanPlaceBlock(BlockShape shape, Vector2Int startPos)
        {
            for (int y = 0; y < shape.height; y++)
            {
                for (int x = 0; x < shape.width; x++)
                {
                    if (shape.shape[y, x] == 1)
                    {
                        int gridX = startPos.x + x;
                        int gridY = startPos.y + y;

                        // 범위 체크
                        if (gridX < 0 || gridX >= Constants.BOARD_SIZE ||
                            gridY < 0 || gridY >= Constants.BOARD_SIZE)
                        {
                            return false;
                        }

                        // 이미 차있는지 체크
                        if (gridState[gridX, gridY])
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        private void PlaceBlock(BlockShape shape, Vector2Int startPos)
        {
            for (int y = 0; y < shape.height; y++)
            {
                for (int x = 0; x < shape.width; x++)
                {
                    if (shape.shape[y, x] == 1)
                    {
                        int gridX = startPos.x + x;
                        int gridY = startPos.y + y;

                        gridState[gridX, gridY] = true;
                        cells[gridX, gridY].PlaceBlock(blockSprite);
                    }
                }
            }
        }

        public async UniTask<int> CheckAndClearLines()
        {
            List<int> rowsToClear = new List<int>();
            List<int> colsToClear = new List<int>();

            // 완성된 가로줄 찾기
            for (int y = 0; y < Constants.BOARD_SIZE; y++)
            {
                bool isComplete = true;
                for (int x = 0; x < Constants.BOARD_SIZE; x++)
                {
                    if (!gridState[x, y])
                    {
                        isComplete = false;
                        break;
                    }
                }

                if (isComplete)
                {
                    rowsToClear.Add(y);
                }
            }

            // 완성된 세로줄 찾기
            for (int x = 0; x < Constants.BOARD_SIZE; x++)
            {
                bool isComplete = true;
                for (int y = 0; y < Constants.BOARD_SIZE; y++)
                {
                    if (!gridState[x, y])
                    {
                        isComplete = false;
                        break;
                    }
                }

                if (isComplete)
                {
                    colsToClear.Add(x);
                }
            }

            int totalLinesCleared = rowsToClear.Count + colsToClear.Count;

            if (totalLinesCleared > 0)
            {
                // 매칭 효과 표시
                ShowMatchEffect(rowsToClear, colsToClear);
                await UniTask.Delay(200);

                // 라인 제거
                await ClearLines(rowsToClear, colsToClear);
            }

            return totalLinesCleared;
        }

        private void ShowMatchEffect(List<int> rows, List<int> cols)
        {
            foreach (int y in rows)
            {
                for (int x = 0; x < Constants.BOARD_SIZE; x++)
                {
                    cells[x, y].ShowMatchEffect();
                }
            }

            foreach (int x in cols)
            {
                for (int y = 0; y < Constants.BOARD_SIZE; y++)
                {
                    cells[x, y].ShowMatchEffect();
                }
            }
        }

        private async UniTask ClearLines(List<int> rows, List<int> cols)
        {
            List<UniTask> clearTasks = new List<UniTask>();

            // 가로줄 제거
            foreach (int y in rows)
            {
                for (int x = 0; x < Constants.BOARD_SIZE; x++)
                {
                    int capturedX = x;
                    int capturedY = y;

                    UniTask task = UniTask.Create(async () =>
                    {
                        await cells[capturedX, capturedY].ClearWithAnimation().ToUniTask();
                        gridState[capturedX, capturedY] = false;
                    });

                    clearTasks.Add(task);
                }
            }

            // 세로줄 제거
            foreach (int x in cols)
            {
                for (int y = 0; y < Constants.BOARD_SIZE; y++)
                {
                    int capturedX = x;
                    int capturedY = y;

                    // 이미 가로줄로 제거 예정이면 스킵
                    if (rows.Contains(y)) continue;

                    UniTask task = UniTask.Create(async () =>
                    {
                        await cells[capturedX, capturedY].ClearWithAnimation().ToUniTask();
                        gridState[capturedX, capturedY] = false;
                    });

                    clearTasks.Add(task);
                }
            }

            await UniTask.WhenAll(clearTasks);
        }

        public bool HasAnyValidPlacement(List<BlockShape> blocks)
        {
            foreach (var block in blocks)
            {
                if (HasValidPlacement(block))
                {
                    return true;
                }
            }

            return false;
        }

        private bool HasValidPlacement(BlockShape shape)
        {
            for (int y = 0; y < Constants.BOARD_SIZE; y++)
            {
                for (int x = 0; x < Constants.BOARD_SIZE; x++)
                {
                    if (CanPlaceBlock(shape, new Vector2Int(x, y)))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public CellUI GetCell(int x, int y)
        {
            if (x >= 0 && x < Constants.BOARD_SIZE && y >= 0 && y < Constants.BOARD_SIZE)
            {
                return cells[x, y];
            }
            return null;
        }
    }
}
