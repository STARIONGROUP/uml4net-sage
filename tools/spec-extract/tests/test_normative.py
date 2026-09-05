from spec_extract.models import Clause, Line, PositionedWord
from spec_extract.normative import split_normative


def _line(text: str, top: float) -> Line:
    word = PositionedWord(
        text=text, x0=72, x1=72 + len(text) * 6, top=top, bottom=top + 10, font_name="Helvetica", size=10.0
    )
    return Line(top=top, words=(word,))


def _clause(lines: list[Line]) -> Clause:
    clause = Clause(number="1", title="Test", page_start=1, page_end=1)
    clause.raw_lines = lines
    return clause


def test_split_normative_groups_close_lines_into_one_paragraph_and_splits_on_a_big_gap() -> None:
    clause = _clause(
        [
            _line("A classifier is a classification", 100),
            _line("of instances.", 112),
            _line("It describes structural features.", 160),  # big gap before this line
        ]
    )

    split_normative(clause)

    assert len(clause.paragraphs) == 2
    assert clause.paragraphs[0].text == "A classifier is a classification of instances."
    assert clause.paragraphs[1].text == "It describes structural features."


def test_split_normative_flags_shall_and_must_as_normative() -> None:
    clause = _clause(
        [
            _line("A Class shall have a", 100),
            _line("name.", 112),  # small gap: same paragraph as the line above
            _line("A description may be provided.", 160),  # big gap: a new, non-normative paragraph
        ]
    )

    split_normative(clause)

    assert len(clause.paragraphs) == 2
    assert clause.paragraphs[0].text == "A Class shall have a name."
    assert clause.paragraphs[0].is_normative is True
    assert clause.paragraphs[1].is_normative is False
    assert clause.is_normative is True


def test_split_normative_tags_note_and_example_as_informative_and_not_normative() -> None:
    clause = _clause(
        [
            _line("A Class must have a name.", 100),
            _line("NOTE This is an informative note that also says shall.", 112),
            _line("EXAMPLE Consider a Class named Car.", 124),
        ]
    )

    split_normative(clause)

    assert len(clause.paragraphs) == 3
    assert clause.paragraphs[0].is_normative is True
    assert clause.paragraphs[1].informative_kind == "note"
    assert clause.paragraphs[1].is_normative is False
    assert clause.paragraphs[2].informative_kind == "example"
