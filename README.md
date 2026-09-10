# uml4net-sage

Claude plugin providing grounded, spec-accurate answers about UML, backed by a knowledge base built
from the OMG UML specification, the metamodel and uml4net.

Sage is the AI agent of the [uml4net](https://github.com/STARIONGROUP/uml4net) ecosystem: a bridge
between engineers and the UML 2.5.1 specification. It reads the OMG UML metamodel XMI, the Standard
Profile, and (optionally) the specification PDF - all fetched fresh on your own machine, never
redistributed by this plugin - and answers questions grounded in that data instead of general
training knowledge.

## Install

```
/plugin marketplace add STARIONGROUP/uml4net-sage
/plugin install uml4net-sage@uml4net
```

Nothing is downloaded at install time - OMG's specification files aren't redistributed here (see
[Specifications & licensing](#specifications--licensing)). On first use, ask Claude to set up the
knowledge base, or run directly:

```bash
uml4net-sage fetch --version 2.5.1     # XMI + specification PDFs, from omg.org
uml4net-sage generate --version 2.5.1  # builds the knowledge base
```

Pass `--no-specs` to `fetch` to skip the PDFs - metamodel/Standard Profile lookups still work fully;
only verbatim specification citation needs them.

## What it knows

| Skill | Example prompt |
|---|---|
| `metamodel-lookup` | "What features does `Classifier` own and inherit?" |
| `standard-profile-lookup` | "What does the `«Trace»` stereotype mean?" |
| `uml-spec-citation` | "What does the UML spec say about generalization?" |
| `xmi-spec-citation` | "What does the XMI standard say about `href` vs `idref`?" |
| `xmi-schema-lookup` | "What attributes does XMI's `Extension` element support?" |
| `uml-xmi-model-inspection` | "Check this XMI file against the metamodel: `path/to/model.xmi`" |
| `knowledge-setup` | "Is the UML knowledge base fetched and generated?" |

Answers are tagged by provenance: **NORMATIVE** (a verbatim spec quote), **MODEL** (read directly
from the metamodel/Standard Profile XMI), or **DERIVED** (computed here, e.g. a clause number cited
without the PDF available).

## Releases

One UML version is relevant today - 2.5.1 - fetched from a fixed, static set of OMG URLs (see
`sources/README.md`). Unlike specifications with a rolling release cadence, there's no
update-checking machinery here: if OMG ever ships a new UML version, a maintainer adds one entry to
`Uml4Net.Sage.Knowledge.KnownUmlVersions` and cuts a new plugin release - a deliberate,
human-in-the-loop step rather than automated discovery. The companion OMG XMI specification (also
2.5.1 today, by coincidence - it versions independently) is tracked the same way, in
`Uml4Net.Sage.Knowledge.KnownXmiVersions`.

## The knowledge base

Generated locally, never committed - see `knowledge/README.md` for the full file layout and
`CLAUDE.md`'s "Committed vs git-ignored" for why. In short: `metamodel/` and `standard-profile/` are
markdown + JSON derived from OMG's metamodel XMI (structural facts, not the specification's
copyrighted prose); `spec/` (present once the PDFs are fetched and extracted) holds the
specification's own text and is treated identically to the source PDFs for licensing purposes -
generated locally, never committed, never redistributed. `knowledge/xmi/<version>/spec/` holds the
same for the companion OMG XMI specification (the serialization standard UML models are themselves
written in) as a top-level sibling, since it versions independently of UML.

If you have [`jq`](https://jqlang.github.io/jq/) installed, skills use it to query
`knowledge/<version>/metamodel/metamodel.json` directly for set/closure-shaped questions (e.g.
"every concrete subclass of `Classifier`") - faster and cheaper than reading every element's
markdown page. Without `jq`, they fall back to `Grep`/`Read`.

## Specifications & licensing

The OMG UML 2.5.1 specification PDF, its four metamodel XMI files, and the companion OMG XMI 2.5.1
specification PDF are copyrighted by OMG and its member companies (see `NOTICE` for the full list
and license terms) and are **never committed to this repository or bundled with any release** -
`uml4net-sage fetch` downloads them fresh, directly from omg.org, onto your own machine:

- UML Specification PDF: https://www.omg.org/spec/UML/2.5.1/PDF
- Abstract Syntax Metamodel XMI: https://www.omg.org/spec/UML/20161101/UML.xmi
- Primitive Types XMI: https://www.omg.org/spec/UML/20161101/PrimitiveTypes.xmi
- Standard Profile XMI: https://www.omg.org/spec/UML/20161101/StandardProfile.xmi
- Diagram Interchange Metamodel XMI: https://www.omg.org/spec/UML/20161101/UMLDI.xmi
- XMI Specification PDF: https://www.omg.org/spec/XMI/2.5.1/PDF

See `sources/README.md` for the full provenance table and `NOTICE` for OMG's complete licensing
terms.

## Development

This repository contains both the plugin (shipped) and the toolchain that generates its knowledge
base (not shipped - see `tools/README.md`). See `CLAUDE.md` for build/test commands, architecture,
and conventions.
