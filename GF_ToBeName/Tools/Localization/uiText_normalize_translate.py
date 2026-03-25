#!/usr/bin/env python3
"""
Normalize Doc/Language/uiText.xls:
- Merge duplicate rows by English text (ignoring trailing ':' / '：' and whitespace)
- Re-number specific legacy ids (e.g. 200xx) back into <100 range
- Fill missing translations for known English phrases

IMPORTANT:
This script is intentionally conservative:
- It never overwrites a non-empty translation that differs from English
- It may treat "same as English" as missing for non-English columns (opt-in)
"""

import argparse
import os
import shutil
import sys

_SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
_PYDEPS_DIR = os.path.abspath(os.path.join(_SCRIPT_DIR, "..", "..", "..", "pydeps"))
if os.path.exists(_PYDEPS_DIR) and _PYDEPS_DIR not in sys.path:
    sys.path.insert(0, _PYDEPS_DIR)


def _import_xls():
    import xlrd  # type: ignore
    import xlwt  # type: ignore

    return xlrd, xlwt


def _as_int(x):
    try:
        if x is None:
            return None
        if isinstance(x, (int, float)):
            return int(float(x))
        s = str(x).strip()
        if s == "":
            return None
        return int(float(s))
    except Exception:
        return None


def _norm_english(s: str) -> str:
    t = (s or "").strip()
    while t.endswith(":") or t.endswith("："):
        t = t[:-1].rstrip()
    return " ".join(t.split()).lower()


def _is_blank_cell(v) -> bool:
    if v is None:
        return True
    if not isinstance(v, str):
        return False
    s = v.strip()
    if s == "":
        return True
    if "<NoKey>" in s:
        return True
    return False


def _should_fill_cell(v, english: str, treat_same_as_english_missing: bool) -> bool:
    if _is_blank_cell(v):
        return True
    if treat_same_as_english_missing and isinstance(v, str):
        if v.strip() == (english or "").strip():
            return True
    return False


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--xls", required=True, help="Path to Doc/Language/uiText.xls")
    parser.add_argument(
        "--treat-same-as-english-missing",
        action="store_true",
        help="Consider non-English cells equal to English as missing (useful to undo prior English fallback).",
    )
    args = parser.parse_args()

    xlrd, xlwt = _import_xls()

    xls_path = args.xls
    treat_same = bool(args.treat_same_as_english_missing)

    wb = xlrd.open_workbook(xls_path)
    sh = wb.sheet_by_index(0)
    nrows, ncols = sh.nrows, sh.ncols
    header = [sh.cell_value(0, i) for i in range(ncols)]

    if "id" not in header:
        raise RuntimeError("Could not find 'id' column")
    if "strEnglish" not in header:
        raise RuntimeError("Could not find 'strEnglish' column")

    id_col = header.index("id")
    en_col = header.index("strEnglish")

    lang_cols = {}
    for i, h in enumerate(header):
        if isinstance(h, str) and h.startswith("str") and h not in ("strText", "strText "):
            # example: strChineseSimplified -> ChineseSimplified
            lang_key = h[3:]
            lang_cols[lang_key] = i

    # Translation dictionary for phrases currently used in code and recently added.
    # Keys are EXACT English strings from strEnglish.
    TR = {
        "Ad failed or interrupted.": {
            "ChineseSimplified": "广告失败或被中断。",
            "ChineseTraditional": "廣告失敗或被中斷。",
            "German": "Werbung fehlgeschlagen oder abgebrochen.",
            "Japanese": "広告の再生に失敗、または中断されました。",
            "Korean": "광고가 실패했거나 중단되었습니다.",
            "Vietnamese": "Quảng cáo thất bại hoặc bị gián đoạn.",
            "Thai": "โฆษณาล้มเหลวหรือถูกขัดจังหวะ",
            "Indonesian": "Gagal memutar iklan atau dibatalkan.",
            "French": "La pub a échoué ou a été interrompue.",
            "Spanish": "El anuncio falló o fue interrumpido.",
            "Russian": "Реклама не удалась или была прервана.",
            "PortuguesePortugal": "O anúncio falhou ou foi interrompido.",
        },
        "Failed": {
            "ChineseSimplified": "失败",
            "ChineseTraditional": "失敗",
            "German": "Fehlgeschlagen",
            "Japanese": "失敗",
            "Korean": "실패",
            "Vietnamese": "Thất bại",
            "Thai": "ล้มเหลว",
            "Indonesian": "Gagal",
            "French": "Échec",
            "Spanish": "Fallido",
            "Russian": "Неудача",
            "PortuguesePortugal": "Falhou",
        },
        "Try again or watch an ad to revive.": {
            "ChineseSimplified": "再试一次，或观看广告复活。",
            "ChineseTraditional": "再試一次，或觀看廣告復活。",
            "German": "Versuche es erneut oder schau eine Werbung, um wiederzubeleben.",
            "Japanese": "もう一度挑戦するか、広告を見て復活しましょう。",
            "Korean": "다시 시도하거나 광고를 보고 부활하세요.",
            "Vietnamese": "Thử lại hoặc xem quảng cáo để hồi sinh.",
            "Thai": "ลองใหม่หรือดูโฆษณาเพื่อชุบชีวิต",
            "Indonesian": "Coba lagi atau tonton iklan untuk menyelamatkan.",
            "French": "Réessaie ou regarde une pub pour revivre.",
            "Spanish": "Inténtalo de nuevo o mira un anuncio para revivir.",
            "Russian": "Попробуйте снова или посмотрите рекламу, чтобы ожить.",
            "PortuguesePortugal": "Tenta novamente ou vê um anúncio para reanimar.",
        },
        "Watch Ad Revive": {
            "ChineseSimplified": "观看广告复活",
            "ChineseTraditional": "觀看廣告復活",
            "German": "Werbung ansehen & wiederbeleben",
            "Japanese": "広告を見て復活",
            "Korean": "광고 보고 부활",
            "Vietnamese": "Xem quảng cáo để hồi sinh",
            "Thai": "ดูโฆษณาเพื่อชุบชีวิต",
            "Indonesian": "Tonton Iklan untuk Bangkit",
            "French": "Voir une pub pour revivre",
            "Spanish": "Ver anuncio para revivir",
            "Russian": "Смотреть рекламу — ожить",
            "PortuguesePortugal": "Ver anúncio para reanimar",
        },
        "Ad unavailable. Restarting...": {
            "ChineseSimplified": "广告不可用，正在重开…",
            "ChineseTraditional": "廣告不可用，正在重開…",
            "German": "Werbung nicht verfügbar. Neustart...",
            "Japanese": "広告が利用できません。再開します…",
            "Korean": "광고를 사용할 수 없습니다. 재시작합니다…",
            "Vietnamese": "Không có quảng cáo. Đang khởi động lại…",
            "Thai": "ไม่มีโฆษณา กำลังเริ่มใหม่…",
            "Indonesian": "Iklan tidak tersedia. Memulai ulang...",
            "French": "Pub indisponible. Redémarrage…",
            "Spanish": "Anuncio no disponible. Reiniciando…",
            "Russian": "Реклама недоступна. Перезапуск…",
            "PortuguesePortugal": "Anúncio indisponível. A reiniciar…",
        },
        "Revive failed.": {
            "ChineseSimplified": "复活失败。",
            "ChineseTraditional": "復活失敗。",
            "German": "Wiederbelebung fehlgeschlagen.",
            "Japanese": "復活に失敗しました。",
            "Korean": "부활에 실패했습니다.",
            "Vietnamese": "Hồi sinh thất bại.",
            "Thai": "ชุบชีวิตไม่สำเร็จ",
            "Indonesian": "Gagal bangkit.",
            "French": "Échec de la résurrection.",
            "Spanish": "Falló la reanimación.",
            "Russian": "Не удалось ожить.",
            "PortuguesePortugal": "Falha ao reanimar.",
        },
        "Daily Sign-In": {
            "ChineseSimplified": "每日签到",
            "ChineseTraditional": "每日簽到",
            "German": "Tägliche Anmeldung",
            "Japanese": "デイリーログイン",
            "Korean": "일일 출석",
            "Vietnamese": "Điểm danh hằng ngày",
            "Thai": "เช็คอินรายวัน",
            "Indonesian": "Tanda Harian",
            "French": "Connexion quotidienne",
            "Spanish": "Inicio de sesión diario",
            "Russian": "Ежедневный вход",
            "PortuguesePortugal": "Entrada diária",
        },
        "Current streak: {0} days": {
            "ChineseSimplified": "当前连续签到：{0}天",
            "ChineseTraditional": "目前連續簽到：{0}天",
            "German": "Aktuelle Serie: {0} Tage",
            "Japanese": "連続日数：{0}日",
            "Korean": "현재 연속: {0}일",
            "Vietnamese": "Chuỗi hiện tại: {0} ngày",
            "Thai": "สตรีคปัจจุบัน: {0} วัน",
            "Indonesian": "Rangkaian saat ini: {0} hari",
            "French": "Série actuelle : {0} jours",
            "Spanish": "Racha actual: {0} días",
            "Russian": "Текущая серия: {0} дн.",
            "PortuguesePortugal": "Sequência atual: {0} dias",
        },
        "Reward: unlock first skin after 3-day streak": {
            "ChineseSimplified": "奖励：连续签到3天解锁第一个皮肤",
            "ChineseTraditional": "獎勵：連續簽到3天解鎖第一個皮膚",
            "German": "Belohnung: Ersten Skin nach 3 Tagen Serie freischalten",
            "Japanese": "報酬：3日連続で最初のスキンを解放",
            "Korean": "보상: 3일 연속 시 첫 스킨 해금",
            "Vietnamese": "Thưởng: mở skin đầu tiên sau chuỗi 3 ngày",
            "Thai": "รางวัล: ปลดล็อกสกินแรกเมื่อเช็คอินครบ 3 วันติด",
            "Indonesian": "Hadiah: buka skin pertama setelah rangkaian 3 hari",
            "French": "Récompense : déverrouille le premier skin après 3 jours d’affilée",
            "Spanish": "Recompensa: desbloquea la primera skin tras 3 días seguidos",
            "Russian": "Награда: откройте первый скин за серию 3 дня",
            "PortuguesePortugal": "Recompensa: desbloqueia o primeiro skin após 3 dias seguidos",
        },
        "Already signed today.": {
            "ChineseSimplified": "今天已经签到过了。",
            "ChineseTraditional": "今天已經簽到過了。",
            "German": "Heute bereits angemeldet.",
            "Japanese": "今日はすでに受け取りました。",
            "Korean": "오늘은 이미 출석했습니다.",
            "Vietnamese": "Hôm nay bạn đã điểm danh rồi.",
            "Thai": "วันนี้เช็คอินแล้ว",
            "Indonesian": "Sudah tanda hari ini.",
            "French": "Déjà validé aujourd’hui.",
            "Spanish": "Ya has marcado hoy.",
            "Russian": "Сегодня уже отмечено.",
            "PortuguesePortugal": "Já fez hoje.",
        },
        "3-day streak reward unlocked! Skin has been unlocked!": {
            "ChineseSimplified": "连续签到3天奖励已解锁！皮肤已解锁！",
            "ChineseTraditional": "連續簽到3天獎勵已解鎖！皮膚已解鎖！",
            "German": "Belohnung für 3 Tage Serie freigeschaltet! Skin freigeschaltet!",
            "Japanese": "3日連続報酬を獲得！スキンを解放しました！",
            "Korean": "3일 연속 보상 획득! 스킨이 해금되었습니다!",
            "Vietnamese": "Đã mở thưởng chuỗi 3 ngày! Skin đã được mở!",
            "Thai": "ปลดล็อกรางวัลสตรีค 3 วันแล้ว! ปลดล็อคสกินแล้ว!",
            "Indonesian": "Hadiah 3 hari beruntun diterima! Skin sudah dibuka!",
            "French": "Récompense 3 jours débloquée ! Skin débloqué !",
            "Spanish": "¡Recompensa de 3 días desbloqueada! ¡Skin desbloqueada!",
            "Russian": "Награда за 3 дня получена! Скин открыт!",
            "PortuguesePortugal": "Recompensa de 3 dias desbloqueada! Skin desbloqueado!",
        },
        "Level Target": {
            "ChineseSimplified": "关卡目标",
            "ChineseTraditional": "關卡目標",
            "German": "Levelziel",
            "Japanese": "レベル目標",
            "Korean": "레벨 목표",
            "Vietnamese": "Mục tiêu màn",
            "Thai": "เป้าหมายด่าน",
            "Indonesian": "Target Level",
            "French": "Objectif du niveau",
            "Spanish": "Objetivo del nivel",
            "Russian": "Цель уровня",
            "PortuguesePortugal": "Objetivo do nível",
        },
        "Score >= {0}": {
            "ChineseSimplified": "分数 ≥ {0}",
            "ChineseTraditional": "分數 ≥ {0}",
            "German": "Punkte ≥ {0}",
            "Japanese": "スコア ≥ {0}",
            "Korean": "점수 ≥ {0}",
            "Vietnamese": "Điểm ≥ {0}",
            "Thai": "คะแนน ≥ {0}",
            "Indonesian": "Skor minimal {0}",
            "French": "Score ≥ {0}",
            "Spanish": "Puntuación ≥ {0}",
            "Russian": "Счёт ≥ {0}",
            "PortuguesePortugal": "Pontuação ≥ {0}",
        },
        "Glove": {
            "ChineseSimplified": "手套",
            "ChineseTraditional": "手套",
            "German": "Handschuh",
            "Japanese": "手袋",
            "Korean": "장갑",
            "Vietnamese": "Găng tay",
            "Thai": "ถุงมือ",
            "Indonesian": "Sarung Tangan",
            "French": "Gant",
            "Spanish": "Guante",
            "Russian": "Перчатка",
            "PortuguesePortugal": "Luva",
        },
        "Star": {
            "ChineseSimplified": "星星",
            "ChineseTraditional": "星星",
            "German": "Stern",
            "Japanese": "星",
            "Korean": "별",
            "Vietnamese": "Ngôi sao",
            "Thai": "ดาว",
            "Indonesian": "Bintang",
            "French": "Étoile",
            "Spanish": "Estrella",
            "Russian": "Звезда",
            "PortuguesePortugal": "Estrela",
        },
        "Gem": {
            "ChineseSimplified": "宝石",
            "ChineseTraditional": "寶石",
            "German": "Edelstein",
            "Japanese": "宝石",
            "Korean": "보석",
            "Vietnamese": "Đá quý",
            "Thai": "อัญมณี",
            "Indonesian": "Permata",
            "French": "Gemme",
            "Spanish": "Gema",
            "Russian": "Самоцвет",
            "PortuguesePortugal": "Gema",
        },
        "Watch {0} ads to unlock this skin?": {
            "ChineseSimplified": "观看{0}次广告解锁该皮肤？",
            "ChineseTraditional": "觀看{0}次廣告解鎖該皮膚？",
            "German": "Sieh dir {0} Werbungen an, um diesen Skin freizuschalten?",
            "Japanese": "広告を{0}回見てこのスキンを解放しますか？",
            "Korean": "광고 {0}회 시청으로 이 스킨을 해금할까요?",
            "Vietnamese": "Xem {0} quảng cáo để mở skin này?",
            "Thai": "ดูโฆษณา {0} ครั้งเพื่อปลดล็อกสกินนี้ไหม?",
            "Indonesian": "Tonton {0} iklan untuk membuka skin ini?",
            "French": "Regarder {0} pubs pour débloquer ce skin ?",
            "Spanish": "¿Ver {0} anuncios para desbloquear esta skin?",
            "Russian": "Посмотреть {0} реклам(ы), чтобы открыть этот скин?",
            "PortuguesePortugal": "Ver {0} anúncios para desbloquear este skin?",
        },
    }

    # Read rows
    rows = []
    for r in range(1, nrows):
        kid = _as_int(sh.cell_value(r, id_col))
        english = sh.cell_value(r, en_col)
        row = {
            "_row": r,
            "id": kid,
            "english": english if isinstance(english, str) else ("" if english is None else str(english)),
            "cells": [sh.cell_value(r, c) for c in range(ncols)],
        }
        rows.append(row)

    # Merge duplicates by normalized English (ignore trailing ':' and whitespace)
    canon_by_norm = {}
    removed_rows = set()
    for row in rows:
        en = row["english"]
        if not isinstance(en, str) or en.strip() == "":
            continue
        norm = _norm_english(en)
        if norm == "":
            continue
        if norm not in canon_by_norm:
            canon_by_norm[norm] = row
            continue
        canon = canon_by_norm[norm]
        # Choose canonical row: prefer smaller id, and prefer <100 when tie-ish
        cid, rid = canon.get("id"), row.get("id")
        prefer_new = False
        if cid is None and rid is not None:
            prefer_new = True
        elif cid is not None and rid is not None:
            if (cid >= 100 and rid < 100) or (rid < cid):
                prefer_new = True

        if prefer_new:
            # swap canonical
            canon_by_norm[norm] = row
            # merge old canon into new row
            src, dst = canon, row
            removed_rows.add(src["_row"])
        else:
            src, dst = row, canon
            removed_rows.add(src["_row"])

        # merge translation cells: only fill if dst is blank or equals English (when treat_same)
        dst_en = dst["english"]
        for lk, ci in lang_cols.items():
            if lk == "English":
                continue
            dst_v = dst["cells"][ci]
            src_v = src["cells"][ci]
            if _should_fill_cell(dst_v, dst_en, treat_same):
                # take src only if it is meaningful
                if not _is_blank_cell(src_v) and (not (isinstance(src_v, str) and src_v.strip() == dst_en.strip())):
                    dst["cells"][ci] = src_v

    kept = [r for r in rows if r["_row"] not in removed_rows]

    # Remap ids back into <100 for specific legacy ids (200xx that are referenced by code)
    id_map = {
        20006: 75,
        20021: 76,
        20022: 77,
        20023: 78,
        20024: 79,
        20026: 80,
        20030: 81,
        20031: 82,
        20032: 83,
        20033: 84,
        20034: 85,
        20040: 86,
        20041: 87,
        20060: 88,
        20061: 89,
        20062: 90,
        # 20042 / 20043 are duplicates and should be merged away to 57/56 by English content.
    }

    # Ensure target ids are not already used by a different row
    used_ids = {}
    for r in kept:
        kid = r.get("id")
        if kid is not None:
            used_ids[kid] = r
    for old_id, new_id in id_map.items():
        if new_id in used_ids and new_id != old_id:
            # If the target id exists, we won't overwrite it here.
            raise RuntimeError(f"Target id {new_id} already exists; cannot remap {old_id} -> {new_id}")

    for r in kept:
        kid = r.get("id")
        if kid in id_map:
            r["id"] = id_map[kid]

    # Fill translations for blanks / <NoKey> (and optionally same-as-English)
    for r in kept:
        en = r["english"] if isinstance(r["english"], str) else ""
        if en.strip() == "":
            continue
        if en not in TR:
            continue
        for lk, ci in lang_cols.items():
            if lk == "English":
                continue
            if _should_fill_cell(r["cells"][ci], en, treat_same):
                trv = TR[en].get(lk)
                if trv:
                    r["cells"][ci] = trv

    # Rebuild workbook: header + kept rows sorted by id (id None goes last)
    kept_sorted = sorted(kept, key=lambda x: (999999 if x.get("id") is None else int(x.get("id"))))

    backup_path = xls_path + ".bak_normalize"
    if not os.path.exists(backup_path):
        shutil.copy2(xls_path, backup_path)

    out_tmp = xls_path + ".tmp_normalize"
    out_wb = xlwt.Workbook()
    out_sh = out_wb.add_sheet(sh.name)

    # write header
    for c in range(ncols):
        out_sh.write(0, c, sh.cell_value(0, c))

    # write rows
    out_r = 1
    for r in kept_sorted:
        cells = list(r["cells"])
        # set id and english consistently
        cells[id_col] = r.get("id") if r.get("id") is not None else ""
        cells[en_col] = r.get("english") or ""
        for c in range(ncols):
            out_sh.write(out_r, c, cells[c])
        out_r += 1

    out_wb.save(out_tmp)
    shutil.move(out_tmp, xls_path)

    print(
        f"[uiText_normalize_translate] removed_duplicates={len(removed_rows)} "
        f"rows_before={len(rows)} rows_after={len(kept_sorted)}"
    )
    print(
        f"[uiText_normalize_translate] applied_id_map_count={len(id_map)} "
        f"treat_same_as_english_missing={treat_same}"
    )


if __name__ == "__main__":
    main()
