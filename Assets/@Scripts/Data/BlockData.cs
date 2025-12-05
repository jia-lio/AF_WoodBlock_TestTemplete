using System;
using System.Collections.Generic;
using UnityEngine;

namespace WoodBlock.Data
{
    /// <summary>
    /// 블록 모양 정의 (1 = 채워진 셀, 0 = 빈 셀)
    /// </summary>
    [Serializable]
    public class BlockShape
    {
        public int id;
        public int width;
        public int height;
        public int[,] shape;

        public BlockShape(int id, int[,] shape)
        {
            this.id = id;
            this.shape = shape;
            this.height = shape.GetLength(0);
            this.width = shape.GetLength(1);
        }

        public int GetCellCount()
        {
            int count = 0;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (shape[y, x] == 1) count++;
                }
            }
            return count;
        }
    }

    /// <summary>
    /// 블록 모양 데이터베이스
    /// </summary>
    public static class BlockDatabase
    {
        private static List<BlockShape> blockShapes;

        public static List<BlockShape> GetAllShapes()
        {
            if (blockShapes == null)
            {
                InitializeShapes();
            }
            return blockShapes;
        }

        public static BlockShape GetRandomShape()
        {
            if (blockShapes == null)
            {
                InitializeShapes();
            }
            return blockShapes[UnityEngine.Random.Range(0, blockShapes.Count)];
        }

        private static void InitializeShapes()
        {
            blockShapes = new List<BlockShape>
            {
                // 1x1 블록
                new BlockShape(1, new int[,] { { 1 } }),

                // 2x1 블록 (가로)
                new BlockShape(2, new int[,] { { 1, 1 } }),

                // 1x2 블록 (세로)
                new BlockShape(3, new int[,] { { 1 }, { 1 } }),

                // 3x1 블록 (가로)
                new BlockShape(4, new int[,] { { 1, 1, 1 } }),

                // 1x3 블록 (세로)
                new BlockShape(5, new int[,] { { 1 }, { 1 }, { 1 } }),

                // 2x2 블록
                new BlockShape(6, new int[,] { { 1, 1 }, { 1, 1 } }),

                // 3x3 블록
                new BlockShape(7, new int[,] { { 1, 1, 1 }, { 1, 1, 1 }, { 1, 1, 1 } }),

                // L자 블록 (4가지 회전)
                new BlockShape(8, new int[,] { { 1, 0 }, { 1, 0 }, { 1, 1 } }),
                new BlockShape(9, new int[,] { { 1, 1, 1 }, { 1, 0, 0 } }),
                new BlockShape(10, new int[,] { { 1, 1 }, { 0, 1 }, { 0, 1 } }),
                new BlockShape(11, new int[,] { { 0, 0, 1 }, { 1, 1, 1 } }),

                // T자 블록 (4가지 회전)
                new BlockShape(12, new int[,] { { 1, 1, 1 }, { 0, 1, 0 } }),
                new BlockShape(13, new int[,] { { 0, 1 }, { 1, 1 }, { 0, 1 } }),
                new BlockShape(14, new int[,] { { 0, 1, 0 }, { 1, 1, 1 } }),
                new BlockShape(15, new int[,] { { 1, 0 }, { 1, 1 }, { 1, 0 } }),

                // Z자 블록 (2가지)
                new BlockShape(16, new int[,] { { 1, 1, 0 }, { 0, 1, 1 } }),
                new BlockShape(17, new int[,] { { 0, 1 }, { 1, 1 }, { 1, 0 } }),
            };
        }
    }
}
