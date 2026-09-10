---
name: xmi-schema-lookup
description: Answer structural questions about the OMG XMI 2.5.1 specification's own machine-readable schema (XMI.xsd, XMI-Canonical.xsd) - what attributes an element/complex type carries, what a complex type extends, what an <xsd:any> wildcard's processContents means. Use when the user asks about XMI's schema-level structure (e.g. "what attributes does xmi:Extension support", "what's in the LinkAttribs group"), not the XMI specification's prose (see xmi-spec-citation), the UML metamodel (see metamodel-lookup), or a specific UML model file's conformance (see uml-xmi-model-inspection).
---

# XMI schema lookup

Answers structural (**MODEL**-tier) questions about XMI's own generic wrapper schema by reading the
two real, fetched `.xsd` files directly - `XMI.xsd` and `XMI-Canonical.xsd` (a narrower profile).
There is **no generated index or per-construct markdown** for this corpus, unlike `metamodel-lookup`:
both files together are only about 120 lines of plain, un-annotated XSD - small enough to read in
full directly, so generating an intermediate representation would add ceremony without adding
comprehension. Read the files themselves for the current, authoritative facts rather than relying on
any summary below going stale.

## Read order

1. `knowledge/registry.json`'s `xmiVersions` - resolve the current XMI version (today, "2.5.1").
2. `knowledge/installed.json`'s `xmiSpecs` array - confirm that version's entry has `fetched: true`.
   If it's absent or `false`, the schema files haven't been fetched - see "When the schema isn't
   fetched" below and stop.
3. Read `sources/xmi/<version>/schema/XMI.xsd` and `sources/xmi/<version>/schema/XMI-Canonical.xsd`
   directly (`Read`/`Grep` - both are small enough to read in full; `Grep` for a construct name first
   if only checking whether it exists in one file vs. the other).

## Interpreting the raw XSD

Since there's no compiled/resolved object model here (unlike code that runs `XmlSchemaSet.Compile()`
against these files), resolve references by hand when reading:

- **`<xsd:attributeGroup ref="X"/>`** inside a complex type or another attribute group means "every
  attribute declared in group X also applies here" - follow the reference to `X`'s own declaration
  and include its attributes in the answer. `ObjectAttribs` in particular refs both
  `IdentityAttribs` and `LinkAttribs`, so a complex type that refs `ObjectAttribs` transitively gets
  `label`/`uuid` (or just `uuid` in `XMI-Canonical.xsd`, which drops `label`) plus `href`/`idref`
  plus its own `type` attribute.
- **`<xsd:complexContent><xsd:extension base="X">`** means the complex type inherits `X`'s content
  and attributes, then adds its own - report this as "extends X", the closest XSD analogue to
  generalization. `Add`/`Replace`/`Delete` all extend `Difference` this way.
- **`<xsd:any processContents="strict|lax|skip"/>`** is a wildcard: content from *any* namespace is
  allowed at that point, but how strictly it's checked depends on the value. `XMI.xsd`'s own root
  `XMI` complex type uses `processContents="strict"` - this is the concrete, citable answer to "why
  can't XMI.xsd validate a real UML model on its own": strict wildcard content requires a schema for
  the content's own namespace (here, `uml:`) to be available, and OMG does not publish one - so any
  real UML content under `<xmi:XMI>` is simply outside what this schema alone can check. `Any`'s
  complex type uses `processContents="skip"` instead - no checking at all, a deliberate escape hatch.
- Neither file has any `<xsd:annotation>`/`<xsd:documentation>` - there is no prose to quote here;
  everything answerable from these files is structural, not a citable sentence (that's what
  `xmi-spec-citation`'s prose corpus is for).

## When the schema isn't fetched

**Never guess at the schema's structure from general knowledge** if the files aren't present -
say so plainly and point at the fetch command: `uml4net-sage fetch --version <version>` (without
`--no-specs` - this fetches the schema files alongside the PDF). This is independent of whether the
XMI *specification text* (`xmi-spec-citation`) or the UML corpus is available - each can be present
or absent on its own.

## Answering

- Always name which file the fact came from (`XMI.xsd` vs. `XMI-Canonical.xsd`) - they're similar but
  not identical (e.g. `XMI-Canonical.xsd`'s `IdentityAttribs` drops `label`).
- Tag facts as **MODEL** tier - read directly from the real schema, not paraphrase.
- When resolving an `attributeGroup` reference, show your work (name the group you followed) rather
  than just listing the final flattened attribute set, so the answer stays traceable back to the
  actual file structure.
- For structure/prose questions that mix ("what does XMI.xsd's Extension type look like, and what
  does the spec say about how it's meant to be used"), split the answer: the schema-structure part
  from here, the prose part via `xmi-spec-citation`, each tagged with its own tier.
