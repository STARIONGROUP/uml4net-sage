# tools/

The generation toolchain behind the `uml4net-sage` plugin. **Nothing under `tools/` is shipped
with the installed plugin** - the plugin itself only carries `hooks/native/` (the small SessionStart
hook binaries) plus the `skills/`, `agents/`, and `commands/` markdown at the repository root. The
full CLI built here is downloaded on demand by the hook the first time it's actually needed (see
`hooks/native/README.md` and the root `CLAUDE.md`).

| Directory | Responsibility |
|---|---|
| `sage-cli/Uml4Net.Sage.Tools` | The `uml4net-sage` CLI (`fetch`, `generate`, `versions`, `use`, `remove`, `check`, `inspect`) |
| `sage-cli/Uml4Net.Sage.Tools.Hook` | The small, dependency-free NativeAOT SessionStart hook |
| `knowledge-gen/Uml4Net.Sage.Knowledge` | Orchestration: the static version registry, source fetching, the Python `spec_extract` subprocess seam (falling back to an on-demand-provisioned `uv` when no Python is found), `datapackage.json` generation, local install-state tracking |
| `metamodel-gen/Uml4Net.Sage.MetamodelGen` | Reads the UML metamodel/Standard Profile XMI via `uml4net.xmi` and generates the `metamodel/`/`standard-profile/` knowledge base |
| `spec-extract` | Python: PDF-to-markdown clause extraction (the one non-C# component - see its own README for why) |

## Build & test

```bash
dotnet restore uml4net-sage.slnx
dotnet build uml4net-sage.slnx
dotnet test uml4net-sage.slnx

cd tools/spec-extract
python -m venv .venv && .venv/Scripts/pip install -e ".[dev]"   # .venv/bin/pip on Linux/macOS
.venv/Scripts/pytest -v
```

## `--json` output

`check`, `versions` and `inspect` accept `--json`. The output is a contract that the SessionStart
hook, the skills and scripts rely on (`JsonOutputTests` pins it):

- Standard output holds **exactly one line with one JSON document**. There is no banner, progress or
  log text, and nothing is written to standard error.
- **Every failure** is the document `{"error": "..."}` with a non-zero exit code: 1, or 130 when a
  program that runs the verbs in-process cancels before the verb starts (`{"error": "Cancelled."}`).
  That includes a command line that does not parse (with `--json`, `--json:true` or
  `--json=true`), `--json` on a verb that does not offer it or without a verb, `--json` together
  with `--help`, a model file that is not XML, a corrupt `knowledge/installed.json` or
  `metamodel.json`, and any unexpected error. Treat any non-zero exit code as a failure.
- **Ctrl+C is the exception**: the process ends without writing a document. System.CommandLine only
  handles Ctrl+C while an asynchronous verb is still running, and these verbs are synchronous. Treat
  a missing or unparseable document as a failure.
- Property names are PascalCase, as listed below. An absent value is written as `null`, never left
  out. Renaming or removing a property is a breaking change; adding one is not.

| Verb | Exit code | Document |
|---|---|---|
| `check` | 0 | `{"Default": string\|null, "HasAnyVersion": bool, "Versions": [{"Version", "Fetched", "Generated", "SpecGenerated"}], "XmiSpecs": [{"Version", "Fetched", "Generated"}]}` |
| `versions` | 0 | `[{"Version": string, "IsCurrent", "IsDefault", "Fetched", "Generated", "SpecGenerated": bool}]`, one entry per known UML version |
| `inspect` | 0 with no error findings (warnings alone do not fail), 1 with at least one error | `{"ModelPath": string, "UmlVersion": string, "Findings": [{"Severity": "error"\|"warning", "Category": string, "ElementXmiId": string\|null, "Message": string, "Line": int\|null}]}` |
| any | 1, or 130 when cancelled before the verb starts | `{"error": string}` |

`inspect` returns 1 both for a model with error findings and for a failure. Tell them apart by the
document: a report has `Findings`, a failure has `error`.

See the root `CLAUDE.md` for coding conventions, the licensing-driven "committed vs git-ignored"
rules, and determinism requirements for generated output.
