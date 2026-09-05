// -------------------------------------------------------------------------------------------------
// <copyright file="SourceFetcher.cs" company="Starion Group S.A.">
//
//   Copyright (C) 2019-2026 Starion Group S.A.
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Uml4Net.Codex.Knowledge
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Security.Cryptography;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;

    using Polly;
    using Polly.Retry;

    /// <summary>
    /// Downloads the OMG UML XMI and (optionally) PDF files for a given <see cref="UmlVersionDescriptor"/>
    /// via plain HTTPS GETs - there is no GitHub mirror or API for UML, unlike SysML v2, so this is a direct
    /// single-file-at-a-time download loop with retry, not a tag-discovery/tree-listing client.
    /// </summary>
    public sealed class SourceFetcher
    {
        private static readonly JsonSerializerOptions SerializerOptions = new() { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        private readonly HttpClient httpClient;
        private readonly AsyncRetryPolicy retryPolicy;

        /// <summary>
        /// Initializes a new instance of the <see cref="SourceFetcher"/> class.
        /// </summary>
        /// <param name="httpClient">
        /// The <see cref="HttpClient"/> to issue downloads with (injectable so tests can supply a stubbed handler).
        /// </param>
        public SourceFetcher(HttpClient httpClient)
        {
            this.httpClient = httpClient;
            this.retryPolicy = Policy
                .Handle<HttpRequestException>()
                .WaitAndRetryAsync(3, attempt => TimeSpan.FromMilliseconds(250 * Math.Pow(2, attempt)));
        }

        /// <summary>
        /// Downloads the XMI files (and, unless <paramref name="includeSpecs"/> is false, the PDFs) for
        /// <paramref name="descriptor"/> into <paramref name="sourcesRootDirectory"/>/&lt;version&gt;/, and
        /// records their SHA-256 and fetch time in <c>fetch-manifest.json</c>.
        /// </summary>
        public async Task<IReadOnlyList<FetchManifestEntry>> FetchAsync(UmlVersionDescriptor descriptor, string sourcesRootDirectory, bool includeSpecs, CancellationToken cancellationToken = default)
        {
            var versionDirectory = Path.Combine(sourcesRootDirectory, descriptor.Version);
            var xmiDirectory = Path.Combine(versionDirectory, "xmi");
            var specsDirectory = Path.Combine(versionDirectory, "specs");

            var downloads = new List<(string Url, string DestinationPath)>
            {
                (descriptor.MetamodelXmiUrl, Path.Combine(xmiDirectory, "UML.xmi")),
                (descriptor.PrimitiveTypesXmiUrl, Path.Combine(xmiDirectory, "PrimitiveTypes.xmi")),
                (descriptor.StandardProfileXmiUrl, Path.Combine(xmiDirectory, "StandardProfile.xmi")),
                (descriptor.DiagramInterchangeXmiUrl, Path.Combine(xmiDirectory, "UMLDI.xmi")),
            };

            if (includeSpecs)
            {
                downloads.Add((descriptor.SpecificationPdfUrl, Path.Combine(specsDirectory, $"UML-{descriptor.Version}.pdf")));
                downloads.Add((descriptor.ChangebarPdfUrl, Path.Combine(specsDirectory, $"UML-{descriptor.Version}-changebar.pdf")));
            }

            var entries = new List<FetchManifestEntry>();
            foreach (var (url, destinationPath) in downloads)
            {
                entries.Add(await this.DownloadOneAsync(url, destinationPath, cancellationToken));
            }

            File.WriteAllText(Path.Combine(versionDirectory, "fetch-manifest.json"), JsonSerializer.Serialize(entries, SerializerOptions));

            return entries;
        }

        private async Task<FetchManifestEntry> DownloadOneAsync(string url, string destinationPath, CancellationToken cancellationToken)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);

            var bytes = await this.retryPolicy.ExecuteAsync(async () =>
            {
                using var response = await this.httpClient.GetAsync(url, cancellationToken);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsByteArrayAsync(cancellationToken);
            });

            await File.WriteAllBytesAsync(destinationPath, bytes, cancellationToken);

            var sha256 = Convert.ToHexStringLower(SHA256.HashData(bytes));

            return new FetchManifestEntry(url, Path.GetFileName(destinationPath), sha256, DateTimeOffset.UtcNow);
        }
    }
}
