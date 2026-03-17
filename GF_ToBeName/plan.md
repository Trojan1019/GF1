# Block Blast Gameplay Implementation Plan

## Phase 1: Cleanup (清理旧玩法)
- [ ] **Delete Legacy Scripts**: Remove all scripts in `Assets/Scripts/Base/` related to `BallSort` namespace (`IBall.cs`, `ITube.cs`, `IGameManager.cs`, `IPay.cs`, `IPos.cs`, `ISize.cs`, `ISprite.cs`).
- [ ] **Delete Legacy Assets**:
    - `Assets/Game/Prefabs/Pool/Question#103.prefab`
    - `Assets/Game/Prefabs/Effcet/Tube_caidai.prefab`
    - `Assets/RawAsset/Sprites/Item/Prop_Tube.png`
- [ ] **Clean Configurations**:
    - Remove `BallSort` references from `Assets/link.xml`.
    - Remove `Question#103` references from `Assets/Game/Configs/ResourcesIdentification.xml`.
    - Remove `BallSort` references from `Assets/GameFramework/Configs/ResourceCollection.xml`.
- [ ] **Verify Cleanup**: Ensure no compilation errors remain.

## Phase 2: Core Architecture (核心架构设计)
- [ ] **Define Data Structures**:
    - `BlockShape`: ScriptableObject or class defining block shapes (e.g., 2x2, L-shape, Line-3).
    - `GridState`: 2D array representing the 8x8 board.
- [ ] **Create Core Managers**:
    - `GridManager`: Handles grid logic (place block, check lines, clear lines).
    - `BlockSpawner`: Manages the 3 available blocks at the bottom.
    - `GameLoopManager`: Handles score, game over check, and restart.

## Phase 3: Implementation (功能实现)
- [ ] **Grid System**:
    - Implement `GridManager` with `CanPlace(BlockShape, Vector2Int pos)` and `Place(BlockShape, Vector2Int pos)`.
    - Implement `CheckLines()` to identify full rows/columns.
    - Implement `ClearLines()` to remove blocks and update score.
- [ ] **Block Interaction**:
    - Create `BlockItem` UI component with `IDragHandler`, `IBeginDragHandler`, `IEndDragHandler`.
    - Implement visual feedback (ghost block or highlight) when dragging over valid grid positions.
    - Handle "return to spawn" animation if placement is invalid.
- [ ] **UI Integration**:
    - Update `UIGamePlayForm` to host the Grid and Spawn Area.
    - Display current score and high score.
- [ ] **Game Flow**:
    - Implement Game Over detection (when no spawned blocks fit in the grid).
    - Implement Restart functionality.

## Phase 4: Polish & Effects (特效与优化)
- [ ] **Visual Effects**:
    - Add particle effects for line clearing.
    - Add screen shake or punch scale animation on combo clears.
- [ ] **Audio**:
    - Integrate sound effects for: Pick up, Place, Clear Line, Game Over.

## Phase 5: Testing (测试)
- [ ] **Unit Tests**:
    - Test `GridManager.CanPlace` with various shapes and grid states.
    - Test `GridManager.CheckLines` for single and multiple line clears.
- [ ] **Integration Tests**:
    - Simulate a full game loop (Start -> Place Blocks -> Clear -> Game Over).
