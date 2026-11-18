# inspection (C#)

`Uml4Net.Sage.Inspection` checks a UML model file serialized as XMI against the UML metamodel, and
reports what it finds as findings, each with a severity, a category, the element's `xmi:id` where
there is one, and a line. The `uml4net-sage inspect` verb runs it.

It has no counterpart in mycelium-hypha, which checks SysML v2 notation through a skill instead; it
is listed in section 2.10 of `sage-mcp-refactor-plan.md`.

## What it does

- **`XmiDocumentScanner`** reads the file once with a plain `XmlReader`, before uml4net does: an
  abstract or unknown `xmi:type`, or a duplicate `xmi:id`, would make uml4net's reader abort
  without saying where.
- **`XmiInspector`** then reads the model with `uml4net.xmi`. uml4net's structured
  `XmiReferenceResolutionFailure`s are only reachable through the `UnresolvedReferencesException`
  that `ThrowOnUnresolvedReferences` raises, which loses the model, so it reads with that option on
  and, only if it throws, reads again with it off. A `pathmap://` prefix mapped to a folder is
  expanded into one entry per referenced document, because uml4net's `PathMaps` keys are whole
  document URIs.
- **`MetamodelFacts`** reads what the inspection needs from the generated `metamodel.json`: which
  metaclasses exist, which are abstract, their ancestors, and the features each requires a value
  for.
- **`CapturingLoggerProvider`** collects what uml4net logs while reading, which becomes findings too.

## Building and testing

```sh
dotnet test tools/inspection/Uml4Net.Sage.Inspection.Tests/Uml4Net.Sage.Inspection.Tests.csproj
```

The model files under `Uml4Net.Sage.Inspection.Tests/Fixtures/` are hand-written toy models, never
OMG's own files (see `CLAUDE.md`, "Committed vs git-ignored"). The CLI's tests read the same files,
linked into their own output rather than copied.
