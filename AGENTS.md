## GF1 Agent Project Conventions (Must Follow)

This repository has **mandatory** engineering and localization conventions.
Any AI agent/model working in this repo must read and follow these rules **before** making changes.

### Source of Truth (Cursor Rules)

- `/.cursor/rules/gf1-core.mdc`
- `/.cursor/rules/gf1-localization-uiText.mdc`

If there is any conflict between documents, **Cursor rules are the source of truth**.

### Non‑negotiables (Quick Summary)

- **Addressing**: Always address the user as **“Trojan大人”** in all replies.
- **Framework-first**: Use existing project systems for UI/events/pooling/persistence/audio/effects.
- **No meaningless singleton null guards**: Don’t add `if (X.Instance == null) return;` style code unless there is a proven lifecycle race.
- **UI tweening**: For UI movement, use `RectTransform.anchoredPosition`-based tweens (`DOAnchorPos*`), not world-space movement.
- **Localization (`uiText.xls`)**:
  - All UI text must come from `Doc/Language/uiText.xls`.
  - Only fill **blank / `<NoKey>`** cells; do **not** overwrite existing translations unless explicitly requested.
  - **No English fallback** for missing translations.
  - Keep ids **< 100** when feasible; avoid introducing `20000+`.
  - Merge duplicate rows (same English meaning; ignore trailing `:` / `：`) and update code references accordingly.

