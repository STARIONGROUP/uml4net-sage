# tools/

The generation toolchain behind the `uml4net-codex` plugin. **Nothing under `tools/` is shipped
with the installed plugin** - the plugin itself only carries `hooks/native/` (the small SessionStart
hook binaries) plus the `skills/`, `agents/`, and `commands/` markdown at the repository root. The
full CLI built here is downloaded on demand by the hook the first time it's actually needed (see
`hooks/native/README.md` and the root `CLAUDE.md`).

| Directory | Responsibility |
|---|---|
| `codex-cli/Uml4Net.Codex.Tools` | The `uml4net-codex` CLI (`fetch`, `generate`, `versions`, `use`, `remove`, `check`, `inspect`) |
| `codex-cli/Uml4Net.Codex.Tools.Hook` | The small, dependency-free NativeAOT SessionStart hook |
| `knowledge-gen/Uml4Net.Codex.Knowledge` | Orchestration: the static version registry, source fetching, the Python `spec_extract` subprocess seam, `datapackage.json` generation, local install-state tracking |
| `metamodel-gen/Uml4Net.Codex.MetamodelGen` | Reads the UML metamodel/Standard Profile XMI via `uml4net.xmi` and generates the `metamodel/`/`standard-profile/` knowledge base |
| `spec-extract` | Python: PDF-to-markdown clause extraction (the one non-C# component - see its own README for why) |

## Build & test

```bash
dotnet restore uml4net-codex.slnx
dotnet build uml4net-codex.slnx
dotnet test uml4net-codex.slnx

cd tools/spec-extract
python -m venv .venv && .venv/Scripts/pip install -e ".[dev]"   # .venv/bin/pip on Linux/macOS
.venv/Scripts/pytest -v
```

See the root `CLAUDE.md` for coding conventions, the licensing-driven "committed vs git-ignored"
rules, and determinism requirements for generated output.
