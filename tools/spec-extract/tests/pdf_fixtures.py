"""A minimal, hand-written (not OMG-derived) single-page PDF builder for pdf_reader tests.

Avoids pulling in a PDF-generation dependency just to test PDF *reading*: this writes
the smallest valid PDF structure by hand (one page, one Helvetica font, a content
stream placing left-aligned text lines).
"""

from __future__ import annotations


def build_simple_pdf(
    lines: list[str],
    *,
    start_y: float = 700,
    line_height: float = 20,
    x: float = 72,
    page_width: float = 612,
    page_height: float = 792,
    sizes: list[float] | None = None,
) -> bytes:
    """`sizes`, when given, is a per-line font size (defaulting to 12pt for any line it omits) -
    used to give heading lines a distinctly larger size than body text, matching real documents."""
    content_lines = []
    y = start_y
    for index, line in enumerate(lines):
        escaped = line.replace("\\", r"\\").replace("(", r"\(").replace(")", r"\)")
        size = sizes[index] if sizes and index < len(sizes) else 12.0
        content_lines.append(f"BT /F1 {size:.1f} Tf {x:.1f} {y:.1f} Td ({escaped}) Tj ET")
        y -= line_height
    content_stream = "\n".join(content_lines).encode("latin-1")

    objects: list[bytes] = [
        b"<< /Type /Catalog /Pages 2 0 R >>",
        b"<< /Type /Pages /Kids [3 0 R] /Count 1 >>",
        f"<< /Type /Page /Parent 2 0 R /Resources << /Font << /F1 4 0 R >> >> "
        f"/MediaBox [0 0 {page_width:.0f} {page_height:.0f}] /Contents 5 0 R >>".encode("latin-1"),
        b"<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>",
        b"<< /Length %d >>\nstream\n" % len(content_stream) + content_stream + b"\nendstream",
    ]

    buffer = bytearray(b"%PDF-1.4\n")
    offsets = [0]
    for index, body in enumerate(objects, start=1):
        offsets.append(len(buffer))
        buffer += f"{index} 0 obj\n".encode("latin-1") + body + b"\nendobj\n"

    xref_offset = len(buffer)
    buffer += f"xref\n0 {len(objects) + 1}\n".encode("latin-1")
    buffer += b"0000000000 65535 f \n"
    for offset in offsets[1:]:
        buffer += f"{offset:010d} 00000 n \n".encode("latin-1")
    buffer += (f"trailer\n<< /Size {len(objects) + 1} /Root 1 0 R >>\nstartxref\n{xref_offset}\n%%EOF").encode(
        "latin-1"
    )

    return bytes(buffer)
