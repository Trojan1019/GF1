#!/usr/bin/env python3
"""
Append new localization rows into Doc/Language/uiText.xls.

By default, this script appends new rows at the end and assigns sequential IDs.
If a key object contains an explicit `"id"`, that ID will be used and the script
will overwrite the existing row if it already exists.

Workflow:
1) Prepare a JSON file containing a list of keys to append.
2) Run this script, it will:
   - detect next sequential id
   - append rows after current last row (for keys without explicit "id")
   - overwrite existing rows when "id" is explicit and already exists
   - fill all language columns with provided values (or fallback to English to avoid blanks)
   - create a backup beside uiText.xls before writing
3) The script prints a mapping of generated ids -> key content.

NOTE:
This tool is intended for the coding workflow (agent-side), not for runtime.
It requires python packages: xlrd and xlwt.
"""

import argparse
import json
import os
import shutil
import sys

# Ensure local python dependencies are discoverable.
# We install xlrd/xlwt into ./pydeps to avoid touching global site-packages.
_SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
_PYDEPS_DIR = os.path.abspath(os.path.join(_SCRIPT_DIR, "..", "..", "..", "pydeps"))
if os.path.exists(_PYDEPS_DIR) and _PYDEPS_DIR not in sys.path:
    sys.path.insert(0, _PYDEPS_DIR)


def _import_xls():
    try:
        import xlrd  # type: ignore
        import xlwt  # type: ignore
    except ModuleNotFoundError as e:
        print(
            "[uiText_append_keys] Missing dependencies. Please install xlrd/xlwt into your python environment.",
            file=sys.stderr,
        )
        raise
    return xlrd, xlwt


def _as_int(x):
    if x is None:
        return None
    try:
        if isinstance(x, (int, float)):
            return int(float(x))
        return int(str(x).strip())
    except Exception:
        return None


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--xls", required=True, help="Path to Doc/Language/uiText.xls")
    parser.add_argument(
        "--input",
        required=True,
        help="Path to JSON file: {\"keys\":[{\"id\":200xx?,\"english\":\"...\",\"indonesian\":\"...\",...}] }",
    )
    parser.add_argument(
        "--fallback-to-english",
        action="store_true",
        help="Allow filling missing non-English columns with the English value. Default: OFF (strict).",
    )
    args = parser.parse_args()

    xlrd, xlwt = _import_xls()

    xls_path = args.xls
    input_path = args.input
    fallback_to_english = bool(getattr(args, "fallback_to_english", False))

    if not os.path.exists(xls_path):
        raise FileNotFoundError(xls_path)
    if not os.path.exists(input_path):
        raise FileNotFoundError(input_path)

    with open(input_path, "r", encoding="utf-8") as f:
        payload = json.load(f)

    keys = payload.get("keys", [])
    if not isinstance(keys, list) or len(keys) == 0:
        raise ValueError("input json must contain non-empty list under keys")

    wb = xlrd.open_workbook(xls_path)
    sh = wb.sheet_by_index(0)

    nrows = sh.nrows
    ncols = sh.ncols

    header = [sh.cell_value(0, i) for i in range(ncols)]
    id_col = None
    lang_cols = {}  # langKey -> colIndex

    for i, h in enumerate(header):
        if isinstance(h, str) and h.strip().lower() == "id":
            id_col = i
        if isinstance(h, str) and h.startswith("str") and h != "strText" and h != "strEnglish" and h != "strText ":
            # header like strIndonesian
            lang_key = h[3:]
            lang_cols[lang_key] = i

    if id_col is None:
        raise RuntimeError("Could not find 'id' column in uiText.xls header")

    # english column might exist as strEnglish
    en_col = None
    for i, h in enumerate(header):
        if h == "strEnglish":
            en_col = i
            break
    if en_col is None:
        raise RuntimeError("Could not find 'strEnglish' column")

    # Build a mapping of langKey -> colIndex, including English
    all_lang_cols = {"English": en_col}
    for lk, ci in lang_cols.items():
        all_lang_cols[lk] = ci

    # Determine next id (used when a key doesn't provide explicit "id")
    max_id = -1
    for r in range(1, nrows):
        idv = _as_int(sh.cell_value(r, id_col))
        if idv is not None:
            max_id = max(max_id, idv)
    next_id = max_id + 1

    # Existing id -> row index, so explicit ids can overwrite instead of duplicating.
    existing_id_to_row = {}
    for r in range(1, nrows):
        idv = _as_int(sh.cell_value(r, id_col))
        if idv is not None:
            existing_id_to_row[idv] = r

    backup_path = xls_path + ".bak_append_keys"
    if not os.path.exists(backup_path):
        shutil.copy2(xls_path, backup_path)

    out_tmp = xls_path + ".tmp_append_keys"
    out_wb = xlwt.Workbook()
    out_sh = out_wb.add_sheet(sh.name)

    # If an explicit "id" already exists, we will overwrite that entire row.
    # xlwt doesn't allow overwriting individual cells, so we must skip copying those rows first.
    explicit_ids = set()
    for k in keys:
        if isinstance(k, dict):
            explicit_id = _as_int(k.get("id", None))
            if explicit_id is not None:
                explicit_ids.add(explicit_id)
    overwrite_rows = {existing_id_to_row[i] for i in explicit_ids if i in existing_id_to_row}

    # copy all existing cells except rows we plan to overwrite
    for r in range(nrows):
        if r in overwrite_rows:
            continue
        for c in range(ncols):
            out_sh.write(r, c, sh.cell_value(r, c))

    generated = []
    appended_row_start = nrows
    append_cursor = 0

    # Append / overwrite keys
    for idx, k in enumerate(keys):
        if not isinstance(k, dict):
            continue
        english = k.get("english", "")
        if english is None:
            english = ""

        indonesia = k.get("indonesian", "")
        if indonesia is None:
            indonesia = ""

        explicit_id = _as_int(k.get("id", None))
        gen_id = explicit_id if explicit_id is not None else (next_id + idx)

        # decide row index
        if gen_id in existing_id_to_row:
            row = existing_id_to_row[gen_id]
        else:
            row = appended_row_start + append_cursor
            append_cursor += 1

        # always set english column (for overwrites too)
        out_sh.write(row, id_col, gen_id)
        out_sh.write(row, en_col, english)

        # fill language columns
        for lk, ci in all_lang_cols.items():
            if lk == "English":
                continue
            provided = k.get(lk.lower(), None)
            if lk.lower() == "indonesian":
                provided = indonesia

            # Provided mapping might be given as Indonesian/ChineseSimplified/... in different keys.
            # Try exact key variants.
            if provided is None:
                provided = k.get(lk, None)

            if provided is None or (isinstance(provided, str) and provided.strip() == ""):
                if fallback_to_english:
                    # Explicitly allowed legacy behavior.
                    provided = english
                else:
                    raise ValueError(
                        f"[uiText_append_keys] Missing translation for id={gen_id}, lang={lk}. "
                        f"Provide '{lk}' in input JSON or use '--fallback-to-english'."
                    )

            out_sh.write(row, ci, provided)

        generated.append({"id": gen_id, "english": english, "indonesian": indonesia})

    out_wb.save(out_tmp)
    shutil.move(out_tmp, xls_path)

    print(json.dumps(generated, ensure_ascii=False, indent=2))


if __name__ == "__main__":
    main()

