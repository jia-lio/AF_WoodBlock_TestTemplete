# 🎮 Wood Block Puzzle - Unity 설정 가이드

## 📋 목차
1. [자동 설정 (권장)](#자동-설정-권장)
2. [수동 설정](#수동-설정)
3. [스프라이트 할당](#스프라이트-할당)
4. [테스트 및 확인](#테스트-및-확인)

---

## ⚡ 자동 설정 (권장)

Unity 에디터에서 자동으로 8x8 그리드를 설정하는 가장 쉬운 방법입니다.

### 1단계: Editor 헬퍼 열기

Unity 에디터 상단 메뉴:
```
WoodBlock → Setup Board
```

### 2단계: 스프라이트 할당

Board Setup 창에서:

1. **Cell Sprite**: `Assets/@Resources/Sprites/@Block/Cell.png` 드래그
2. **Block Sprite**: `Assets/@Resources/Sprites/@Block/Block.png` 드래그
3. **Shadow Sprite**: `Assets/@Resources/Sprites/@Block/Block_Shadow.png` 드래그

### 3단계: 자동 생성

1. **"Cell 프리팹 생성"** 버튼 클릭
   - Cell.prefab이 `Assets/@Resources/Prefabs/`에 생성됩니다

2. **"GameManager 자동 설정"** 버튼 클릭
   - GameManager와 모든 매니저가 씬에 자동 추가됩니다

### 4단계: 게임 실행

**▶ Play 버튼 클릭!**

→ 8x8 그리드가 자동으로 화면 중앙에 생성됩니다!

---

## 🔧 수동 설정

자동 설정이 작동하지 않거나, 직접 설정하고 싶은 경우:

### 1. GameManager 오브젝트 생성

```
Hierarchy 우클릭 → Create Empty
이름: "GameManager"
Add Component → GameManager (스크립트)
```

### 2. BoardManager 생성

```
GameManager 우클릭 → Create Empty
이름: "BoardManager"
Add Component → BoardManager (스크립트)
Transform Position: (0, 0, 0)
```

**BoardManager Inspector 설정:**
- **Cell Sprite**: Cell.png 할당
- **Block Sprite**: Block.png 할당

### 3. BlockManager 생성

```
GameManager 우클릭 → Create Empty
이름: "BlockManager"
Add Component → BlockManager (스크립트)
Transform Position: (0, -5, 0)
```

**BlockManager Inspector 설정:**
- **Block Cell Sprite**: Block.png 할당
- **Shadow Sprite**: Block_Shadow.png 할당

### 4. ScoreManager 생성

```
GameManager 우클릭 → Create Empty
이름: "ScoreManager"
Add Component → ScoreManager (스크립트)
```

### 5. SoundManager 생성

```
Hierarchy 우클릭 → Create Empty
이름: "SoundManager"
Add Component → SoundManager (스크립트)
```

### 6. UIManager (Canvas) 생성

```
Hierarchy 우클릭 → UI → Canvas
이름: "Canvas"
Add Component → UIManager (스크립트)
```

### 7. GameManager 연결

GameManager Inspector에서:
- **Board Manager**: BoardManager 드래그
- **Block Manager**: BlockManager 드래그
- **Score Manager**: ScoreManager 드래그
- **Sound Manager**: SoundManager 드래그
- **UI Manager**: Canvas의 UIManager 드래그

---

## 🎨 스프라이트 할당

### 필수 스프라이트

| 컴포넌트 | 변수명 | 경로 |
|---------|--------|------|
| BoardManager | Cell Sprite | `@Resources/Sprites/@Block/Cell.png` |
| BoardManager | Block Sprite | `@Resources/Sprites/@Block/Block.png` |
| BlockManager | Block Cell Sprite | `@Resources/Sprites/@Block/Block.png` |
| BlockManager | Shadow Sprite | `@Resources/Sprites/@Block/Block_Shadow.png` |

### 스프라이트 Import 설정

각 스프라이트를 선택하고 Inspector에서:

```
Texture Type: Sprite (2D and UI)
Pixels Per Unit: 100
Filter Mode: Bilinear
Compression: None (또는 High Quality)
```

**Apply** 클릭!

---

## 🎯 테스트 및 확인

### 1. 그리드 확인

Play 모드에서:
- ✅ 8x8 그리드 (64개 셀)가 화면 중앙에 생성
- ✅ 각 셀이 Cell.png 스프라이트로 표시
- ✅ 셀 간격이 균일하게 배치

### 2. 블록 생성 확인

Play 모드에서:
- ✅ 화면 하단에 3개의 블록이 생성
- ✅ 각 블록이 랜덤한 모양
- ✅ 블록에 그림자 효과

### 3. 드래그 테스트

- ✅ 블록을 마우스/터치로 드래그 가능
- ✅ 보드 위에 놓으면 배치됨
- ✅ 배치 불가능한 위치는 빨간색 표시

### 4. 라인 제거 테스트

- ✅ 가로줄 8칸 완성 시 제거
- ✅ 세로줄 8칸 완성 시 제거
- ✅ 제거 애니메이션 재생
- ✅ 점수 증가

---

## 🎬 카메라 설정

8x8 그리드를 화면에 잘 보이게 하려면:

### 1. Main Camera 선택

### 2. Inspector 설정

```
Projection: Orthographic
Size: 6 ~ 8 (화면 크기에 맞게 조절)
Position: (0, 0, -10)
Background: 단색 또는 원하는 색상
```

### 3. Canvas Scaler 설정

Canvas 선택 → Canvas Scaler:

```
UI Scale Mode: Scale With Screen Size
Reference Resolution: 1080 x 1920 (세로 모드)
Match: 0.5
```

---

## 🔍 문제 해결

### 그리드가 보이지 않아요
- Camera Size를 6~8로 조정
- BoardManager의 cellSprite가 할당되었는지 확인
- Camera Position Z가 -10인지 확인

### 블록을 드래그할 수 없어요
- EventSystem이 씬에 있는지 확인 (Canvas 생성 시 자동 생성)
- Block에 BoxCollider2D가 있는지 확인
- Canvas에 GraphicRaycaster가 있는지 확인

### 블록이 생성되지 않아요
- BlockManager의 blockCellSprite 할당 확인
- BlockManager의 Transform Position 확인 (Y가 -4 ~ -5)

### 스크립트 오류가 발생해요
- UniTask 패키지 설치 확인
- DOTween 패키지 설치 확인
- TextMesh Pro 패키지 설치 확인

---

## 📦 필수 패키지

Package Manager (Window → Package Manager)에서 설치:

1. **TextMesh Pro** (Unity Registry)
2. **Addressables** (이미 설치됨)
3. **Input System** (이미 설치됨)

Asset Store에서 설치:

1. **DOTween** (이미 Plugins에 있음)
2. **UniTask** (Packages/manifest.json에 추가 필요)

### UniTask 설치 (필수)

`Packages/manifest.json` 파일 열기:

```json
{
  "dependencies": {
    "com.cysharp.unitask": "https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask",
    ...
  }
}
```

---

## ✅ 최종 확인 체크리스트

- [ ] GameManager 생성 및 스크립트 할당
- [ ] BoardManager 생성 및 스프라이트 할당
- [ ] BlockManager 생성 및 스프라이트 할당
- [ ] ScoreManager, SoundManager, UIManager 생성
- [ ] GameManager에 모든 매니저 연결
- [ ] Camera 설정 (Orthographic, Size 6-8)
- [ ] EventSystem 존재 확인
- [ ] UniTask 패키지 설치
- [ ] Play 모드에서 8x8 그리드 확인
- [ ] 블록 드래그 테스트
- [ ] 라인 제거 테스트

---

## 🎉 완료!

이제 Wood Block Puzzle 게임이 작동합니다!

더 자세한 구현 내용은 `IMPLEMENTATION.md`를 참고하세요.
