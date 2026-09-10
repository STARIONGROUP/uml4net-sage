---
name: uml-xmi-model-inspection
description: Check a user-supplied .xmi or .uml UML model FILE for conformance to the UML 2.5.1 metamodel - unresolved references, abstract metaclasses instantiated directly, and other reader-level diagnostics. Use when the user shares a UML model file (serialized as XMI) and asks to validate it, check it for errors, or find problems in it. Does not quote the XMI specification's own normative text - see xmi-spec-citation for that.
---

# UML XMI model inspection

Loads a user-supplied `.xmi`/`.uml` **UML model file** (content serialized in XMI form) with
`uml4net.xmi` - the same reader the knowledge base itself is built with - and checks it against the
generated metamodel graph. This skill is about checking one specific file's UML content; for
questions about what the XMI standard itself requires or means, independent of any particular file
(e.g. the meaning of `href`/`idref`, or cross-document linking rules), use `xmi-spec-citation`
instead - that skill is about the XMI serialization standard, not UML content.

## Running the check

Run the CLI directly (this is the one skill that executes the CLI rather than only reading the
knowledge base):

```bash
uml4net-sage inspect <path-to-file> --json
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
- For each `abstract-instantiation` finding, explain *why* it matters using `metamodel-lookup` if
  useful (e.g. "Classifier is abstract because ... - see its Specializations for the concrete
  metaclasses you can use instead").
- For each `reader-diagnostic` finding, where the underlying mechanism is governed by the XMI
  standard itself rather than the UML metamodel (e.g. an unresolved `href` - governed by XMI's
  cross-document linking rules, clause 7.10), name the relevant XMI clause and use
  `xmi-spec-citation` to quote or cite it, the same way `abstract-instantiation` findings lean on
  `metamodel-lookup`. Not every reader diagnostic maps to a specific clause - only do this when it
  genuinely clarifies the finding, not as a rote addition to every message.
- If there are zero findings, say so plainly - don't imply a clean bill of health beyond what the
  two implemented checks actually cover (see "Known scope" above).
