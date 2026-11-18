// SPDX-FileCopyrightText: 2026 Starion Group S.A.
// SPDX-License-Identifier: Apache-2.0

namespace Uml4Net.Sage.Knowledge.Metamodel.Tests
{
    using System.IO;

    /// <summary>
    /// Shared access to the hand-authored (not OMG-derived) fixture XMI files under <c>Fixtures/Xmi/</c>,
    /// used instead of committing any real UML 2.5.1 XMI - see the root CLAUDE.md's licensing discussion.
    /// </summary>
    public static class TestFixtures
    {
        public static string XmiDirectory => Path.Combine(NUnit.Framework.TestContext.CurrentContext.TestDirectory, "Fixtures", "Xmi");

        /// <summary>
        /// Reads <c>Fixture.xmi</c> and <c>FixtureProfile.xmi</c> and catalogs their elements.
        /// </summary>
        /// <returns>The <see cref="ElementCatalog"/> of the fixture metamodel and profile.</returns>
        public static ElementCatalog BuildCatalog()
        {
            var umlModel = XmiModelReader.Read(Path.Combine(XmiDirectory, "Fixture.xmi"));
            var profileModel = XmiModelReader.Read(Path.Combine(XmiDirectory, "FixtureProfile.xmi"));
            return ElementCatalog.Build(umlModel, profileModel);
        }
    }
}
