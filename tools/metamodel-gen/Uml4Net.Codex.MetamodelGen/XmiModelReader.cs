// -------------------------------------------------------------------------------------------------
// <copyright file="XmiModelReader.cs" company="Starion Group S.A.">
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

namespace Uml4Net.Codex.MetamodelGen
{
    using System.IO;

    using Microsoft.Extensions.Logging.Abstractions;

    using uml4net.xmi;
    using uml4net.xmi.Readers;

    /// <summary>
    /// A thin wrapper around <see cref="XmiReaderBuilder"/> for reading the OMG UML 2.5.1 metamodel XMI files.
    /// </summary>
    /// <remarks>
    /// UML.xmi, PrimitiveTypes.xmi, StandardProfile.xmi and UMLDI.xmi cross-reference each other using
    /// absolute http(s) hrefs (e.g. <c>http://www.omg.org/spec/UML/20161101/UML.xmi#Class</c>), not
    /// <c>pathmap://</c> URIs. <c>uml4net.xmi</c>'s <see cref="uml4net.xmi.ReferenceResolver.ExternalReferenceResolver"/>
    /// resolves such an href by looking for a local file with the same file name (here, "UML.xmi") under
    /// <see cref="uml4net.xmi.Settings.IXmiReaderSettings.LocalReferenceBasePath"/> - so as long as all four
    /// fetched files sit in one directory under their original file names, no <c>PathMaps</c> entries are needed.
    /// </remarks>
    public static class XmiModelReader
    {
        /// <summary>
        /// Reads a UML metamodel XMI file, resolving its cross-references against sibling files in the same directory.
        /// </summary>
        /// <param name="filePath">
        /// The path of the XMI file to read (e.g. ".../xmi/UML.xmi").
        /// </param>
        /// <param name="localReferenceBasePath">
        /// The directory containing the sibling XMI files (UML.xmi, PrimitiveTypes.xmi, StandardProfile.xmi, UMLDI.xmi)
        /// that <paramref name="filePath"/> may cross-reference. Defaults to <paramref name="filePath"/>'s own directory.
        /// </param>
        /// <returns>
        /// The <see cref="XmiReaderResult"/> produced by reading <paramref name="filePath"/>.
        /// </returns>
        public static XmiReaderResult Read(string filePath, string? localReferenceBasePath = null)
        {
            var basePath = localReferenceBasePath ?? Path.GetDirectoryName(Path.GetFullPath(filePath)) ?? ".";

            var reader = XmiReaderBuilder.Create()
                .UsingSettings(settings =>
                {
                    settings.LocalReferenceBasePath = basePath;
                    settings.UseStrictReading = false;
                })
                .WithLogger(NullLoggerFactory.Instance)
                .Build();

            using (reader)
            {
                return reader.Read(filePath);
            }
        }
    }
}
