# Skin System Usage

## 1) Runtime Components

- `SkinManager`:
  - Add to `GamePlay` scene root (recommended under `Canvas/GameMain`).
  - Drag references:
    - `bgImage` -> `Canvas/Bg` Image
    - `boardImage` -> `Canvas/Board` Image
    - `skinPrefabRoot` -> optional root transform for skin extra prefabs
  - Assign `skinDatabase` asset.

- `UIGamePlayForm`:
  - Bind `skinButton` (new button in gameplay UI).
  - Clicking opens `SkinDialog`.

- `SkinDialog`:
  - Inherit `BaseDialog`.
  - Requires:
    - `contentRoot` under a `ScrollRect` content with `Horizontal Layout Group`
    - `itemTemplate` (`SkinItem` prefab)
    - `closeButton`, optional `titleText`

- `SkinItem`:
  - Required UI fields:
    - `previewImage`
    - `nameText`
    - `unlockHintText`
    - `checkIcon`
    - `lockIcon`
    - `clickButton`

## 2) Config Fields

`SkinConfig`:

- `skinId`: unique int id.
- `skinName`: fallback display name.
- `skinNameKey`: localization key for name.
- `previewSprite`: list preview.
- `adWatchRequiredCount`: unlock ad count.
- `unlocked`: default unlock state.
- `bgSprite`: gameplay background sprite.
- `boardSprite`: gameplay board sprite.
- `relatedPrefabResourcePaths`: prefab paths under `Resources/`, instantiated after apply.

`SkinDatabase`:

- `skins`: ordered skin list.
- first skin is default unlocked/using.

## 3) Event Payloads

- `Constant.Event.SkinSelectedEvent (108)`
  - args: `(int skinId)`
- `Constant.Event.SkinUnlockedEvent (109)`
  - args: `(int skinId, int watchedAdCount)`
- `Constant.Event.SkinAppliedEvent (110)`
  - args: `(int newSkinId, int oldSkinId)`

### Listener Example

```csharp
EventManager.Instance.AddEventListener(Constant.Event.SkinAppliedEvent, OnSkinApplied);

private void OnSkinApplied(params object[] args)
{
    int newSkinId = (int)args[0];
    int oldSkinId = (int)args[1];
    Debug.Log($"Skin changed: {oldSkinId} -> {newSkinId}");
}
```

## 4) Localization Keys (CN / EN)

- `20001`: 皮肤 / Skins
- `20003`: 观看{0}次广告解锁 / Watch {0} ads to unlock
- `20004`: 解锁提示 / Unlock
- `20005`: 观看{0}次广告解锁该皮肤？ / Watch {0} ads to unlock this skin?
- `20006`: 广告播放失败或中断 / Ad failed or interrupted.
- `20010`: 默认皮肤 / Default Skin
- `20011`: 海洋皮肤 / Ocean Skin
- `20012`: 霓虹皮肤 / Neon Skin

## 5) Batch Config Tool

Menu:

- `CubeCrush/Skin/Create Test Skin Configs`

Creates:

- `Assets/Game/CubeCrush/Skins/SkinDatabase.asset`
- `Assets/Game/CubeCrush/Skins/Configs/Skin_1.asset`
- `Assets/Game/CubeCrush/Skins/Configs/Skin_2.asset`
- `Assets/Game/CubeCrush/Skins/Configs/Skin_3.asset`

Then replace sprites and prefab resource paths in inspector.
