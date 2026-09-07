// -------------------------------------------------------------------------------------------------
// <copyright file="CapturingLoggerProvider.cs" company="Starion Group S.A.">
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
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Captures <see cref="LogLevel.Warning"/>/<see cref="LogLevel.Error"/> log entries emitted while
    /// <c>uml4net.xmi</c> reads a model, so <see cref="XmiInspector"/> can surface unresolved references
    /// and unknown-element diagnostics as inspection findings instead of them only going to the console.
    /// </summary>
    public sealed class CapturingLoggerProvider : ILoggerProvider
    {
        private readonly ConcurrentQueue<(LogLevel Level, string Message)> entries = new();

        /// <summary>
        /// Gets the captured warning/error entries, in the order they were logged.
        /// </summary>
        public IReadOnlyCollection<(LogLevel Level, string Message)> Entries => this.entries;

        /// <inheritdoc/>
        public ILogger CreateLogger(string categoryName) => new CapturingLogger(this.entries);

        /// <inheritdoc/>
        public void Dispose()
        {
        }

        private sealed class CapturingLogger(ConcurrentQueue<(LogLevel Level, string Message)> entries) : ILogger
        {
            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

            public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Warning;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                if (!this.IsEnabled(logLevel))
                {
                    return;
                }

                entries.Enqueue((logLevel, formatter(state, exception)));
            }
        }
    }
}
