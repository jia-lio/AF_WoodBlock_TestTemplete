using UnityEngine;
using System.Collections.Generic;
using WoodBlock.Gameplay;
using WoodBlock.Data;
using Cysharp.Threading.Tasks;
using System.Linq;

namespace WoodBlock.Core
{
    /// <summary>
    /// 게임 보드 관리 (8x8 그리드)
    /// </summary>
    public class BoardManager : MonoBehaviour
    {
        [Header("보드 설정")]
        public Transform boardRoot;
        public Sprite cellSprite;
        public Sprite blockSprite;

        [Header("프리팹")]
        public GameObject cellPrefab;

        private Cell[,] cells;
        private bool[,] gridState;

        private void Awake()
        {
            cells = new Cell[Constants.BOARD_SIZE, Constants.BOARD_SIZE];
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
                boardRoot = new GameObject("BoardRoot").transform;
                boardRoot.SetParent(transform);
                boardRoot.localPosition = Vector3.zero;
            }

            // 보드 중앙 정렬을 위한 오프셋
            float totalSize = Constants.BOARD_SIZE * (Constants.CELL_SIZE + Constants.CELL_SPACING);
            float offset = -totalSize / 2f + (Constants.CELL_SIZE + Constants.CELL_SPACING) / 2f;

            for (int y = 0; y < Constants.BOARD_SIZE; y++)
            {
                for (int x = 0; x < Constants.BOARD_SIZE; x++)
                {
                    GameObject cellObj;

                    if (cellPrefab != null)
                    {
                        cellObj = Instantiate(cellPrefab, boardRoot);
                    }
                    else
                    {
                        cellObj = new GameObject($"Cell_{x}_{y}");
                        cellObj.transform.SetParent(boardRoot);
                        cellObj.AddComponent<SpriteRenderer>().sprite = cellSprite;
                        cellObj.AddComponent<BoxCollider2D>();
                        cellObj.AddComponent<Cell>();
                    }

                    float posX = offset + x * (Constants.CELL_SIZE + Constants.CELL_SPACING);
                    float posY = offset + y * (Constants.CELL_SIZE + Constants.CELL_SPACING);
                    cellObj.transform.localPosition = new Vector3(posX, posY, 0);

                    Cell cell = cellObj.GetComponent<Cell>();
                    cell.Initialize(x, y);

                    // 셀 스프라이트 설정
                    SpriteRenderer sr = cellObj.GetComponent<SpriteRenderer>();
                    if (sr != null && cellSprite != null)
                    {
                        sr.sprite = cellSprite;
                        sr.sortingOrder = 0;
                    }

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

        public bool TryPlaceBlock(Block block, Vector3 worldPosition)
        {
            // 월드 좌표를 그리드 좌표로 변환
            Vector2Int gridPos = WorldToGrid(worldPosition);

            // 배치 가능 여부 확인
            if (!CanPlaceBlock(block.blockShape, gridPos))
            {
                return false;
            }

            // 블록 배치
            PlaceBlock(block.blockShape, gridPos);

            return true;
        }

        private Vector2Int WorldToGrid(Vector3 worldPos)
        {
            Vector3 localPos = boardRoot.InverseTransformPoint(worldPos);

            float totalSize = Constants.BOARD_SIZE * (Constants.CELL_SIZE + Constants.CELL_SPACING);
            float offset = -totalSize / 2f + (Constants.CELL_SIZE + Constants.CELL_SPACING) / 2f;

            int gridX = Mathf.RoundToInt((localPos.x - offset) / (Constants.CELL_SIZE + Constants.CELL_SPACING));
            int gridY = Mathf.RoundToInt((localPos.y - offset) / (Constants.CELL_SIZE + Constants.CELL_SPACING));

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

        public Cell GetCell(int x, int y)
        {
            if (x >= 0 && x < Constants.BOARD_SIZE && y >= 0 && y < Constants.BOARD_SIZE)
            {
                return cells[x, y];
            }
            return null;
        }
    }
}
