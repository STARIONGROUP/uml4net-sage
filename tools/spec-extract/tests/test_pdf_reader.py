from pathlib import Path

from spec_extract.pdf_reader import read_positioned_pages
from tests.pdf_fixtures import build_simple_pdf


def test_read_positioned_pages_extracts_text_and_positions(tmp_path: Path) -> None:
    pdf_path = tmp_path / "sample.pdf"
    pdf_path.write_bytes(build_simple_pdf(["Hello World", "Second Line"]))

    pages = read_positioned_pages(pdf_path)

    assert len(pages) == 1
    page = pages[0]
    assert page.number == 1
    assert page.height == 792.0

    texts = [word.text for word in page.words]
    assert "Hello" in texts
    assert "World" in texts
    assert "Second" in texts
    assert "Line" in texts


def test_read_positioned_pages_words_have_increasing_top_per_line(tmp_path: Path) -> None:
    pdf_path = tmp_path / "sample.pdf"
    pdf_path.write_bytes(build_simple_pdf(["First", "Second"], start_y=700, line_height=50))

    pages = read_positioned_pages(pdf_path)
    words = sorted(pages[0].words, key=lambda word: word.top)

    assert words[0].text == "First"
    assert words[1].text == "Second"
    assert words[0].top < words[1].top
