// SPDX-FileCopyrightText: 2026 Starion Group S.A.
// SPDX-License-Identifier: Apache-2.0

namespace Uml4Net.Sage.Knowledge.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// A minimal, hand-rolled <see cref="HttpMessageHandler"/> stub - no real network calls in unit tests.
    /// </summary>
    public sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly Queue<Func<HttpRequestMessage, HttpResponseMessage>> responses = new();

        public int RequestCount { get; private set; }

        /// <summary>
        /// Queues a response for the next unanswered request.
        /// </summary>
        /// <param name="respond">Builds the response from the request.</param>
        public void Enqueue(Func<HttpRequestMessage, HttpResponseMessage> respond)
        {
            this.responses.Enqueue(respond);
        }

        /// <summary>
        /// Queues a <c>200 OK</c> response with <paramref name="content"/> as its body.
        /// </summary>
        /// <param name="content">The response body.</param>
        public void EnqueueSuccess(byte[] content)
        {
            this.Enqueue(_ => new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(content) });
        }

        /// <summary>
        /// Counts the request and answers it with the next queued response.
        /// </summary>
        /// <param name="request">The request being sent.</param>
        /// <param name="cancellationToken">Ignored.</param>
        /// <returns>The response built by the next queued function.</returns>
        /// <exception cref="InvalidOperationException">No queued responses are left.</exception>
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            this.RequestCount++;

            if (this.responses.Count == 0)
            {
                throw new InvalidOperationException("No more stubbed responses were enqueued.");
            }

            return Task.FromResult(this.responses.Dequeue()(request));
        }
    }
}
