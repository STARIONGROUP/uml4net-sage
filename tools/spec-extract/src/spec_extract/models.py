"""Data model shared across the extraction pipeline stages."""

from __future__ import annotations

from dataclasses import dataclass, field


@dataclass(frozen=True)
class PositionedWord:
    """A single word with its bounding box and font, as reported by pdfplumber."""

    text: str
    x0: float
    x1: float
    top: float
    bottom: float
    font_name: str
    size: float


@dataclass(frozen=True)
class PositionedPage:
    """The raw positioned words on one page, in no particular order."""

    number: int
    height: float
    words: tuple[PositionedWord, ...]


@dataclass(frozen=True)
class Line:
    """One reading-order line: words sorted left-to-right at a shared height."""

    top: float
    words: tuple[PositionedWord, ...]

    @property
    def text(self) -> str:
        return " ".join(word.text for word in self.words)


@dataclass(frozen=True)
class ReconstructedPage:
    """A page's lines in top-to-bottom reading order, headers/footers stripped."""

    number: int
    lines: tuple[Line, ...]


@dataclass
class Paragraph:
    """One paragraph of a clause's body text."""

    text: str
    is_normative: bool = False
    informative_kind: str | None = None  # "note" | "example" | None


@dataclass
class Clause:
    """One detected clause of the specification: a numbered heading plus its body.

    `raw_lines` holds the body as reading-ordered lines, populated by clause detection;
    `paragraphs` is populated afterwards by the normative/informative split.
    """

    number: str
    title: str
    page_start: int
    page_end: int
    raw_lines: list[Line] = field(default_factory=list)
    paragraphs: list[Paragraph] = field(default_factory=list)

    @property
    def is_normative(self) -> bool:
        return any(paragraph.is_normative for paragraph in self.paragraphs)

    @property
    def slug(self) -> str:
        slug = "".join(character if character.isalnum() else "-" for character in self.title.lower())
        while "--" in slug:
            slug = slug.replace("--", "-")
        return slug.strip("-")

    @property
    def file_name(self) -> str:
        return f"{self.number}-{self.slug}.md"
