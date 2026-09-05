"""Stage 3: skip the Table of Contents, then detect clause headings via a font-size-gated,
successor-numbering heuristic.

Skipping the Table of Contents (however many pages long, however irregularly it numbers annexes -
"Annex A:", "B.7.3 UMLClassDiagram [Class]", ...) is done by looking for pages dense with
dotted-leader entries ("... NN" trailing a page number) - a TOC's structural signature - rather
than by pattern-matching the entry text itself, which real annexes defeat.

Once inside the body, a line is treated as a clause heading candidate only when it (a) starts with
a dotted number (e.g. "9.3.2 Classifier") *and* (b) is set in a visibly larger font than the
document's body text - real headings in a typeset specification are styled distinctly from prose,
and without this gate a long technical document's numbered lists, table rows, and cross-references
produce a flood of false positives. A candidate is accepted as an actual heading only when its
number is also a plausible successor of the previously accepted heading number in a depth-first
traversal of the section tree (a child, a sibling, or a sibling of some ancestor).
"""

from __future__ import annotations

import re
import statistics

from spec_extract.models import Clause, Line, ReconstructedPage

_HEADING = re.compile(r"^(\d+(?:\.\d+)*)\s+(\S.*)$")
_TOC_TITLES = {"contents", "table of contents"}
_TOC_ENTRY = re.compile(r"\.{2,}\s*\d+\s*$")
_TOC_ENTRY_DENSITY_THRESHOLD = 0.3
_HEADING_FONT_SIZE_MARGIN = 0.5


def _parse_number(text: str) -> tuple[int, ...]:
    return tuple(int(part) for part in text.split("."))


def _is_successor(previous: tuple[int, ...], candidate: tuple[int, ...]) -> bool:
    if candidate == previous + (1,):
        return True
    for depth in range(1, len(previous) + 1):
        ancestor = previous[:depth]
        sibling = ancestor[:-1] + (ancestor[-1] + 1,)
        if candidate == sibling:
            return True
    return False


def _document_body_font_size(pages: list[ReconstructedPage]) -> float:
    """The most common word font size across the document - a proxy for "body text size"."""
    sizes = [word.size for page in pages for line in page.lines for word in line.words if word.size > 0]
    return statistics.mode(sizes) if sizes else 0.0


def _line_font_size(line: Line) -> float:
    sizes = [word.size for word in line.words if word.size > 0]
    return max(sizes) if sizes else 0.0


def _is_heading_shaped(line: Line, body_font_size: float) -> bool:
    """Whether `line` looks like a clause heading: dotted-number prefix and (when known) a larger font."""
    if not _HEADING.match(line.text.strip()):
        return False
    if body_font_size <= 0:
        return True  # no font-size data available - regex only
    return _line_font_size(line) > body_font_size + _HEADING_FONT_SIZE_MARGIN


def _flat_lines(pages: list[ReconstructedPage]) -> list[tuple[int, Line]]:
    return [(page.number, line) for page in pages for line in page.lines]


def _is_toc_entry(line: Line) -> bool:
    """Whether `line` has a Table of Contents entry's structural shape: "... title ..... NN"."""
    return bool(_TOC_ENTRY.search(line.text.strip()))


def _toc_entry_density(page: ReconstructedPage) -> float:
    lines = [line for line in page.lines if line.text.strip()]
    if not lines:
        return 0.0
    return sum(1 for line in lines if _is_toc_entry(line)) / len(lines)


def _find_toc_start_page(pages: list[ReconstructedPage]) -> int | None:
    """Return the index (into `pages`) of the page whose title is "Contents", if any."""
    for index, page in enumerate(pages):
        if any(line.text.strip().lower() in _TOC_TITLES for line in page.lines):
            return index
    return None


def _find_body_start(pages: list[ReconstructedPage]) -> int:
    """Return the flat-line index where the real body starts, skipping any Table of Contents.

    Scans pages after the "Contents" title page for the dotted-leader-entry density to drop -
    this is a structural signature of a TOC page, robust to however many pages it spans and however
    irregularly it numbers annexes, unlike matching the entry text against the heading pattern.
    """
    toc_page_index = _find_toc_start_page(pages)
    if toc_page_index is None:
        return 0

    body_page_index = next(
        (
            index
            for index in range(toc_page_index + 1, len(pages))
            if _toc_entry_density(pages[index]) < _TOC_ENTRY_DENSITY_THRESHOLD
        ),
        len(pages),
    )

    return sum(len(page.lines) for page in pages[:body_page_index])


def detect_clauses(pages: list[ReconstructedPage]) -> list[Clause]:
    """Walk reading-ordered pages and split them into numbered clauses."""
    body_font_size = _document_body_font_size(pages)
    flat_lines = _flat_lines(pages)
    start = _find_body_start(pages)

    clauses: list[Clause] = []
    current: Clause | None = None
    last_number: tuple[int, ...] | None = None

    for page_number, line in flat_lines[start:]:
        text = line.text.strip()
        match = _HEADING.match(text) if _is_heading_shaped(line, body_font_size) else None
        candidate_number = _parse_number(match.group(1)) if match else None

        is_heading = match is not None and (last_number is None or _is_successor(last_number, candidate_number))

        if is_heading:
            if current is not None:
                current.page_end = page_number
                clauses.append(current)
            current = Clause(
                number=match.group(1),
                title=match.group(2).strip(),
                page_start=page_number,
                page_end=page_number,
            )
            last_number = candidate_number
        elif current is not None:
            current.raw_lines.append(line)
            current.page_end = page_number

    if current is not None:
        clauses.append(current)

    return clauses
