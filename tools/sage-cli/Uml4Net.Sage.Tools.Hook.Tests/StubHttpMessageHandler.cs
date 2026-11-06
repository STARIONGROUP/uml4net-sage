// -------------------------------------------------------------------------------------------------
// <copyright file="StubHttpMessageHandler.cs" company="Starion Group S.A.">
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

namespace Uml4Net.Sage.Tools.Hook.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// A minimal, hand-rolled <see cref="HttpMessageHandler"/> stub - no real network calls in unit tests.
    /// </summary>
    public sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly Queue<Func<HttpRequestMessage, HttpResponseMessage>> responses = new();

        /// <summary>
        /// Adds a response factory to the queue. Each request uses the next factory in order.
        /// </summary>
        /// <param name="respond">Builds the response for a request, or throws to simulate a failure.</param>
        public void Enqueue(Func<HttpRequestMessage, HttpResponseMessage> respond) => this.responses.Enqueue(respond);

        /// <summary>
        /// Answers <paramref name="request"/> with the next queued response factory, without any network call.
        /// </summary>
        /// <param name="request">The HTTP request to answer.</param>
        /// <param name="cancellationToken">Not used.</param>
        /// <returns>A completed task holding the response built by the dequeued factory.</returns>
        /// <exception cref="InvalidOperationException">No response factory is left in the queue.</exception>
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (this.responses.Count == 0)
            {
                throw new InvalidOperationException("No more stubbed responses were enqueued.");
            }

            return Task.FromResult(this.responses.Dequeue()(request));
        }
    }
}
