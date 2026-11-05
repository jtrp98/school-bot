---
name: i18n-strings
description: Add, change or remove UI text in the school-bot frontend the way AGENTS.md and tests/i18n.test.mjs require — structural keys in th.json, en.json and zh.json together, matching {{placeholders}}, no Thai/Chinese literals in TS/TSX, no sentences glued from several t() calls — then run the i18n test. Use whenever a label, title, placeholder, tooltip, toast, dialog text or aria-label changes.
---

# UI text and i18n (school-bot frontend)

## Where text lives

- Catalogs: `app/frontend/src/i18n/locales/th.json`, `en.json`, `zh.json`. They are nested JSON with the same top-level namespaces (`installer`, `nav`, `language`, `profile`, `shell`, `settings`, `robot`, `common`, `lessons`, `modal`, `window`, `home`, `blocks`, `adventure`, `entry`). Each catalog had 1,633 leaf keys on 2026-11-02.
- Setup: `app/frontend/src/i18n/index.ts`. Use the `t()` exported there, or `useTranslation()` / `i18nKey=` with `RichText` for tagged sentences. Thai is the fallback language. The chosen language is stored as `school-bot:lang`.

## Rules (enforced by `tests/i18n.test.mjs` unless noted)

1. Add or change every key in all three catalogs in the same change. Keys and `{{placeholder}}` names must match exactly, and no value may be empty.
2. Keys are structural paths such as `blocks.toolbar.upload.label` or `robot.connect.status.title`. Never use a sentence, Thai text or whitespace as a key. Reuse an existing key when its meaning matches, and search the catalogs first.
3. Name placeholders after their meaning (`robotName`, `poseCount`, `portName`), never `value0`.
4. No Thai or Chinese text in `src/**/*.ts(x)` outside the catalogs. The only exceptions are files listed in `SAMPLE_DATA_FILES` in the test, and only for Sample-mode fixture data. Remove a file's entry when it no longer needs one, because the test fails on stale entries.
5. Never build a sentence by joining `t()` results with `+` or nested ternaries. Give each condition its own full-sentence key and choose the key in the component or in a small named helper.
6. Every literal `t("…")`, `translate("…")` or `i18nKey="…"` must exist in the catalogs. A key computed at runtime is not checked, so prefer literal keys.
7. Not UI text, so never translate it: user-entered names (projects, poses, robots), code, device and engine diagnostics, and source history (AGENTS.md).
8. Accessibility strings (`aria-label`, `title`) are UI text and follow the same rules.

## Steps

1. Search before adding: run `rg -n "\"<word>\"" app/frontend/src/i18n/locales/en.json` and grep the key prefix you plan to use.
2. Place the key under the namespace of its screen or section, for example `blocks.<area>.<element>.<role>`.
3. Edit all three catalogs. Write natural Thai, English and Simplified Chinese. If you are unsure of the Chinese wording, say so in the handoff instead of guessing silently.
4. Use the key in TSX through `t()` or `RichText`.
5. From `app/frontend`, run `npm run test:i18n`. It needs `node_modules`; if they are missing, ask before running `npm ci`. Run `npm run typecheck` too when TS changed.
6. In the Thai handoff, list the keys you added or changed.

## Block Lab note

Mixly block labels (about 1,800 `Blockly.MIXLY_*` messages per language upstream) are a special case. Moving them into these catalogs would roughly double their size. How Block Lab sources block-label translations is an open design decision, so check `.claude/notes/` before choosing an approach.
