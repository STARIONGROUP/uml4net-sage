// -------------------------------------------------------------------------------------------------
// <copyright file="PythonSpecExtractRunner.cs" company="Starion Group S.A.">
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
    using System.ComponentModel;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Invokes the <c>tools/spec-extract</c> Python package as a subprocess. This is the one place C#
    /// and Python meet in this repository: everything else is C# (see the root CLAUDE.md).
    /// </summary>
    public sealed class PythonSpecExtractRunner
    {
        private readonly IProcessRunner processRunner;

        /// <summary>
        /// Initializes a new instance of the <see cref="PythonSpecExtractRunner"/> class.
        /// </summary>
        public PythonSpecExtractRunner(IProcessRunner processRunner)
        {
            this.processRunner = processRunner;
        }

        /// <summary>
        /// Runs <c>python -m spec_extract extract</c> against <paramref name="pdfPath"/>, writing clause
        /// markdown and indexes into <paramref name="outputDirectory"/>. Never throws: any failure
        /// (Python not installed, the <c>spec_extract</c> package not installed, a non-zero exit code)
        /// results in a <see cref="SpecExtractionOutcome.Skipped"/> outcome with a human-readable reason.
        /// </summary>
        /// <param name="pdfPath">
        /// The locally fetched specification PDF (e.g. <c>sources/2.5.1/specs/UML-2.5.1.pdf</c>).
        /// </param>
        /// <param name="outputDirectory">
        /// The <c>spec/</c> output directory (e.g. <c>knowledge/2.5.1/spec</c>).
        /// </param>
        /// <param name="specExtractProjectDirectory">
        /// The <c>tools/spec-extract</c> directory, whose <c>.venv</c> (if present) is preferred over a
        /// system Python interpreter.
        /// </param>
        /// <param name="version">
        /// The UML version recorded in each clause's front matter.
        /// </param>
        public async Task<SpecExtractionOutcome> ExtractAsync(string pdfPath, string outputDirectory, string specExtractProjectDirectory, string version, CancellationToken cancellationToken = default)
        {
            if (!File.Exists(pdfPath))
            {
                return SpecExtractionOutcome.Skipped($"The specification PDF was not found at {pdfPath} - run 'fetch' without --no-specs first.");
            }

            var srcDirectory = Path.Combine(specExtractProjectDirectory, "src");
            if (!Directory.Exists(srcDirectory))
            {
                return SpecExtractionOutcome.Skipped($"The spec-extract project was not found at {specExtractProjectDirectory} - this repository checkout may be incomplete.");
            }

            foreach (var pythonExecutable in PythonCandidates(specExtractProjectDirectory))
            {
                try
                {
                    var result = await this.processRunner.RunAsync(
                        pythonExecutable,
                        ["-m", "spec_extract", "extract", "--pdf", pdfPath, "--out", outputDirectory, "--document", "UML", "--version", version],
                        workingDirectory: srcDirectory,
                        cancellationToken);

                    if (result.ExitCode != 0)
                    {
                        return SpecExtractionOutcome.Skipped($"spec_extract exited with code {result.ExitCode}: {result.StandardError}");
                    }

                    return SpecExtractionOutcome.Ok();
                }
                catch (Win32Exception)
                {
                    // This candidate interpreter isn't available - try the next one.
                }
            }

            return SpecExtractionOutcome.Skipped(
                "Python was not found. Verbatim spec citation is unavailable until it's installed - " +
                "see tools/spec-extract/README.md (Python >= 3.12, `pip install -e \".[dev]\"`).");
        }

        private static string[] PythonCandidates(string specExtractProjectDirectory)
        {
            var venvPython = OperatingSystem.IsWindows()
                ? Path.Combine(specExtractProjectDirectory, ".venv", "Scripts", "python.exe")
                : Path.Combine(specExtractProjectDirectory, ".venv", "bin", "python");

            if (File.Exists(venvPython))
            {
                return [venvPython];
            }

            // On Windows, "python3" commonly resolves to a Microsoft Store app-execution-alias stub
            // (WindowsApps\python3.exe) even when a real interpreter is installed as "python.exe" - so
            // "python" is tried first there. Elsewhere "python3" is the more reliable name.
            return OperatingSystem.IsWindows() ? ["python", "python3"] : ["python3", "python"];
        }
    }
}
