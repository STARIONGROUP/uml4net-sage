---
name: standard-profile-lookup
description: Look up a UML 2.5.1 Standard Profile stereotype (e.g. Create, Destroy, Trace, Refine, Derive, Focus, Metaclass, ModelLibrary, Framework, Utility) - what metaclass it extends and what it means. Use when the user asks "what does the Trace stereotype mean", "what can Create be applied to", or names a stereotype in guillemets like «Trace».
---

# Standard Profile lookup

Answers questions about the OMG UML 2.5.1 **Standard Profile** - the fixed set of stereotypes
(`«Trace»`, `«Create»`, `«Destroy»`, `«Refine»`, `«Derive»`, `«Focus»`, `«Metaclass»`,
`«ModelLibrary»`, `«Framework»`, `«Utility»`, ...) that OMG ships as part of the UML specification
itself. Not for metamodel structure (a stereotype is not a metaclass) - see `metamodel-lookup` for
that, including for the primitive types (Integer, String, ...), which live in the metamodel index,
not here.

## Read order

1. `knowledge/installed.json` - resolve the default UML version.
2. `knowledge/<version>/standard-profile/index.json` - an array of `{qualifiedName, kind, file,
   source}` rows, one per stereotype (`kind` is always `"stereotype"`, `source` is `"StandardProfile"`).
3. `knowledge/<version>/standard-profile/pages/<file>` - front matter with `baseMetaclasses` (which
   metaclasses the stereotype can be applied to) plus `## Base metaclasses`, `## Tagged values`
   (the stereotype's own attributes, i.e. what you'd set when applying it), and `## Description`
   (the OMG documentation comment, when present in the source XMI - some stereotypes have none).

## Answering

- State the UML version.
- Name every base metaclass a stereotype extends - a stereotype can apply to more than one (e.g.
  `«Trace»` extends both `Abstraction` and other classifiers in some editions - always check the
  actual list rather than assuming one).
- Tag facts as **MODEL** tier (read directly from `StandardProfile.xmi`). If `## Description` says
  "_No description available._", say so plainly rather than inventing an explanation - the OMG
  Standard Profile XMI itself omits prose for some stereotypes.
