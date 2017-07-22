﻿using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Threading;

namespace FakeHttp
{
    /// <summary>
    /// A <see cref="HttpMessageHandler"/> that retrieves http response messages from
    /// an alternate storage rather than from a given http endpoint
    /// </summary>
    public sealed class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly IReadOnlyResponseStore _store;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="resources">An object that can access stored response</param>
        /// <exception cref="ArgumentNullException"/>
        public FakeHttpMessageHandler(IReadOnlyResources resources)
            : this(new ReadOnlyResponseStore(resources))
        {

        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="store">The storage mechanism for responses</param>
        /// <exception cref="ArgumentNullException"/>
        public FakeHttpMessageHandler(IReadOnlyResponseStore store)
        {
            _store = store ?? throw new ArgumentNullException("store");
        }

        /// <summary>
        /// Override the base class to skip http and retrieve message from storage
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>The stored response message</returns>
        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return await Task.FromResult(_store.FindResponse(request));
        }
    }
}
