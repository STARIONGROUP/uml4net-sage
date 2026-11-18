# Changelog

All notable changes to uml4net-sage are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

This file starts after plugin 1.6.0 and tools 1.6.0. What those and earlier versions changed is in
the [GitHub releases](https://github.com/STARIONGROUP/uml4net-sage/releases) and the git history.

## [Unreleased]

### Added
- **Repository groundwork for the MCP refactor** (`refs #44`, part of #43): this changelog, a
  contributing guide, pull request and issue templates, and Dependabot updates for the GitHub
  Actions, the NuGet packages and the specification extractor's `uv.lock`.
- **CI runs on Linux, with SonarCloud and coverage** (`refs #44`): mycelium-hypha's build job, which
  builds and tests the solution, collects coverage for C# and Python, and fails when the tests leave
  a tracked file changed. The Python checks install from `uv.lock`, through the `uv` version the CLI
  pins, so they test the versions `uml4net-sage generate` installs. `pytest-cov` joins the
  extractor's development dependencies.

### Changed
- **Every source file's license header is two SPDX lines** (`refs #59`), as mycelium-hypha's:
  `SPDX-FileCopyrightText: 2026 Starion Group S.A.` and `SPDX-License-Identifier: Apache-2.0`
  replace the Apache-2.0 `<copyright>` block in the C# files and are added to the Python files. The
  root `.editorconfig` writes the header into new C# files.
- **The projects take mycelium-hypha's names and shape** (`refs #59`): `Uml4Net.Sage.MetamodelGen`
  is now `Uml4Net.Sage.Knowledge.Metamodel`, the model inspection is its own library,
  `Uml4Net.Sage.Inspection`, and the project files, test projects and solution folders follow
  hypha's. The CLI and its output do not change.

### Removed
- **The SessionStart hook's source and its workflow** (`refs #59`): the MCP refactor replaces the
  hook with a launcher. Its binaries stay in `hooks/native/` for plugin 1.x until then; their source
  is at the tag `v1.6.0`.

### Fixed
- **Verbatim specification text through `uv` on Linux and macOS** (`refs #44`): the `uv` the CLI
  downloads when no Python is found was extracted and then reported missing, because uv's tar.gz
  archives hold the executable in a `uv-<target>` folder. Without a Python of the user's own,
  `uml4net-sage generate` therefore skipped the specification text there. The provisioner and its
  tests are now mycelium-hypha's, which had fixed this; Sage's copy dated from before the fix.

[Unreleased]: https://github.com/STARIONGROUP/uml4net-sage/compare/v1.6.0...HEAD
