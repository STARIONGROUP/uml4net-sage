"""Thin, non-interactive entry point so the `uml4net-sage` C# CLI can invoke this pipeline.

    python -m spec_extract extract --pdf <path-to-pdf> --out <output-dir> [--document UML] [--version 2.5.1]

The primary dev workflow for this package is still pytest (see tests/), matching
mycelium-hypha's "driven by tests, not a distributed CLI" approach; this entry point
exists only for the `generate` verb's `spec` step to shell out to non-interactively.
"""

from __future__ import annotations

import argparse
import sys

from spec_extract.pipeline import extract_document


def main(argv: list[str] | None = None) -> int:
    parser = argparse.ArgumentParser(prog="spec_extract")
    subparsers = parser.add_subparsers(dest="command", required=True)

    extract_parser = subparsers.add_parser("extract", help="Extract clause markdown from a spec PDF.")
    extract_parser.add_argument("--pdf", required=True, help="Path to the source PDF.")
    extract_parser.add_argument("--out", required=True, help="Output directory for clauses/, index.json, index.md.")
    extract_parser.add_argument("--document", default="UML", help="Document name recorded in front matter.")
    extract_parser.add_argument("--version", default="2.5.1", help="Spec version recorded in front matter.")

    args = parser.parse_args(argv)

    if args.command == "extract":
        result = extract_document(args.pdf, args.out, document=args.document, version=args.version)
        print(f"Extracted {len(result.clauses)} clauses to {result.output_dir}")
        return 0

    return 1


if __name__ == "__main__":
    sys.exit(main())
