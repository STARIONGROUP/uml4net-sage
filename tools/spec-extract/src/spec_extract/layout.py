"""Stage 2: reconstruct reading order and strip running headers/footers."""

from __future__ import annotations

import re
from collections import Counter

from spec_extract.models import Line, PositionedPage, PositionedWord, ReconstructedPage

_LINE_TOLERANCE = 3.0
_HEADER_BAND = 45.0
_FOOTER_BAND = 45.0
_MIN_REPEAT_COUNT = 3
_MIN_REPEAT_FRACTION = 0.3
_WHITESPACE = re.compile(r"\s+")


def _normalize(text: str) -> str:
    return _WHITESPACE.sub(" ", text).strip().lower()


def _group_into_lines(words: tuple[PositionedWord, ...], tolerance: float) -> list[Line]:
    if not words:
        return []
    ordered = sorted(words, key=lambda word: word.top)
    clusters: list[list[PositionedWord]] = []
    for word in ordered:
        placed = False
        for cluster in clusters:
            if abs(cluster[0].top - word.top) <= tolerance:
                cluster.append(word)
                placed = True
                break
        if not placed:
            clusters.append([word])
    lines = [
        Line(top=min(w.top for w in cluster), words=tuple(sorted(cluster, key=lambda w: w.x0))) for cluster in clusters
    ]
    return sorted(lines, key=lambda line: line.top)


def _is_bare_number(text: str) -> bool:
    return _normalize(text).isdigit()


def _find_repeated_band_text(
    pages: list[PositionedPage], lines_by_page: dict[int, list[Line]], *, band: str
) -> tuple[set[str], bool]:
    """Return (repeated normalized texts, numeric-footer-detected) for the header or footer band."""
    total_pages = len(pages)
    counter: Counter[str] = Counter()
    numeric_band_count = 0
    for page in pages:
        for line in lines_by_page[page.number]:
            normalized = _normalize(line.text)
            if not normalized:
                continue
            in_band = line.top <= _HEADER_BAND if band == "header" else line.top >= page.height - _FOOTER_BAND
            if not in_band:
                continue
            counter[normalized] += 1
            if band == "footer" and _is_bare_number(line.text):
                numeric_band_count += 1

    threshold = max(_MIN_REPEAT_COUNT, int(total_pages * _MIN_REPEAT_FRACTION))
    repeated = {text for text, count in counter.items() if count >= threshold}
    # A bare page number changes on every page, so it never repeats by exact text - detect it
    # by "most pages have *some* bare number in this band" instead.
    numeric_footer_detected = band == "footer" and numeric_band_count >= threshold
    return repeated, numeric_footer_detected


def reconstruct_pages(pages: list[PositionedPage]) -> list[ReconstructedPage]:
    """Turn raw positioned words into reading-ordered lines, with running headers/footers stripped."""
    lines_by_page = {page.number: _group_into_lines(page.words, _LINE_TOLERANCE) for page in pages}
    repeated_header, _ = _find_repeated_band_text(pages, lines_by_page, band="header")
    repeated_footer, numeric_footer = _find_repeated_band_text(pages, lines_by_page, band="footer")

    reconstructed: list[ReconstructedPage] = []
    for page in pages:
        kept = []
        for line in lines_by_page[page.number]:
            normalized = _normalize(line.text)
            in_footer_band = line.top >= page.height - _FOOTER_BAND
            if normalized in repeated_header or normalized in repeated_footer:
                continue
            if numeric_footer and in_footer_band and _is_bare_number(line.text):
                continue
            kept.append(line)
        reconstructed.append(ReconstructedPage(number=page.number, lines=tuple(kept)))
    return reconstructed
