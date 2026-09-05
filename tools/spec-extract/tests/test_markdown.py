from spec_extract.markdown import render_clause_markdown
from spec_extract.models import Clause, Paragraph


def test_render_clause_markdown_includes_front_matter_and_body() -> None:
    clause = Clause(number="9.3.2", title="Classifier", page_start=123, page_end=125)
    clause.paragraphs = [
        Paragraph(text="A Classifier shall have a name.", is_normative=True),
        Paragraph(text="NOTE This is informative.", informative_kind="note"),
    ]

    rendered = render_clause_markdown(clause, document="UML", version="2.5.1")

    assert '---\nclause: "9.3.2"' in rendered
    assert 'title: "Classifier"' in rendered
    assert 'document: "UML"' in rendered
    assert 'version: "2.5.1"' in rendered
    assert 'pages: "123-125"' in rendered
    assert "normative: true" in rendered
    assert "# 9.3.2 Classifier" in rendered
    assert "A Classifier shall have a name." in rendered
    assert "<!-- informative:note -->" in rendered
    assert "NOTE This is informative." in rendered
    assert "<!-- /informative:note -->" in rendered


def test_render_clause_markdown_single_page_pages_field() -> None:
    clause = Clause(number="1", title="Scope", page_start=3, page_end=3)
    clause.paragraphs = [Paragraph(text="This is scope text.")]

    rendered = render_clause_markdown(clause, document="UML", version="2.5.1")

    assert 'pages: "3"' in rendered
    assert "normative: false" in rendered
