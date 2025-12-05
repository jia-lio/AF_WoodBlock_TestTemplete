using UnityEngine;
using UnityEditor;
using WoodBlock.Gameplay;

namespace WoodBlock.Editor
{
    /// <summary>
    /// 에디터 헬퍼: 보드 및 블록 설정 자동화
    /// </summary>
    public class BoardSetupHelper : EditorWindow
    {
        private Sprite cellSprite;
        private Sprite blockSprite;
        private Sprite shadowSprite;

        [MenuItem("WoodBlock/Setup Board")]
        public static void ShowWindow()
        {
            GetWindow<BoardSetupHelper>("Board Setup");
        }

        private void OnGUI()
        {
            GUILayout.Label("8x8 그리드 보드 자동 설정", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            cellSprite = (Sprite)EditorGUILayout.ObjectField("Cell Sprite", cellSprite, typeof(Sprite), false);
            blockSprite = (Sprite)EditorGUILayout.ObjectField("Block Sprite", blockSprite, typeof(Sprite), false);
            shadowSprite = (Sprite)EditorGUILayout.ObjectField("Shadow Sprite", shadowSprite, typeof(Sprite), false);

            EditorGUILayout.Space();

            if (GUILayout.Button("Cell 프리팹 생성", GUILayout.Height(30)))
            {
                CreateCellPrefab();
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("GameManager 자동 설정", GUILayout.Height(30)))
            {
                SetupGameManager();
            }

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "1. Cell Sprite를 할당하고 'Cell 프리팹 생성' 클릭\n" +
                "2. 'GameManager 자동 설정' 클릭하여 씬에 매니저 추가",
                MessageType.Info);
        }

        private void CreateCellPrefab()
        {
            if (cellSprite == null)
            {
                EditorUtility.DisplayDialog("오류", "Cell Sprite를 먼저 할당해주세요!", "확인");
                return;
            }

            // Cell 프리팹 생성
            GameObject cellObj = new GameObject("Cell");

            // SpriteRenderer 추가
            SpriteRenderer sr = cellObj.AddComponent<SpriteRenderer>();
            sr.sprite = cellSprite;
            sr.sortingOrder = 0;

            // BoxCollider2D 추가
            BoxCollider2D collider = cellObj.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(1f, 1f);

            // Cell 스크립트 추가
            Cell cell = cellObj.AddComponent<Cell>();

            // 프리팹으로 저장
            string path = "Assets/@Resources/Prefabs/Cell.prefab";

            // Prefabs 폴더 생성
            if (!AssetDatabase.IsValidFolder("Assets/@Resources/Prefabs"))
            {
                AssetDatabase.CreateFolder("Assets/@Resources", "Prefabs");
            }

            PrefabUtility.SaveAsPrefabAsset(cellObj, path);
            DestroyImmediate(cellObj);

            EditorUtility.DisplayDialog("완료", "Cell 프리팹이 생성되었습니다!\n경로: " + path, "확인");
            AssetDatabase.Refresh();
        }

        private void SetupGameManager()
        {
            // 1. GameManager 생성
            GameObject gameManagerObj = new GameObject("GameManager");
            var gameManager = gameManagerObj.AddComponent<Core.GameManager>();

            // 2. BoardManager 생성
            GameObject boardManagerObj = new GameObject("BoardManager");
            boardManagerObj.transform.SetParent(gameManagerObj.transform);
            var boardManager = boardManagerObj.AddComponent<Core.BoardManager>();

            // 스프라이트 할당
            if (cellSprite != null)
                boardManager.cellSprite = cellSprite;
            if (blockSprite != null)
                boardManager.blockSprite = blockSprite;

            // Cell 프리팹 로드 시도
            GameObject cellPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/@Resources/Prefabs/Cell.prefab");
            if (cellPrefab != null)
                boardManager.cellPrefab = cellPrefab;

            // 3. BlockManager 생성
            GameObject blockManagerObj = new GameObject("BlockManager");
            blockManagerObj.transform.SetParent(gameManagerObj.transform);
            var blockManager = blockManagerObj.AddComponent<Core.BlockManager>();

            if (blockSprite != null)
                blockManager.blockCellSprite = blockSprite;
            if (shadowSprite != null)
                blockManager.shadowSprite = shadowSprite;

            // 4. ScoreManager 생성
            GameObject scoreManagerObj = new GameObject("ScoreManager");
            scoreManagerObj.transform.SetParent(gameManagerObj.transform);
            scoreManagerObj.AddComponent<Managers.ScoreManager>();

            // 5. SoundManager 생성
            GameObject soundManagerObj = new GameObject("SoundManager");
            soundManagerObj.AddComponent<Managers.SoundManager>();

            // 6. UIManager 생성 (Canvas)
            GameObject canvasObj = new GameObject("Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            canvasObj.AddComponent<Managers.UIManager>();

            // GameManager에 연결
            gameManager.boardManager = boardManager;
            gameManager.blockManager = blockManager;

            EditorUtility.DisplayDialog("완료",
                "GameManager와 모든 매니저가 생성되었습니다!\n\n" +
                "다음 단계:\n" +
                "1. Play 버튼을 눌러 8x8 그리드 확인\n" +
                "2. UIManager에 UI 요소 추가",
                "확인");

            Selection.activeGameObject = gameManagerObj;
        }
    }
}
