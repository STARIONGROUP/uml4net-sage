from spec_extract.layout import reconstruct_pages
from spec_extract.models import PositionedPage, PositionedWord


def _word(text: str, x0: float, top: float) -> PositionedWord:
    return PositionedWord(
        text=text, x0=x0, x1=x0 + len(text) * 6, top=top, bottom=top + 10, font_name="Helvetica", size=10.0
    )


def test_reconstruct_pages_orders_words_into_lines_top_to_bottom_left_to_right() -> None:
    page = PositionedPage(
        number=1,
        height=792.0,
        words=(
            _word("World", 130, 200),
            _word("Hello", 72, 200),
            _word("Second", 72, 220),
        ),
    )

    reconstructed = reconstruct_pages([page])

    assert len(reconstructed) == 1
    lines = reconstructed[0].lines
    assert [line.text for line in lines] == ["Hello World", "Second"]


def test_reconstruct_pages_strips_repeated_header_and_changing_page_number_footer() -> None:
    pages = []
    for number in range(1, 5):
        words = (
            _word("Header", 72, 20),
            _word("Body", 72, 400),
            _word(str(41 + number), 300, 780),  # page number changes every page, unlike the header
        )
        pages.append(PositionedPage(number=number, height=792.0, words=words))

    reconstructed = reconstruct_pages(pages)

    for page in reconstructed:
        texts = [line.text for line in page.lines]
        assert texts == ["Body"]
