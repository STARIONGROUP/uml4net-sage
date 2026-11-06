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

namespace Uml4Net.Sage.Knowledge
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
        /// <param name="descriptor">The UML version whose files to download.</param>
        /// <param name="sourcesRootDirectory">The <c>sources/</c> root directory.</param>
        /// <param name="includeSpecs">Whether to download the specification and changebar PDFs as well as the XMI files.</param>
        /// <param name="cancellationToken">Cancels the downloads.</param>
        /// <returns>One manifest entry per downloaded file, in download order.</returns>
        /// <exception cref="HttpRequestException">A download still fails after the retries.</exception>
        /// <exception cref="InvalidDataException">A <c>.pdf</c> download does not start with the PDF magic bytes.</exception>
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

        /// <summary>
        /// Downloads the companion OMG XMI specification PDF and its two machine-readable schema files
        /// (<c>XMI.xsd</c>, <c>XMI-Canonical.xsd</c>) for <paramref name="descriptor"/> into
        /// <paramref name="sourcesRootDirectory"/>/xmi/&lt;version&gt;/, and records them in their own
        /// <c>fetch-manifest.json</c>. Kept separate from <see cref="FetchAsync"/> rather than folded into
        /// it: the XMI spec is a top-level sibling corpus (see <c>knowledge/xmi/&lt;version&gt;/</c>), not
        /// tied to any one UML version's directory.
        /// </summary>
        /// <param name="descriptor">The XMI specification version whose files to download.</param>
        /// <param name="sourcesRootDirectory">The <c>sources/</c> root directory.</param>
        /// <param name="cancellationToken">Cancels the downloads.</param>
        /// <returns>One manifest entry per downloaded file, in download order.</returns>
        /// <exception cref="HttpRequestException">A download still fails after the retries.</exception>
        /// <exception cref="InvalidDataException">
        /// The PDF does not start with the PDF magic bytes, or a schema file does not look like XML.
        /// </exception>
        public async Task<IReadOnlyList<FetchManifestEntry>> FetchXmiSpecAsync(XmiSpecDescriptor descriptor, string sourcesRootDirectory, CancellationToken cancellationToken = default)
        {
            var versionDirectory = Path.Combine(sourcesRootDirectory, "xmi", descriptor.Version);
            var specsDirectory = Path.Combine(versionDirectory, "specs");
            var schemaDirectory = Path.Combine(versionDirectory, "schema");

            var downloads = new List<(string Url, string DestinationPath)>
            {
                (descriptor.SpecificationPdfUrl, Path.Combine(specsDirectory, $"XMI-{descriptor.Version}.pdf")),
                (descriptor.XsdUrl, Path.Combine(schemaDirectory, "XMI.xsd")),
                (descriptor.CanonicalXsdUrl, Path.Combine(schemaDirectory, "XMI-Canonical.xsd")),
            };

            var entries = new List<FetchManifestEntry>();
            foreach (var (url, destinationPath) in downloads)
            {
                entries.Add(await this.DownloadOneAsync(url, destinationPath, cancellationToken));
            }

            File.WriteAllText(Path.Combine(versionDirectory, "fetch-manifest.json"), JsonSerializer.Serialize(entries, SerializerOptions));

            return entries;
        }

        /// <summary>
        /// Downloads one file with retry, checks that a <c>.pdf</c> or <c>.xsd</c> response has the
        /// expected content, writes it to <paramref name="destinationPath"/> and hashes it.
        /// </summary>
        /// <param name="url">The URL to download.</param>
        /// <param name="destinationPath">The file to write; its directory is created if needed.</param>
        /// <param name="cancellationToken">Cancels the download and the write.</param>
        /// <returns>The file's manifest entry, with its SHA-256 and the current UTC time.</returns>
        /// <exception cref="HttpRequestException">The download still fails after the retries.</exception>
        /// <exception cref="InvalidDataException">The response is not a PDF or not XML, as the file extension requires.</exception>
        private async Task<FetchManifestEntry> DownloadOneAsync(string url, string destinationPath, CancellationToken cancellationToken)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);

            var bytes = await this.retryPolicy.ExecuteAsync(async () =>
            {
                using var response = await this.httpClient.GetAsync(url, cancellationToken);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsByteArrayAsync(cancellationToken);
            });

            if (destinationPath.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase) && !HasPdfMagicBytes(bytes))
            {
                throw new InvalidDataException(
                    $"{url} did not return a PDF file (expected the first bytes to be \"%PDF\") - the server may have returned an error page instead of the document.");
            }

            if (destinationPath.EndsWith(".xsd", StringComparison.OrdinalIgnoreCase) && !LooksLikeXml(bytes))
            {
                throw new InvalidDataException(
                    $"{url} did not return an XML file (expected to find \"<\" after any leading whitespace/BOM) - the server may have returned an error page instead of the schema.");
            }

            await File.WriteAllBytesAsync(destinationPath, bytes, cancellationToken);

            var sha256 = Convert.ToHexStringLower(SHA256.HashData(bytes));

            return new FetchManifestEntry(url, Path.GetFileName(destinationPath), sha256, DateTimeOffset.UtcNow);
        }

        /// <summary>
        /// Checks whether <paramref name="bytes"/> starts with <c>%PDF</c>, the header every PDF file opens with.
        /// </summary>
        /// <param name="bytes">The downloaded content.</param>
        /// <returns><c>true</c> when the content starts with <c>%PDF</c>; otherwise <c>false</c>.</returns>
        private static bool HasPdfMagicBytes(byte[] bytes)
        {
            ReadOnlySpan<byte> pdfMagic = "%PDF"u8;
            return bytes.Length >= pdfMagic.Length && bytes.AsSpan(0, pdfMagic.Length).SequenceEqual(pdfMagic);
        }

        /// <summary>
        /// A loose "is this actually XML" sanity check - unlike a PDF, XML has no fixed magic bytes, so
        /// this just confirms the first non-whitespace byte (after an optional UTF-8 BOM) is "&lt;", which
        /// every well-formed XML document starts with (a leading <c>&lt;?xml ...?&gt;</c> declaration is
        /// optional but a document must still open with an element). Deliberately weaker than
        /// <see cref="HasPdfMagicBytes"/>: an HTML error page often also starts with "&lt;" (e.g.
        /// <c>&lt;!DOCTYPE html&gt;</c>), so this only reliably catches responses that aren't even
        /// XML-shaped at all (empty, plain text, JSON) - not a mistaken HTML response.
        /// </summary>
        /// <param name="bytes">The downloaded content.</param>
        /// <returns><c>true</c> when the first non-whitespace byte is "&lt;"; otherwise <c>false</c>.</returns>
        private static bool LooksLikeXml(byte[] bytes)
        {
            var span = bytes.AsSpan();
            if (span.Length >= 3 && span[0] == 0xEF && span[1] == 0xBB && span[2] == 0xBF)
            {
                span = span[3..];
            }

            var index = 0;
            while (index < span.Length && char.IsWhiteSpace((char)span[index]))
            {
                index++;
            }

            return index < span.Length && span[index] == (byte)'<';
        }
    }
}
