"""Stage 4: group a clause's raw lines into paragraphs and split normative from informative."""

from __future__ import annotations

import re
import statistics

from spec_extract.models import Clause, Line, Paragraph

_NORMATIVE_WORDS = re.compile(r"\bshall\b|\bshall not\b|\bmust\b")
_NOTE_PREFIX = re.compile(r"^NOTE\b")
_EXAMPLE_PREFIX = re.compile(r"^EXAMPLE\b")
_GAP_MULTIPLIER = 1.5


def _group_paragraphs(lines: list[Line]) -> list[list[Line]]:
    if not lines:
        return []

    gaps = [second.top - first.top for first, second in zip(lines, lines[1:]) if second.top > first.top]
    typical_gap = statistics.median(gaps) if gaps else 0.0

    paragraphs: list[list[Line]] = [[lines[0]]]
    for previous, line in zip(lines, lines[1:]):
        gap = line.top - previous.top
        starts_new = (
            (typical_gap > 0 and gap > typical_gap * _GAP_MULTIPLIER)
            or _NOTE_PREFIX.match(line.text)
            or _EXAMPLE_PREFIX.match(line.text)
        )
        if starts_new:
            paragraphs.append([line])
        else:
            paragraphs[-1].append(line)
    return paragraphs


def _classify(text: str) -> Paragraph:
    informative_kind = "note" if _NOTE_PREFIX.match(text) else "example" if _EXAMPLE_PREFIX.match(text) else None
    is_normative = informative_kind is None and _NORMATIVE_WORDS.search(text) is not None
    return Paragraph(text=text, is_normative=is_normative, informative_kind=informative_kind)


def split_normative(clause: Clause) -> Clause:
    """Populate `clause.paragraphs` from `clause.raw_lines`, tagging each as normative/informative."""
    for group in _group_paragraphs(clause.raw_lines):
        text = " ".join(line.text for line in group)
        clause.paragraphs.append(_classify(text))
    return clause
