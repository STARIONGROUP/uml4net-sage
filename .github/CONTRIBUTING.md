# How to contribute

Thank you for wanting to contribute to uml4net-sage. A few guidelines keep the history readable and
the review quick.

- [Making changes](#making-changes)
  - [Keeping up with development](#keeping-up-with-development)
  - [Sending a pull request](#sending-a-pull-request)
- [Building and testing](#building-and-testing)
  - [Prerequisites](#prerequisites)
  - [.NET](#net)
  - [Python](#python)
- [Style guidelines](#style-guidelines)
- [What may be committed](#what-may-be-committed)

## Making changes

1. [Fork](https://docs.github.com/en/pull-requests/collaborating-with-pull-requests/working-with-forks/fork-a-repo) the repository on GitHub, and clone your fork
1. Configure the upstream repository (`git remote add upstream https://github.com/STARIONGROUP/uml4net-sage.git`)
1. Check out `development`
1. Create a branch from `development` named after the issue it addresses: `feature/gh-<issue-number>-<short-name>` for a feature, `fix/gh-<issue-number>-<short-name>` for a fix
1. Work on your change, and rebase if required (see below)
1. Push the branch to your fork
1. Send a pull request into `development`

Never work on, or send a pull request from, `development` itself: always use a branch.

### Keeping up with development

When `development` moves while you work:

1. Stash any uncommitted changes
1. `git checkout development`
1. `git pull upstream development`
1. `git checkout <your-branch>`
1. `git rebase development`

This keeps the history a straight line from `development` followed by your changes, without merge
commits.

### Sending a pull request

Before sending it, rebase on `development` (above), so the pull request holds only your changes. Then
send a descriptive pull request into `development` and wait for a maintainer to review it.

Commit subjects follow `[Verb] <summary>; fixes #<n>` (or `refs #<n>` when the commit does not close
the issue), for example `[Fix] the uv provisioner finds uv in the tar.gz archives of Linux and macOS;
refs #44`. Common verbs are `Add`, `Update`, `Fix`, `Remove` and `Refactor`. Add a line to the
`[Unreleased]` section of `CHANGELOG.md` for anything a user of the plugin or the CLI would notice.

A pull request with tests is a pull request that is likely to be merged.

## Building and testing

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download), for everything under `tools/` except `tools/spec-extract`
- Python 3.12 or later and [uv](https://docs.astral.sh/uv/), for `tools/spec-extract`

### .NET

From the repository root:

```sh
dotnet build uml4net-sage.slnx
dotnet test uml4net-sage.slnx
```

A test run reaches no network and leaves the working tree clean; CI fails a pull request whose tests
change a tracked file.

### Python

From `tools/spec-extract/`:

```sh
uv sync --extra dev                  # .venv from uv.lock: the versions CI and uml4net-sage generate use
uv run --extra dev ruff check .
uv run --extra dev ruff format --check .
uv run --extra dev pytest
```

Without uv, `python -m venv .venv` and `pip install -e ".[dev]"` also work, but install the newest
versions rather than the locked ones, so ruff and pytest may differ from CI.

## Style guidelines

The full conventions are in [`CLAUDE.md`](../CLAUDE.md), "Conventions". In short:

- **C#:** 4 spaces, no tabs; no `_` prefix on members, and `this.` for instance members; `var`
  unless the type is ambiguous; braces on every block; no `#region`; `using`s inside the namespace,
  `System.*` first. Every public member has its own XML documentation (no `<inheritdoc/>`); test
  methods and fixtures have none. Every `.cs` file opens with the Apache 2.0 copyright header.
- **Python** exists only in `tools/spec-extract`: ruff-clean and ruff-formatted.
- **Generated output** is byte-stable: ordinal sorting, LF line endings, fixed JSON options, and no
  timestamps, machine paths or culture-sensitive formatting.

## What may be committed

OMG permits informational use of its specifications but not their redistribution (see `NOTICE`).
So nothing fetched from omg.org (`sources/<version>/`) and nothing generated from it
(`knowledge/<version>/`) is ever committed, and test fixtures are hand-written toy models, never
OMG's own files. See `CLAUDE.md`, "Committed vs git-ignored".
