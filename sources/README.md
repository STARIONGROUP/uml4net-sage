# sources/

Raw files fetched directly from omg.org by `uml4net-sage fetch`. **Nothing under `sources/` is
ever committed to this repository** - see the root `CLAUDE.md` ("Committed vs git-ignored") and
`NOTICE`. Only this README (which contains no OMG content, just URLs and licensing notes) is
committed.

## Provenance (UML 2.5.1)

| File | Fetched from | OMG document ID | License |
|---|---|---|---|
| `xmi/UML.xmi` | https://www.omg.org/spec/UML/20161101/UML.xmi | ptc/18-01-01 | OMG specification license (see `NOTICE`) |
| `xmi/PrimitiveTypes.xmi` | https://www.omg.org/spec/UML/20161101/PrimitiveTypes.xmi | ptc/18-01-02 | OMG specification license |
| `xmi/StandardProfile.xmi` | https://www.omg.org/spec/UML/20161101/StandardProfile.xmi | ptc/18-01-03 | OMG specification license |
| `xmi/UMLDI.xmi` | https://www.omg.org/spec/UML/20161101/UMLDI.xmi | ptc/18-01-04 | OMG specification license |
| `specs/UML-2.5.1.pdf` | https://www.omg.org/spec/UML/2.5.1/PDF | formal/17-12-05 | OMG specification license |
| `specs/UML-2.5.1-changebar.pdf` | https://www.omg.org/spec/UML/2.5.1/PDF/changebar | formal/17-12-06 (informative) | OMG specification license |

Full OMG licensing terms: https://doc.omg.org/ipr (RF-Limited IPR mode for UML 2.5.1). The
verbatim "USE OF SPECIFICATION" terms are reproduced in the root `NOTICE` file.

Unlike SysML v2 (whose pilot-implementation XMI is mirrored on GitHub under EPL-2.0), UML has no
GitHub-hosted mirror - every file above is fetched with a plain HTTPS GET directly from omg.org, no
API pagination or tag discovery involved. See `Uml4Net.Sage.Knowledge.SourceFetcher` and
`KnownUmlVersions` for the exact URLs (kept in sync with this table by hand).

`UMLDI.xmi` (the Diagram Interchange metamodel) is fetched for completeness but not yet mined into
the generated knowledge base - UML users overwhelmingly ask about the abstract syntax metamodel and
Standard Profile, not diagram interchange. A dedicated skill/generator for it is a possible
fast-follow, not a v1 gap that blocks anything.

## Provenance (XMI 2.5.1)

UML models are themselves serialized using XMI, an OMG standard independent of UML - it versions on
its own schedule, even though both happen to be at "2.5.1" today. Its specification PDF is fetched
into a top-level sibling directory rather than under `sources/2.5.1/`, since it isn't tied to any one
UML version:

| File | Fetched from | OMG document ID | License |
|---|---|---|---|
| `xmi/2.5.1/specs/XMI-2.5.1.pdf` | https://www.omg.org/spec/XMI/2.5.1/PDF | formal/15-06-07 | OMG specification license (see `NOTICE`) |

Full OMG licensing terms as above. See `Uml4Net.Sage.Knowledge.KnownXmiVersions` for the exact URL.
Fetched alongside the UML sources whenever `fetch`/`generate` run without `--no-specs`; unlike the
UML XMI/PDF files above, a failure to fetch this file degrades to a warning rather than aborting UML
setup, since it's a supplementary corpus.

## Test fixtures

No file under `sources/` (or anything derived from it) is used as a committed test fixture -
`Uml4Net.Sage.MetamodelGen.Tests` uses small, hand-authored XMI files under its own `Fixtures/`
directory instead, describing a fictional toy metamodel (`Widget`/`Gadget`/`Sample`), specifically
to avoid any question about whether OMG's XMI may be redistributed as test data.
