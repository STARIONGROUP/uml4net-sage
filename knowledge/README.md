# knowledge/

The generated UML 2.5.1 knowledge base skills read from. **Nothing under `knowledge/<version>/` is
committed to this repository** - it's generated on your own machine by `uml4net-codex fetch` +
`uml4net-codex generate`, from OMG's own XMI and PDF files, which are themselves never committed
either (see the root `CLAUDE.md`, "Committed vs git-ignored", and `NOTICE`).

## Committed here

- `registry.json` - the static list of UML versions this tool knows about (mirrors
  `Uml4Net.Codex.Knowledge.KnownUmlVersions`), so skills can see what's available without invoking
  the CLI.
- `metamodel.schema.json`, `index.schema.json`, `datapackage.schema.json` - JSON Schema
  documentation of the generated shapes below. None of these are copyrighted OMG content - they
  describe uml4net-codex's own output format.

## Generated per version (git-ignored)

```
knowledge/<version>/
├── datapackage.json                    # OKF Frictionless Data Package, scoped to the 3 tabular indexes below
├── metamodel/
│   ├── index.json / index.md           # every class/enumeration/primitiveType: {name, kind, package, qualifiedName, isAbstract, file}
│   ├── metamodel.json                  # full graph with precomputed inheritance closures
│   └── elements/<Name>.md              # one page per class/enumeration/primitiveType
├── standard-profile/
│   ├── index.json / index.md           # every stereotype: {qualifiedName, kind, file, source}
│   └── pages/<Name>.md                 # one page per stereotype
└── spec/                               # present only once the PDFs are fetched and extracted
    ├── index.json / index.md           # every clause: {clause, title, pages, normative, file}
    └── clauses/<number>-<slug>.md      # one page per clause
```

`knowledge/installed.json` (also git-ignored) records which version(s) are fetched/generated on
this machine and which is the default - see `Uml4Net.Codex.Knowledge.InstalledVersionsStore`.

## Why `datapackage.json` and not a bespoke index

`datapackage.json` describes only the three genuinely tabular resources (the `index.json` files) as
an OKF Frictionless Data Package, with a real Table Schema per resource. It's deliberately **not**
the manifest skills read through - they read `metamodel/index.json` etc. directly at their stable,
well-known paths, exactly as mycelium-hypha's skills do. The Frictionless format's actual value here
is external-tool interoperability and a standard vocabulary for licensing/provenance metadata, not
an indirection layer - see `datapackage.schema.json`'s `$comment` for the full reasoning.

## Provenance tiers

Every skill answer should be tagged with where the fact came from:

- **NORMATIVE** - a verbatim quote from `spec/`, i.e. the OMG specification text itself.
- **MODEL** - read directly from the metamodel/Standard Profile XMI (an OCL constraint body, a
  feature's multiplicity, a stereotype's base metaclass).
- **DERIVED** - computed or inferred here (e.g. a clause number cited without the PDF fetched).
