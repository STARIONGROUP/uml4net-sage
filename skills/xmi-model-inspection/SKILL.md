---
name: xmi-model-inspection
description: Check a user-supplied .xmi or .uml model file for conformance to the UML 2.5.1 metamodel - unresolved references, abstract metaclasses instantiated directly, and other reader-level diagnostics. Use when the user shares a UML model file and asks to validate it, check it for errors, or find problems in it.
---

# XMI model inspection

Loads a user-supplied `.xmi`/`.uml` model file with `uml4net.xmi` - the same reader the knowledge
base itself is built with - and checks it against the generated metamodel graph.

## Running the check

Run the CLI directly (this is the one skill that executes the CLI rather than only reading the
knowledge base):

```bash
uml4net-codex inspect <path-to-file> --json
```

If the CLI reports the version hasn't been generated yet, run the `knowledge-setup` skill's fetch +
generate steps first.

The `--json` output is an `InspectionReport`: `{modelPath, umlVersion, findings: [{severity,
category, elementXmiId, message}]}`. `severity` is `"error"` or `"warning"`. Two finding categories
exist today:

- **`reader-diagnostic`** - a warning or error the XMI reader itself emitted while loading the file:
  an unresolved external reference (a `href` the reader couldn't find locally), an unknown
  element/attribute, or similar structural problems. This is usually the most actionable category.
- **`abstract-instantiation`** - an element whose declared `xmi:type` is a metaclass marked
  abstract in the UML 2.5.1 metamodel (e.g. `Classifier`, `Type`) - such a metaclass can never be
  legitimately instantiated directly in valid XMI; only its concrete subtypes (`Class`, `DataType`,
  ...) can. This is rare in practice (most XMI-producing tools never emit it) but is checked because
  it's cheap and unambiguous when it does occur.

## Known scope

Deeper semantic checks - multiplicity violations against a specific association end, whether a
`redefinedProperty`/`subsettedProperty` actually resolves to an inherited feature - are not yet
implemented; they would require reflecting over uml4net's generated property-decorator metadata for
arbitrary elements, which is a larger undertaking than this v1 covers. Mention this limitation if
the user asks for a check this skill doesn't perform, rather than implying full validation coverage.

## Answering

- Report findings grouped by severity, most severe first.
- For each finding, explain *why* it matters using `metamodel-lookup` if useful (e.g. "Classifier is
  abstract because ... - see its Specializations for the concrete metaclasses you can use instead").
- If there are zero findings, say so plainly - don't imply a clean bill of health beyond what the
  two implemented checks actually cover (see "Known scope" above).
