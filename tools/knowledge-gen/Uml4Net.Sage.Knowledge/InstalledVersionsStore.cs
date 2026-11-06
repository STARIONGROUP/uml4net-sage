// -------------------------------------------------------------------------------------------------
// <copyright file="InstalledVersionsStore.cs" company="Starion Group S.A.">
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
    using System.IO;
    using System.Linq;
    using System.Text.Json;

    /// <summary>
    /// Reads and writes <c>knowledge/installed.json</c>, the local record of which UML versions are
    /// fetched/generated and which is the default.
    /// </summary>
    public sealed class InstalledVersionsStore
    {
        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        private readonly string manifestPath;

        /// <summary>
        /// Initializes a new instance bound to the given knowledge-base root directory.
        /// </summary>
        /// <param name="knowledgeRootDirectory">
        /// The <c>knowledge/</c> directory (containing the version subfolders and this manifest).
        /// </param>
        public InstalledVersionsStore(string knowledgeRootDirectory)
        {
            this.manifestPath = Path.Combine(knowledgeRootDirectory, "installed.json");
        }

        /// <summary>
        /// Loads the manifest, or an empty one if it doesn't exist yet.
        /// </summary>
        /// <returns>The manifest read from disk, with a missing <c>XmiSpecs</c> list replaced by an empty one.</returns>
        /// <exception cref="InvalidDataException">The manifest exists but is not valid JSON.</exception>
        public InstalledVersionsManifest Load()
        {
            if (!File.Exists(this.manifestPath))
            {
                return InstalledVersionsManifest.Empty;
            }

            var json = File.ReadAllText(this.manifestPath);
            InstalledVersionsManifest manifest;

            try
            {
                manifest = JsonSerializer.Deserialize<InstalledVersionsManifest>(json, SerializerOptions) ?? InstalledVersionsManifest.Empty;
            }
            catch (JsonException exception)
            {
                throw new InvalidDataException(
                    $"{this.manifestPath} is not valid JSON ({exception.Message}). Delete it and run 'uml4net-sage fetch' and 'generate' again to rebuild it.",
                    exception);
            }

            // installed.json written before XmiSpecs existed deserializes that property as null, not [] -
            // coalesce so every other member never has to null-check it.
            return manifest.XmiSpecs is null ? manifest with { XmiSpecs = [] } : manifest;
        }

        /// <summary>
        /// Writes the manifest.
        /// </summary>
        /// <param name="manifest">The manifest to write, replacing any existing file.</param>
        public void Save(InstalledVersionsManifest manifest)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(this.manifestPath)!);
            File.WriteAllText(this.manifestPath, JsonSerializer.Serialize(manifest, SerializerOptions));
        }

        /// <summary>
        /// Records that a version's sources have been fetched, creating or updating its entry.
        /// Sets it as the default when <paramref name="setAsDefault"/> is true or no default is set yet.
        /// </summary>
        /// <param name="version">The UML version that was fetched.</param>
        /// <param name="setAsDefault">Whether to make <paramref name="version"/> the default even when one is already set.</param>
        public void MarkFetched(string version, bool setAsDefault)
        {
            var manifest = this.Load();
            var updated = Upsert(manifest, version, entry => entry with { Fetched = true });
            var newDefault = setAsDefault || manifest.Default is null ? version : manifest.Default;
            this.Save(updated with { Default = newDefault });
        }

        /// <summary>
        /// Records that a version's metamodel/standard-profile knowledge has been generated.
        /// </summary>
        /// <param name="version">The UML version that was generated.</param>
        /// <param name="specGenerated">
        /// Whether the specification text was extracted too. Once recorded as <c>true</c>, a later
        /// <c>false</c> does not clear it.
        /// </param>
        public void MarkGenerated(string version, bool specGenerated)
        {
            var manifest = this.Load();
            var updated = Upsert(manifest, version, entry => entry with { Generated = true, SpecGenerated = entry.SpecGenerated || specGenerated });
            this.Save(updated);
        }

        /// <summary>
        /// Sets the default version, refusing if it isn't recorded as generated.
        /// </summary>
        /// <param name="version">The UML version to make the default.</param>
        /// <exception cref="System.InvalidOperationException"><paramref name="version"/> is not recorded as generated.</exception>
        public void SetDefault(string version)
        {
            var manifest = this.Load();
            var entry = manifest.Versions.FirstOrDefault(v => v.Version == version);
            if (entry is null || !entry.Generated)
            {
                throw new System.InvalidOperationException($"UML {version} has not been generated yet - run 'generate --version {version}' first.");
            }

            this.Save(manifest with { Default = version });
        }

        /// <summary>
        /// Removes a version's entry entirely. If it was the default, the first remaining version
        /// becomes the default, or none if no versions remain.
        /// </summary>
        /// <param name="version">The UML version to remove.</param>
        public void Remove(string version)
        {
            var manifest = this.Load();
            var remaining = manifest.Versions.Where(v => v.Version != version).ToList();
            var newDefault = manifest.Default == version ? remaining.FirstOrDefault()?.Version : manifest.Default;
            this.Save(manifest with { Versions = remaining, Default = newDefault });
        }

        /// <summary>
        /// Records that the companion OMG XMI specification's PDF has been fetched, creating or updating
        /// its entry. Tracked independently of any UML version - see <see cref="InstalledXmiSpec"/>.
        /// </summary>
        /// <param name="version">The XMI specification version that was fetched.</param>
        public void MarkXmiSpecFetched(string version)
        {
            var manifest = this.Load();
            var updated = UpsertXmiSpec(manifest, version, entry => entry with { Fetched = true });
            this.Save(updated);
        }

        /// <summary>
        /// Records whether the companion OMG XMI specification's clause text has been extracted. Sticky
        /// like <see cref="InstalledVersion.SpecGenerated"/>: once <see langword="true"/>, a later failed
        /// attempt does not clear it back to <see langword="false"/>.
        /// </summary>
        /// <param name="version">The XMI specification version that extraction ran for.</param>
        /// <param name="succeeded">Whether the extraction succeeded.</param>
        public void MarkXmiSpecGenerated(string version, bool succeeded)
        {
            var manifest = this.Load();
            var updated = UpsertXmiSpec(manifest, version, entry => entry with { Generated = entry.Generated || succeeded });
            this.Save(updated);
        }

        /// <summary>
        /// Applies <paramref name="update"/> to a UML version's entry, creating an all-false entry first
        /// when none exists, and keeps the entries sorted by version.
        /// </summary>
        /// <param name="manifest">The manifest to update.</param>
        /// <param name="version">The UML version whose entry to change.</param>
        /// <param name="update">Produces the new entry from the existing or newly created one.</param>
        /// <returns>A copy of <paramref name="manifest"/> with the updated entry.</returns>
        private static InstalledVersionsManifest Upsert(InstalledVersionsManifest manifest, string version, System.Func<InstalledVersion, InstalledVersion> update)
        {
            var existing = manifest.Versions.FirstOrDefault(v => v.Version == version);
            var updatedEntry = update(existing ?? new InstalledVersion(version, false, false, false));

            var versions = manifest.Versions.Where(v => v.Version != version).Append(updatedEntry).OrderBy(v => v.Version, System.StringComparer.Ordinal).ToList();
            return manifest with { Versions = versions };
        }

        /// <summary>
        /// Applies <paramref name="update"/> to an XMI specification version's entry, creating an
        /// all-false entry first when none exists, and keeps the entries sorted by version.
        /// </summary>
        /// <param name="manifest">The manifest to update.</param>
        /// <param name="version">The XMI specification version whose entry to change.</param>
        /// <param name="update">Produces the new entry from the existing or newly created one.</param>
        /// <returns>A copy of <paramref name="manifest"/> with the updated entry.</returns>
        private static InstalledVersionsManifest UpsertXmiSpec(InstalledVersionsManifest manifest, string version, System.Func<InstalledXmiSpec, InstalledXmiSpec> update)
        {
            var existing = manifest.XmiSpecs.FirstOrDefault(v => v.Version == version);
            var updatedEntry = update(existing ?? new InstalledXmiSpec(version, false, false));

            var xmiSpecs = manifest.XmiSpecs.Where(v => v.Version != version).Append(updatedEntry).OrderBy(v => v.Version, System.StringComparer.Ordinal).ToList();
            return manifest with { XmiSpecs = xmiSpecs };
        }
    }
}
