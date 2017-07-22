﻿using System;
using System.Threading.Tasks;
using System.Net.Http;

using Newtonsoft.Json;

namespace FakeHttp
{
    /// <summary>
    /// Class that can store and retrieve response messages using an <see cref="IResources"/> instance
    /// </summary>
    public sealed class ResponseStore : ReadOnlyResponseStore, IResponseStore
    {
        private readonly IResources _resources;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="resources">An instance that can read and write to the underlying storage of response resources</param>
        /// <exception cref="ArgumentNullException"/>
        public ResponseStore(IResources resources)
            : this(resources, new ResponseCallbacks())
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resources">An instance that can read and write to the underlying storage of response resources</param>
        /// <param name="callbacks">Object to manage and modify responses at runtime</param>
        /// <exception cref="ArgumentNullException"/>
        public ResponseStore(IResources resources, IResponseCallbacks callbacks)
            : base(resources, callbacks)
        {
            _resources = resources;
        }

        /// <summary>
        /// Stores a response message for later retrieval
        /// </summary>
        /// <param name="response">The response message to store</param>
        /// <returns>Task</returns>
        /// <exception cref="ArgumentNullException"/>
        public async Task StoreResponse(HttpResponseMessage response)
        {
            if (response == null) throw new ArgumentNullException("response");

            var resourcePath = _formatter.ToResourcePath(response.RequestMessage.RequestUri);

            // this is the object that is serialized (response, normalized request query and pointer to the content file)
            var info = _formatter.PackageResponse(response);

            // get the content stream loaded and serialize it
            await response.Content.LoadIntoBufferAsync();
            var content = await _formatter.Callbacks.Serializing(response);
            _resources.Store(resourcePath, info.ContentFileName, content);

            // now serialize the response object and its meta-data
            var fileName = _formatter.ToName(response.RequestMessage);
            var json = JsonConvert.SerializeObject(info, Formatting.Indented, new VersionConverter());
            _resources.Store(resourcePath, fileName + ".response.json", json);
        }
    }
}
