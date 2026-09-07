// -------------------------------------------------------------------------------------------------
// <copyright file="XmiInspector.cs" company="Starion Group S.A.">
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

namespace Uml4Net.Sage.Tools.Inspection
{
    using System.Collections.Generic;
    using System.IO;
    using System.Text.Json;

    using Microsoft.Extensions.Logging;

    using uml4net.CommonStructure;
    using uml4net.Packages;
    using uml4net.xmi;

    /// <summary>
    /// Loads a user-supplied <c>.xmi</c>/<c>.uml</c> file via <c>uml4net.xmi</c> - the same reader the
    /// knowledge base itself is built with - and checks it against the generated metamodel.
    /// </summary>
    /// <remarks>
    /// Two checks are implemented in v1: (1) every reader-level warning/error (unresolved references,
    /// unknown elements/attributes) is surfaced as a finding; (2) any element whose declared metaclass is
    /// abstract in the UML 2.5.1 metamodel is flagged, since abstract metaclasses cannot be instantiated
    /// directly in valid XMI. Deeper semantic checks (multiplicity violations, redefinition/subsetting
    /// resolution) would require reflecting over uml4net's generated <c>[Property]</c> decorator metadata
    /// for arbitrary elements and are left as a documented fast-follow, not a v1 blocker.
    /// </remarks>
    public static class XmiInspector
    {
        /// <summary>
        /// Inspects <paramref name="modelPath"/> against the metamodel graph at <paramref name="metamodelJsonPath"/>.
        /// </summary>
        public static InspectionReport Inspect(string modelPath, string metamodelJsonPath, string umlVersion)
        {
            var loggerProvider = new CapturingLoggerProvider();
            using var loggerFactory = LoggerFactory.Create(builder => builder.AddProvider(loggerProvider).SetMinimumLevel(LogLevel.Warning));

            var reader = XmiReaderBuilder.Create()
                .UsingSettings(settings =>
                {
                    settings.LocalReferenceBasePath = Path.GetDirectoryName(Path.GetFullPath(modelPath)) ?? ".";
                    settings.UseStrictReading = false;
                })
                .WithLogger(loggerFactory)
                .Build();

            uml4net.xmi.Readers.XmiReaderResult result;
            using (reader)
            {
                result = reader.Read(modelPath);
            }

            var findings = new List<InspectionFinding>();

            foreach (var (level, message) in loggerProvider.Entries)
            {
                findings.Add(new InspectionFinding(level == LogLevel.Error ? "error" : "warning", "reader-diagnostic", null, message));
            }

            var abstractClassNames = LoadAbstractClassNames(metamodelJsonPath);
            foreach (var package in result.Packages)
            {
                WalkForAbstractInstantiation(package, abstractClassNames, findings);
            }

            return new InspectionReport(modelPath, umlVersion, findings);
        }

        private static void WalkForAbstractInstantiation(IPackage package, HashSet<string> abstractClassNames, List<InspectionFinding> findings)
        {
            foreach (var element in package.PackagedElement)
            {
                var metaclassName = element.GetType().Name;
                if (abstractClassNames.Contains(metaclassName))
                {
                    var name = (element as INamedElement)?.Name ?? "(unnamed)";
                    findings.Add(new InspectionFinding(
                        "error",
                        "abstract-instantiation",
                        null,
                        $"'{name}' is declared with xmi:type '{metaclassName}', which is abstract in the UML 2.5.1 metamodel and cannot be instantiated directly."));
                }

                if (element is IPackage nested)
                {
                    WalkForAbstractInstantiation(nested, abstractClassNames, findings);
                }
            }
        }

        private static HashSet<string> LoadAbstractClassNames(string metamodelJsonPath)
        {
            using var stream = File.OpenRead(metamodelJsonPath);
            using var document = JsonDocument.Parse(stream);

            var names = new HashSet<string>();
            foreach (var classElement in document.RootElement.GetProperty("classes").EnumerateArray())
            {
                if (classElement.GetProperty("isAbstract").GetBoolean())
                {
                    names.Add(classElement.GetProperty("name").GetString()!);
                }
            }

            return names;
        }
    }
}
