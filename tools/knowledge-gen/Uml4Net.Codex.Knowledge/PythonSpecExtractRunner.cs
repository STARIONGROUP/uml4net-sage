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
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Uml4Net.Codex.Knowledge.Toolchain;

    /// <summary>
    /// Invokes the <c>tools/spec-extract</c> Python package as a subprocess. This is the one place C#
    /// and Python meet in this repository: everything else is C# (see the root CLAUDE.md).
    /// </summary>
    public sealed class PythonSpecExtractRunner
    {
        private readonly IProcessRunner processRunner;
        private readonly IUvProvisioner uvProvisioner;

        /// <summary>
        /// Initializes a new instance of the <see cref="PythonSpecExtractRunner"/> class.
        /// </summary>
        /// <param name="processRunner">Runs the Python (or <c>uv</c>) subprocess.</param>
        /// <param name="uvProvisioner">
        /// Provisions <c>uv</c> as a last-resort fallback when no <c>.venv</c> or system Python is
        /// found - see <see cref="ExtractAsync"/>.
        /// </param>
        public PythonSpecExtractRunner(IProcessRunner processRunner, IUvProvisioner uvProvisioner)
        {
            this.processRunner = processRunner;
            this.uvProvisioner = uvProvisioner;
        }

        /// <summary>
        /// Runs <c>python -m spec_extract extract</c> against <paramref name="pdfPath"/>, writing clause
        /// markdown and indexes into <paramref name="outputDirectory"/>. Never throws: any failure
        /// (no usable Python or <c>uv</c>, the <c>spec_extract</c> package not installed, a non-zero
        /// exit code) results in a <see cref="SpecExtractionOutcome.Skipped"/> outcome with a
        /// human-readable reason.
        /// </summary>
        /// <param name="pdfPath">
        /// The locally fetched specification PDF (e.g. <c>sources/2.5.1/specs/UML-2.5.1.pdf</c>).
        /// </param>
        /// <param name="outputDirectory">
        /// The <c>spec/</c> output directory (e.g. <c>knowledge/2.5.1/spec</c>).
        /// </param>
        /// <param name="specExtractProjectDirectory">
        /// The <c>tools/spec-extract</c> directory. A <c>.venv</c> under it, or a system Python
        /// interpreter, is tried first; if neither is found, <c>uv</c> is provisioned on demand and used
        /// to run <c>spec_extract</c> instead - it resolves a matching Python and installs
        /// <c>spec_extract</c>'s dependencies into a managed venv itself, so an installed plugin needs
        /// neither a pre-existing Python nor a provisioned <c>.venv</c>.
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

            var extractArguments = new[] { "-m", "spec_extract", "extract", "--pdf", pdfPath, "--out", outputDirectory, "--document", "UML", "--version", version };

            foreach (var pythonExecutable in PythonCandidates(specExtractProjectDirectory))
            {
                try
                {
                    var result = await this.processRunner.RunAsync(pythonExecutable, extractArguments, workingDirectory: srcDirectory, cancellationToken);

                    return result.ExitCode == 0
                        ? SpecExtractionOutcome.Ok()
                        : SpecExtractionOutcome.Skipped($"spec_extract exited with code {result.ExitCode}: {result.StandardError}");
                }
                catch (Win32Exception)
                {
                    // This candidate interpreter isn't available - try the next one.
                }
            }

            var uvExecutable = await this.uvProvisioner.EnsureAsync(cancellationToken);
            if (uvExecutable is null)
            {
                return SpecExtractionOutcome.Skipped(
                    "No Python interpreter was found and uv could not be provisioned (offline, or an " +
                    "unsupported platform). Verbatim spec citation is unavailable until one of them is - " +
                    "see tools/spec-extract/README.md.");
            }

            try
            {
                var uvArguments = new[] { "run", "--project", specExtractProjectDirectory, "python" }.Concat(extractArguments).ToArray();
                var uvResult = await this.processRunner.RunAsync(uvExecutable.FullName, uvArguments, workingDirectory: specExtractProjectDirectory, cancellationToken);

                return uvResult.ExitCode == 0
                    ? SpecExtractionOutcome.Ok()
                    : SpecExtractionOutcome.Skipped($"spec_extract (via uv) exited with code {uvResult.ExitCode}: {uvResult.StandardError}");
            }
            catch (Win32Exception)
            {
                return SpecExtractionOutcome.Skipped(
                    $"The provisioned uv executable at {uvExecutable.FullName} could not be started.");
            }
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
