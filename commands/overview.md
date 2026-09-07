---
description: What the uml4net-sage plugin can do and how to set it up.
disable-model-invocation: true
---

# uml4net-sage

Sage is the AI agent of the uml4net ecosystem: grounded, spec-accurate answers about the OMG UML
2.5.1 specification, backed by a knowledge base built from the UML metamodel XMI, the OMG
specification text, and the `uml4net` .NET library.

## First-time setup

Nothing is downloaded when you install this plugin - OMG's UML specification files aren't
redistributed here (see `NOTICE`). Ask Claude to set it up, or run directly:

```bash
uml4net-sage fetch --version 2.5.1     # downloads XMI + specification PDFs from omg.org
uml4net-sage generate --version 2.5.1  # builds the knowledge base
```

Pass `--no-specs` to `fetch` to skip the PDFs - the metamodel and Standard Profile knowledge base
still works fully; only verbatim specification citation is unavailable until you fetch them.

## What you can ask

- **Metamodel structure** - "What features does Classifier own and inherit?", "Is Interface
  abstract?", "What does Property redefine from StructuralFeature?"
- **Standard Profile stereotypes** - "What does the «Trace» stereotype mean?", "What can «Create»
  be applied to?"
- **Specification text** - "What does the spec say about generalization?", "Quote the definition of
  Association." (needs the PDFs fetched)
- **Model conformance** - "Check this XMI file against the metamodel: path/to/model.xmi"

## Specifications & licensing

The OMG UML 2.5.1 specification PDF and metamodel XMI are copyrighted by OMG and are never
committed to this repository - they are fetched fresh, at runtime, on your own machine. See
`NOTICE` and `sources/README.md` for the full licensing details and the exact OMG URLs used.
