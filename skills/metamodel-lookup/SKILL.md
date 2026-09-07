---
name: metamodel-lookup
description: Look up the structure of a UML 2.5.1 metaclass - its owned and inherited features, generalizations/specializations, multiplicities, redefinitions/subsettings, and OCL constraints. Use when the user asks "what is a Classifier", "what attributes does Class own", "what does Property inherit from", "is Interface abstract", or names any UML metaclass, enumeration, or primitive type.
---

# Metamodel lookup

Answers questions about the **structure of the UML 2.5.1 metamodel itself** - metaclasses (Class,
Classifier, Property, Association, ...), enumerations (VisibilityKind, AggregationKind, ...), and
primitive types (Integer, String, ...). Not for Standard Profile stereotypes (`«Trace»`, `«Create»`,
...) - see the `standard-profile-lookup` skill for those. Not for verbatim spec prose - see
`spec-citation`.

## Read order

1. `knowledge/installed.json` - resolve the default UML version (the `default` field). If the file
   doesn't exist or has no versions, tell the user to run `uml4net-sage fetch` first (see the
   `knowledge-setup` skill) and stop.
2. `knowledge/<version>/metamodel/index.json` - an array of `{name, kind, package, qualifiedName,
   isAbstract, file}` rows. Find the row for the element the user asked about (case-insensitive
   match on `name`; if ambiguous, list the matches and ask). `kind` is `"class"`, `"enumeration"`,
   or `"primitiveType"`.
3. `knowledge/<version>/metamodel/elements/<file>` (the `file` field from the index row) - the full
   per-element markdown page: front matter, `## Generalizations`, `## Specializations`,
   `## Owned features`, `## Inherited features` (a full precomputed table - never re-derive this by
   hand-walking generalizations), `## Constraints` (OCL, tagged MODEL tier since it's read directly
   from the metamodel XMI).

## Set/closure questions

For questions like "every concrete subclass of Classifier" or "which metaclasses have a feature
typed by ValueSpecification", prefer `jq` over `knowledge/<version>/metamodel/metamodel.json` (a
single JSON document with every class's `allAncestors`, `allDescendants`, `directSubclasses`,
`ownedAttributes`, `inheritedAttributes`, `ownedOperations`, and `constraints` precomputed - no
JSON library required, and cheaper than reading every element file). If `jq` isn't installed, fall
back to `Read`/`Grep` over the per-element markdown files; it works but costs more context.

Example: `jq '.classes[] | select(.isAbstract == false and (.allAncestors | index("UML::Classification::Classifier")) != null) | .name' knowledge/2.5.1/metamodel/metamodel.json`

## Answering

- Always state which UML version the answer is for (e.g. "In UML 2.5.1, ...").
- Tag facts read from `metamodel.json`/the element markdown as **MODEL** tier (read directly from
  the metamodel XMI) - distinct from **NORMATIVE** (verbatim spec quote, see `spec-citation`) and
  **DERIVED** (computed/inferred here, e.g. "no clause reference is available").
- Link to related metaclasses by name so the user can ask a follow-up ("tell me more about
  Property") rather than dumping the whole inheritance chain unprompted.
