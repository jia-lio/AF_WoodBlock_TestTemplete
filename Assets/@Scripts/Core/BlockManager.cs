using UnityEngine;
using System.Collections.Generic;
using WoodBlock.Data;
using WoodBlock.Gameplay;

namespace WoodBlock.Core
{
    /// <summary>
    /// 블록 생성 및 관리
    /// </summary>
    public class BlockManager : MonoBehaviour
    {
        [Header("블록 슬롯")]
        public Transform[] blockSlots = new Transform[Constants.MAX_BLOCK_SLOTS];

        [Header("블록 프리팹")]
        public GameObject blockPrefab;

        [Header("스프라이트")]
        public Sprite blockCellSprite;
        public Sprite shadowSprite;

        private List<Block> currentBlocks = new List<Block>();
        private int placedBlockCount = 0;

        public void InitializeBlockSlots()
        {
            // 블록 슬롯 위치 자동 생성
            if (blockSlots[0] == null)
            {
                CreateBlockSlots();
            }

            GenerateNewBlocks();
        }

        private void CreateBlockSlots()
        {
            GameObject slotsParent = new GameObject("BlockSlots");
            slotsParent.transform.SetParent(transform);
            slotsParent.transform.localPosition = Vector3.zero;

            float spacing = 3f;
            float startX = -(spacing * (Constants.MAX_BLOCK_SLOTS - 1)) / 2f;

            for (int i = 0; i < Constants.MAX_BLOCK_SLOTS; i++)
            {
                GameObject slot = new GameObject($"Slot_{i}");
                slot.transform.SetParent(slotsParent.transform);
                slot.transform.localPosition = new Vector3(startX + i * spacing, -4.5f, 0);
                blockSlots[i] = slot.transform;
            }
        }

        public void GenerateNewBlocks()
        {
            ClearCurrentBlocks();

            for (int i = 0; i < Constants.MAX_BLOCK_SLOTS; i++)
            {
                CreateBlock(i);
            }

            placedBlockCount = 0;
        }

        private void ClearCurrentBlocks()
        {
            foreach (var block in currentBlocks)
            {
                if (block != null && block.gameObject != null)
                {
                    Destroy(block.gameObject);
                }
            }

            currentBlocks.Clear();
        }

        private void CreateBlock(int slotIndex)
        {
            if (blockSlots[slotIndex] == null) return;

            BlockShape randomShape = BlockDatabase.GetRandomShape();

            GameObject blockObj;
            if (blockPrefab != null)
            {
                blockObj = Instantiate(blockPrefab, blockSlots[slotIndex]);
            }
            else
            {
                blockObj = new GameObject($"Block_{slotIndex}");
                blockObj.transform.SetParent(blockSlots[slotIndex]);
                blockObj.AddComponent<Block>();
            }

            blockObj.transform.localPosition = Vector3.zero;

            Block block = blockObj.GetComponent<Block>();
            block.Initialize(randomShape, slotIndex, blockCellSprite, shadowSprite);

            currentBlocks.Add(block);
        }

        public void OnBlockPlaced(Block block)
        {
            placedBlockCount++;

            // 모든 블록이 배치되었으면 새 블록 생성
            if (placedBlockCount >= Constants.MAX_BLOCK_SLOTS)
            {
                GenerateNewBlocks();
            }
        }

        public List<BlockShape> GetActiveBlockShapes()
        {
            List<BlockShape> shapes = new List<BlockShape>();

            foreach (var block in currentBlocks)
            {
                if (block != null && block.gameObject.activeSelf && !block.IsPlaced)
                {
                    shapes.Add(block.blockShape);
                }
            }

            return shapes;
        }

        public bool HasActiveBlocks()
        {
            foreach (var block in currentBlocks)
            {
                if (block != null && block.gameObject.activeSelf && !block.IsPlaced)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
