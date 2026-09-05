"""Stage 1: raw positioned words per page. No layout logic lives here."""

from __future__ import annotations

from pathlib import Path

import pdfplumber

from spec_extract.models import PositionedPage, PositionedWord


def read_positioned_pages(pdf_path: str | Path) -> list[PositionedPage]:
    """Open a PDF and extract every word's text and bounding box, page by page."""
    pages: list[PositionedPage] = []
    with pdfplumber.open(pdf_path) as pdf:
        for index, page in enumerate(pdf.pages, start=1):
            raw_words = page.extract_words(extra_attrs=["fontname", "size"], use_text_flow=False)
            words = tuple(
                PositionedWord(
                    text=raw["text"],
                    x0=raw["x0"],
                    x1=raw["x1"],
                    top=raw["top"],
                    bottom=raw["bottom"],
                    font_name=raw.get("fontname", ""),
                    size=raw.get("size", 0.0),
                )
                for raw in raw_words
            )
            pages.append(PositionedPage(number=index, height=float(page.height), words=words))
    return pages
