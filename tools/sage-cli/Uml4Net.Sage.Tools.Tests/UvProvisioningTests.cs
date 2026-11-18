// ------------------------------------------------------------------------------------------------
// <copyright file="UvProvisioningTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Uml4Net.Sage.Tools.Tests
{
    using System;
    using System.IO;
    using System.Linq;

    using Uml4Net.Sage.Knowledge.Toolchain;
    using Uml4Net.Sage.Tools.Commands;

    [TestFixture]
    public class UvProvisioningTests
    {
        [Test]
        public void The_cache_stays_where_the_CLI_has_kept_uv_since_1_x()
        {
            var expected = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "uml4net-sage");

            Assert.That(UvProvisioning.CacheRoot.FullName, Is.EqualTo(expected));
        }

        [Test]
        public void The_GitHub_client_addresses_the_API_with_a_user_agent()
        {
            // UvProvisioner asks for its release by a path relative to the GitHub API, and GitHub
            // refuses a request without a user agent.
            using var client = UvProvisioning.CreateGitHubClient();

            Assert.That(client.BaseAddress, Is.EqualTo(new Uri("https://api.github.com/")));
            Assert.That(client.DefaultRequestHeaders.UserAgent.Select(product => product.Product?.Name), Does.Contain("uml4net-sage"));
        }

        [Test]
        public void The_provisioner_caches_under_the_cache_root_and_leaves_uv_its_default_locations()
        {
            using var client = UvProvisioning.CreateGitHubClient();

            var provisioner = UvProvisioning.Create(client);

            Assert.That(
                provisioner.BinDirectory(UvProvisioner.PinnedVersion, "x86_64-pc-windows-msvc").FullName,
                Is.EqualTo(Path.Combine(UvProvisioning.CacheRoot.FullName, "uv", UvProvisioner.PinnedVersion, "x86_64-pc-windows-msvc")));
            Assert.That(provisioner.Environment, Is.Empty);
        }
    }
}
