---
name: knowledge-setup
description: Check whether the UML 2.5.1 knowledge base is fetched/generated, and fetch and/or generate it. Use when another skill reports the knowledge base is missing or incomplete, when the user asks to set up/update/refresh the UML knowledge, or when a SessionStart hook message mentions uml4net-sage needing setup.
---

# Knowledge setup

Manages the local, git-ignored knowledge base under `knowledge/<version>/`, built from files
fetched directly from omg.org (never committed - see the root `CLAUDE.md`). Much smaller in scope
than a typical "version management" skill: there is one relevant UML version today (2.5.1), fetched
from a fixed, static set of OMG URLs - there is no "check for updates" step, because nothing to poll
against exists (unlike SysML v2's rolling releases).

## Checking status

```bash
uml4net-sage check --json
```

Fully offline (no network call) - reads `knowledge/installed.json` and file existence only. Reports
whether any version is fetched, whether the default is generated, and whether spec text (verbatim
citation) is available.

```bash
uml4net-sage versions --json
```

Lists every UML version this tool knows about (today, just 2.5.1) alongside its local state.

## Fetching and generating

Both of these make network calls / write files - **confirm with the user before running them**
unless they've clearly already asked for this ("set up the UML knowledge base", "fetch UML 2.5.1").

```bash
uml4net-sage fetch --version 2.5.1           # XMI + specification PDFs from omg.org
uml4net-sage fetch --version 2.5.1 --no-specs # XMI only - faster, but spec-citation degrades to clause numbers
uml4net-sage generate --version 2.5.1         # metamodel + standard-profile + (if PDFs fetched) spec text + datapackage.json
```

`generate` never fails outright if spec extraction can't run (PDFs missing, or Python/the
`spec_extract` package not installed) - it prints which step it skipped and why, and still produces
a fully usable metamodel-lookup and standard-profile-lookup knowledge base.

## Switching or removing versions

Only relevant once more than one UML version is ever installed (there's no reason to today, but the
CLI supports it for when OMG eventually ships a new version):

```bash
uml4net-sage use --version <version>              # switch the default; refuses unless generated
uml4net-sage remove --version <version> [--force] # delete a version's sources + knowledge base
```

## Answering

- Always report which version(s) are installed and which is the default after a status check.
- After `fetch`/`generate`, tell the user what's now possible (e.g. "verbatim spec citation is now
  available" or "spec citation is still limited to clause numbers - PDFs weren't fetched").
