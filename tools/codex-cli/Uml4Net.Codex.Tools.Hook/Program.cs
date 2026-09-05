// -------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Starion Group S.A.">
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

using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Uml4Net.Codex.Tools.Hook;

// This SessionStart hook must never block session startup and never print anything but a valid
// hookSpecificOutput envelope (or nothing) to stdout - any failure anywhere below is swallowed and
// the hook exits 0 silently, deferring to the next session. See CLAUDE.md's "small hook, big CLI" split.
try
{
    using var overallTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(8));
    await RunAsync(overallTimeout.Token);
}
catch
{
    // Never surface a failure to the user - silence is the correct degraded behavior here.
}

return 0;

static async Task RunAsync(CancellationToken cancellationToken)
{
    var stdin = await Console.In.ReadToEndAsync(cancellationToken);
    HookInput? input = null;
    try
    {
        input = JsonSerializer.Deserialize(stdin, HookJsonContext.Default.HookInput);
    }
    catch (JsonException)
    {
        // Malformed/absent stdin - proceed as a generic session start.
    }

    var pluginRoot = ResolvePluginRoot();
    if (pluginRoot is null)
    {
        return;
    }

    var cliVersion = PluginManifest.ReadPinnedCliVersion(pluginRoot);
    if (cliVersion is null)
    {
        return;
    }

    using var httpClient = new HttpClient();
    var provisioner = new CliProvisioner(httpClient);
    var cliPath = await provisioner.EnsureAsync(pluginRoot, cliVersion, cancellationToken);
    if (cliPath is null)
    {
        return;
    }

    var checkResult = await new CheckRunner().RunAsync(cliPath, pluginRoot, cancellationToken);
    var additionalContext = CheckResultSummarizer.Summarize(checkResult);
    if (additionalContext is null)
    {
        return;
    }

    var output = new HookOutput
    {
        HookSpecificOutput = new HookSpecificOutput
        {
            HookEventName = "SessionStart",
            AdditionalContext = additionalContext,
        },
    };

    Console.WriteLine(JsonSerializer.Serialize(output, HookJsonContext.Default.HookOutput));
    _ = input; // input.source could steer behavior in the future (e.g. skip on "compact"); unused today.
}

static string? ResolvePluginRoot()
{
    var fromEnvironment = Environment.GetEnvironmentVariable("CLAUDE_PLUGIN_ROOT");
    if (!string.IsNullOrWhiteSpace(fromEnvironment) && Directory.Exists(fromEnvironment))
    {
        return fromEnvironment;
    }

    // Fallback: this executable lives at <pluginRoot>/hooks/native/<rid>/uml4net-codex-hook[.exe].
    var directory = new DirectoryInfo(AppContext.BaseDirectory);
    return directory.Parent?.Parent?.Parent?.FullName;
}
