from spec_extract.clauses import detect_clauses
from spec_extract.models import Line, PositionedWord, ReconstructedPage

_BODY_SIZE = 10.0
_HEADING_SIZE = 14.0


def _line(text: str, top: float, size: float = _BODY_SIZE) -> Line:
    word = PositionedWord(
        text=text, x0=72, x1=72 + len(text) * 6, top=top, bottom=top + 10, font_name="Helvetica", size=size
    )
    return Line(top=top, words=(word,))


def _heading(text: str, top: float) -> Line:
    return _line(text, top, size=_HEADING_SIZE)


def test_detect_clauses_accepts_a_plausible_successor_sequence() -> None:
    pages = [
        ReconstructedPage(
            number=1,
            lines=(
                _heading("1 Scope", 100),
                _line("This clause defines scope.", 120),
                _line("It has a second sentence too.", 140),
                _heading("1.1 Overview", 160),
                _line("This is the overview body.", 180),
                _line("It also has a second sentence.", 200),
                _heading("2 Conformance", 220),
                _line("This is the conformance body.", 240),
                _line("It also has a second sentence.", 260),
            ),
        )
    ]

    clauses = detect_clauses(pages)

    assert [clause.number for clause in clauses] == ["1", "1.1", "2"]
    assert clauses[0].title == "Scope"
    assert [line.text for line in clauses[0].raw_lines] == [
        "This clause defines scope.",
        "It has a second sentence too.",
    ]
    assert clauses[2].title == "Conformance"


def test_detect_clauses_rejects_a_non_successor_number_as_body_text() -> None:
    pages = [
        ReconstructedPage(
            number=1,
            lines=(
                _heading("9.3.2 Classifier", 100),
                _line("3 apples were purchased that day.", 120),
                _line("A classifier is a classification of instances.", 140),
            ),
        )
    ]

    clauses = detect_clauses(pages)

    assert len(clauses) == 1
    assert clauses[0].number == "9.3.2"
    assert [line.text for line in clauses[0].raw_lines] == [
        "3 apples were purchased that day.",
        "A classifier is a classification of instances.",
    ]


def test_detect_clauses_rejects_a_same_font_stray_number_as_body_text() -> None:
    """Even a same-font, heading-shaped line is rejected when it isn't a plausible successor -
    the font gate alone isn't the only defense against false positives."""
    pages = [
        ReconstructedPage(
            number=1,
            lines=(
                _heading("9.3.2 Classifier", 100),
                _line("A classifier is a classification of instances.", 120),
                _line("It describes structural and behavioral features.", 140),
                _line("Classifiers may be generalized.", 160),
                _heading("3 See section 3 for the full grammar.", 180),
            ),
        )
    ]

    clauses = detect_clauses(pages)

    assert len(clauses) == 1
    assert [line.text for line in clauses[0].raw_lines] == [
        "A classifier is a classification of instances.",
        "It describes structural and behavioral features.",
        "Classifiers may be generalized.",
        "3 See section 3 for the full grammar.",
    ]


def test_detect_clauses_ignores_a_body_sized_line_that_matches_the_heading_pattern() -> None:
    """A body-sized line that happens to start with a dotted number (e.g. a numbered list item in
    prose) is never treated as a heading - only a visibly larger font qualifies."""
    pages = [
        ReconstructedPage(
            number=1,
            lines=(
                _heading("1 Scope", 100),
                _line("2 See the numbered list below for details.", 120),
                _line("More scope body text.", 140),
            ),
        )
    ]

    clauses = detect_clauses(pages)

    assert len(clauses) == 1
    assert [line.text for line in clauses[0].raw_lines] == [
        "2 See the numbered list below for details.",
        "More scope body text.",
    ]


def test_detect_clauses_skips_a_table_of_contents_spanning_multiple_pages_with_irregular_annex_numbering() -> None:
    pages = [
        ReconstructedPage(
            number=1,
            lines=(
                _line("Contents", 50),
                _line("1 Scope ..... 3", 80),
                _line("1.1 Overview ..... 4", 100),
                _line("2 Conformance ..... 10", 120),
            ),
        ),
        ReconstructedPage(
            number=2,
            lines=(
                _line("Annex A: Diagrams ..... 300", 80),
                _line("Annex B: UML Diagram Interchange ..... 320", 100),
                _line("B.1 Summary ..... 321", 120),
                _line("B.7.3 UMLAssociationShape [Class] ..... 355", 140),
            ),
        ),
        ReconstructedPage(
            number=3,
            lines=(
                _heading("1 Scope", 100),
                _line("Real scope body line one.", 120),
                _line("Real scope body line two.", 140),
                _line("Real scope body line three.", 160),
                _line("Real scope body line four.", 180),
                _heading("1.1 Overview", 200),
                _line("Real overview body line one.", 220),
                _line("Real overview body line two.", 240),
                _line("Real overview body line three.", 260),
                _line("Real overview body line four.", 280),
            ),
        ),
    ]

    clauses = detect_clauses(pages)

    assert [clause.number for clause in clauses] == ["1", "1.1"]
    assert clauses[0].page_start == 3
    assert [line.text for line in clauses[0].raw_lines] == [
        "Real scope body line one.",
        "Real scope body line two.",
        "Real scope body line three.",
        "Real scope body line four.",
    ]


def test_detect_clauses_tracks_page_span_across_page_boundary() -> None:
    pages = [
        ReconstructedPage(number=1, lines=(_heading("1 Scope", 100), _line("Body on page one.", 120))),
        ReconstructedPage(number=2, lines=(_line("More body on page two.", 100),)),
    ]

    clauses = detect_clauses(pages)

    assert len(clauses) == 1
    assert clauses[0].page_start == 1
    assert clauses[0].page_end == 2
