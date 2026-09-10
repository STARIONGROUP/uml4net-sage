---
name: xmi-spec-citation
description: Quote or cite the normative text of the OMG XMI 2.5.1 specification - the XML serialization standard itself (namespaces, xmi:id/idref/href, xmi:type, document structure), independent of UML - with an exact clause number. Use when the user asks what the XMI standard requires or means, not the UML specification's own text (see uml-spec-citation), UML metamodel structure (see metamodel-lookup), or a specific UML model file's conformance (see uml-xmi-model-inspection).
---

# XMI spec citation

Quotes the **verbatim OMG XMI 2.5.1 specification text**, with an exact clause number, when it has
been locally extracted. XMI is an independent OMG standard from UML - it defines the XML
serialization format UML models (and this specification's own machine-readable files) are written
in, versioning on its own schedule even though it happens to also be at "2.5.1" today. For the UML
specification's own text, use `uml-spec-citation` instead; for a specific UML model file's
conformance to the UML metamodel, use `uml-xmi-model-inspection`.

## Read order

1. `knowledge/registry.json`'s `xmiVersions` - resolve the current XMI version (today, "2.5.1"; there
   is no per-UML-version "default" concept here, since the XMI corpus doesn't vary by UML version).
2. `knowledge/installed.json`'s `xmiSpecs` array - find the entry for that version. If it's absent,
   `generated` is `false`, or `knowledge/xmi/<version>/spec/` doesn't exist, **XMI spec text has not
   been extracted** - see "When spec text is missing" below and stop.
3. `knowledge/xmi/<version>/spec/index.json` is an array of `{clause, title, document, pages,
   normative, file}` rows (`document` is always `"XMI"` here - it exists to disambiguate this file's
   rows from the structurally-identical `knowledge/<version>/spec/index.json` for UML). Find the
   clause covering the topic by searching `title`.
4. `knowledge/xmi/<version>/spec/clauses/<file>` - the clause's markdown, with YAML front matter
   (`clause`, `title`, `document`, `version`, `pages`, `normative`) and body text. Paragraphs wrapped
   in `<!-- informative:note -->`/`<!-- informative:example -->` comments are informative, not
   normative requirements - say so when quoting one.

## When spec text is missing

If `knowledge/xmi/<version>/spec/` doesn't exist (the XMI PDF hasn't been fetched, or `uv` could not
be provisioned to run `spec_extract` - offline on first use, or an unsupported platform; see
`tools/spec-extract/README.md`), **never fabricate a quote**. Instead:
- If you can still name the likely governing clause (e.g. from general XMI knowledge, or because
  `uml-xmi-model-inspection`'s reader-diagnostic findings point at a specific mechanism like
  unresolved `href` references), cite the clause **number only**, tagged **DERIVED**, and say plainly
  that this is a reference, not a verbatim quote.
- Tell the user how to unlock verbatim citation: run `uml4net-sage fetch --version <UML version>`
  (without `--no-specs` - this also fetches the companion XMI PDF) then `uml4net-sage generate
  --version <UML version>`. Note this is independent of UML's own spec-citation availability: one
  can be extracted while the other isn't (e.g. if the XMI PDF failed to download, `fetch` prints a
  warning but UML setup still completes).

## Known extraction limitations

Beyond the general PDF-extraction caveats already documented for `uml-spec-citation`
(`tools/spec-extract/README.md`, "Known limitations" - garbled figure-page text, occasional mojibake
bullets), two things specific to this corpus are worth knowing:

- **Letter-prefixed annexes aren't extracted as clauses.** The extractor's heading detection only
  matches dotted-decimal numbers (e.g. "7.10.2"), not "Annex A"-style headings - confirmed against
  the real XMI 2.5.1 document, which does reference annexes in body text without them appearing as
  their own clauses. If a question is about content that would live in an annex, say plainly that it
  isn't available via this index rather than implying full coverage.
- **A materially lower fraction of clauses are tagged `normative: true`** than in the UML spec (about
  16% in the real document, vs. UML's much higher ratio). This isn't an extraction defect: XMI states
  many of its rules declaratively ("The XMI element name is...") rather than with `shall`/`must`
  language, which is what the normative-paragraph classifier looks for. A clause tagged
  `normative: false` here may still describe a definitive rule - don't imply a clause is merely
  informative just because it wasn't classified as normative; read its actual wording.

## Answering

- Always state the clause number and that this is the **XMI** specification (not UML) - the two
  corpora share an identical file shape, so naming which one avoids ambiguity.
- Tag a genuine verbatim quote as **NORMATIVE** only when `normative: true` in the clause's front
  matter; an informative NOTE/EXAMPLE excerpt is **NORMATIVE-adjacent but not itself a requirement**
  - say which it is. See "Known extraction limitations" above before treating a `normative: false`
  tag as meaning "not a rule."
- Keep quotes short and attributed; do not reproduce an entire long clause verbatim when a sentence
  or two answers the question.
