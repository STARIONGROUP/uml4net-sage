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
    /// One captured log entry.
    /// </summary>
    /// <param name="Level">The entry's level.</param>
    /// <param name="Category">The logger category, i.e. the full name of the logging type.</param>
    /// <param name="Message">The formatted message.</param>
    public sealed record CapturedLogEntry(LogLevel Level, string Category, string Message);

    /// <summary>
    /// Captures <see cref="LogLevel.Warning"/>/<see cref="LogLevel.Error"/> log entries emitted while
    /// <c>uml4net.xmi</c> reads a model, so <see cref="XmiInspector"/> can surface the reader diagnostics its
    /// structured checks don't already cover as inspection findings, instead of them only going to the console.
    /// </summary>
    public sealed class CapturingLoggerProvider : ILoggerProvider
    {
        private readonly ConcurrentQueue<CapturedLogEntry> entries = new();

        /// <summary>
        /// Gets the captured warning/error entries, in the order they were logged.
        /// </summary>
        public IReadOnlyCollection<CapturedLogEntry> Entries => this.entries;

        /// <summary>
        /// Creates a logger that records its entries in this provider.
        /// </summary>
        /// <param name="categoryName">The category of the logger's entries.</param>
        /// <returns>A logger that adds every entry to this provider's captured entries.</returns>
        public ILogger CreateLogger(string categoryName) => new CapturingLogger(categoryName, this.entries);

        /// <summary>
        /// Does nothing: the provider holds no resources, and its captured entries stay readable.
        /// </summary>
        public void Dispose()
        {
        }

        /// <summary>
        /// A logger that adds every warning and error it is given to a shared queue.
        /// </summary>
        /// <param name="category">The category recorded with each entry.</param>
        /// <param name="entries">The queue the entries are added to.</param>
        private sealed class CapturingLogger(string category, ConcurrentQueue<CapturedLogEntry> entries) : ILogger
        {
            /// <summary>
            /// Does not support scopes.
            /// </summary>
            /// <typeparam name="TState">The type of the scope state.</typeparam>
            /// <param name="state">The scope state, which is ignored.</param>
            /// <returns>Always <c>null</c>.</returns>
            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

            /// <summary>
            /// Gets whether entries of <paramref name="logLevel"/> are captured.
            /// </summary>
            /// <param name="logLevel">The level to check.</param>
            /// <returns><c>true</c> for <see cref="LogLevel.Warning"/> and above.</returns>
            public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Warning;

            /// <summary>
            /// Adds the formatted entry to the queue when its level is enabled.
            /// </summary>
            /// <typeparam name="TState">The type of the entry's state.</typeparam>
            /// <param name="logLevel">The entry's level.</param>
            /// <param name="eventId">The entry's event id, which is not recorded.</param>
            /// <param name="state">The entry's state.</param>
            /// <param name="exception">The exception related to the entry, if any.</param>
            /// <param name="formatter">Creates the message from <paramref name="state"/> and <paramref name="exception"/>.</param>
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                if (!this.IsEnabled(logLevel))
                {
                    return;
                }

                entries.Enqueue(new CapturedLogEntry(logLevel, category, formatter(state, exception)));
            }
        }
    }
}
