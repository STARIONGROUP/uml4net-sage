// ------------------------------------------------------------------------------------------------
// <copyright file="UvProvisioning.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Uml4Net.Sage.Tools.Commands
{
    using System;
    using System.IO;
    using System.Net.Http;

    using Microsoft.Extensions.Logging.Abstractions;

    using Uml4Net.Sage.Knowledge.Toolchain;

    /// <summary>
    /// Builds the <see cref="UvProvisioner"/> the <c>fetch</c> and <c>generate</c> verbs run the
    /// specification extractor through, with what mycelium-hypha's provisioner gets from dependency
    /// injection: an HTTP client for the GitHub API, a logger and a cache folder.
    /// </summary>
    /// <remarks>
    /// Interim, until Phase 1 of the MCP refactor (#45) composes the services through dependency
    /// injection, with the named upstream client, the CLI's logger and <c>toolchain/</c> under the home
    /// folder as the cache root. Until then the cache stays where the CLI has kept it since 1.x.
    /// </remarks>
    public static class UvProvisioning
    {
        /// <summary>
        /// Gets the folder <c>uv</c> is cached under: <c>uml4net-sage</c> in the per-user local
        /// application data folder.
        /// </summary>
        public static DirectoryInfo CacheRoot =>
            new(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "uml4net-sage"));

        /// <summary>
        /// Creates an HTTP client for the GitHub API, which the provisioner reads uv's release from.
        /// </summary>
        /// <returns>A client whose base address is the GitHub API, with the user agent GitHub requires.</returns>
        public static HttpClient CreateGitHubClient()
        {
            var client = new HttpClient { BaseAddress = new Uri("https://api.github.com/") };
            client.DefaultRequestHeaders.UserAgent.ParseAdd("uml4net-sage");

            return client;
        }

        /// <summary>
        /// Creates the provisioner over <paramref name="gitHubClient"/>.
        /// </summary>
        /// <param name="gitHubClient">A client from <see cref="CreateGitHubClient"/>.</param>
        /// <returns>A provisioner that caches under <see cref="CacheRoot"/> and logs nowhere.</returns>
        public static UvProvisioner Create(HttpClient gitHubClient) =>
            new(gitHubClient, NullLogger<UvProvisioner>.Instance, CacheRoot);
    }
}
