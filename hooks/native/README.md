# hooks/native/

Committed, platform-specific `SessionStart` hook binaries: a small, dependency-free NativeAOT build
of `tools/codex-cli/Uml4Net.Codex.Tools.Hook`. This binary only checks local knowledge-base status
and (on demand) downloads/verifies the full `uml4net-codex` CLI from a GitHub Release - it never
depends on `uml4net.xmi`, PdfPig-equivalents, or any other heavy library, which is what makes it
NativeAOT-publishable in the first place (the full CLI is not - see the root `CLAUDE.md`).

## Layout

```
hooks/native/
├── win-x64/uml4net-codex-hook.exe
├── linux-x64/uml4net-codex-hook
└── osx-arm64/uml4net-codex-hook
```

`.claude-plugin/plugin.json`'s `SessionStart` hook command dispatches to the matching binary by
`uname -s`/`uname -m` at runtime; on any platform without a matching case (currently Intel Mac,
`Darwin-x86_64`) the hook simply no-ops rather than failing.

**No `osx-x64` build**: GitHub's `macos-13` hosted runner (the last Intel-Mac image) queued
indefinitely rather than starting, as of 2026-09 - consistent with GitHub retiring Intel Mac runners
in favor of Apple Silicon. Revisit if a hosted or self-hosted Intel-Mac runner becomes available
again; until then, Intel Mac users get no `SessionStart` status check (skills/agents still work
normally against whatever `knowledge/` tree exists on disk - the hook is a convenience, not a
dependency).

## How these are built

`.github/workflows/hook-binaries.yml` runs a build matrix across the three currently-buildable
runtime identifiers and commits the resulting binaries here as part of cutting a release. To build
one locally (including for `osx-x64`, on real Intel-Mac hardware):

```bash
dotnet publish tools/codex-cli/Uml4Net.Codex.Tools.Hook -r <rid> -c Release --self-contained -p:PublishAot=true -o hooks/native/<rid>
```

**Prerequisites for a NativeAOT publish**: the C++ linker toolchain for your platform (on Windows,
the "Desktop development with C++" Visual Studio workload; on Linux, `clang`; on macOS, Xcode
command line tools) - see https://aka.ms/nativeaot-prerequisites. Without it, `dotnet publish` fails
with "Platform linker not found" (this is a build-time-only requirement; the published binary itself
has no such dependency).

## Why binaries are committed at all

This is the one deliberate exception to "nothing generated is committed" (see the root `CLAUDE.md`,
"Committed vs git-ignored") - these binaries contain no OMG-derived content whatsoever (they're
compiled from this repository's own C# source, `tools/codex-cli/Uml4Net.Codex.Tools.Hook/`), and a
plugin install needs a ready-to-run hook without requiring the end user to have a .NET SDK and a
C++ linker toolchain installed just to use the plugin at all.
