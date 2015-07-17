﻿using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Diagnostics;

using Newtonsoft.Json;

namespace FakeHttp
{
    /// <summary>
    /// Base calss for file based response message retreival that allows 
    /// separation between desktop and universal app machanisms for file api
    /// </summary>
    public abstract class ResponseLoader
    {
        private readonly MessageFormatter _formatter;

        /// <summary>
        /// ctor 
        /// </summary>
        /// <param name="formatter">PLatofrma specific formatter object</param>
        protected ResponseLoader(MessageFormatter formatter)
        {
            if (formatter == null) throw new ArgumentNullException("formatter");

            _formatter = formatter;
        }

        /// <summary>
        /// Checks whether the specified file exists
        /// </summary>
        /// <param name="folder">The folder name</param>
        /// <param name="fileName">The file name</param>
        /// <returns>Flag indicating whether file exists</returns>
        protected abstract Task<bool> Exists(string folder, string fileName);

        /// <summary>
        /// Loads a given file as a string
        /// </summary>
        /// <param name="folder">The folder name</param>
        /// <param name="fileName">The file name</param>
        /// <returns>The file's contents as a string</returns>
        protected abstract Task<string> LoadAsString(string folder, string fileName);

        /// <summary>
        /// Loads a given file as a stream
        /// </summary>
        /// <param name="folder">The folder name</param>
        /// <param name="fileName">The file name</param>
        /// <returns>File's contents as a stream</returns>
        protected abstract Task<Stream> LoadAsStream(string folder, string fileName);

        /// <summary>
        /// Finds the response message keyed to a request message
        /// </summary>
        /// <param name="request">The request message</param>
        /// <returns>The response message or a 404 message if not found</returns>
        public async Task<HttpResponseMessage> FindResponse(HttpRequestMessage request)
        {
            if (request == null) throw new ArgumentNullException("request");

            var query = _formatter.NormalizeQuery(request.RequestUri);
            var folderPath = _formatter.ToFolderPath(request.RequestUri);

            var longName = _formatter.ToFileName(request, query);
            var shortName = _formatter.ToShortFileName(request);

            // first try to find a file keyed to the request method and query
            return await DeserializeResponse(folderPath, longName)
                // next just look for a default response based on just the http method
                ?? await DeserializeResponse(folderPath, shortName)
                // otherwise return 404            
                ?? await Create404(request, folderPath, longName, shortName);
        }

        private static async Task<HttpResponseMessage> Create404(HttpRequestMessage request, string folderPath, string longName, string shortName)
        {
            Debug.WriteLine("Did not find response for verb {0} and uri {1} in folder {2}.", request.Method, request.RequestUri, folderPath);
            Debug.WriteLine("\tTried content and response files with bases names {0} and {1}.\n\tCheck that fake responses are copied to unit test location.", longName, shortName);

            return await Task.Run(() => new HttpResponseMessage(HttpStatusCode.NotFound) { RequestMessage = request });
        }

        private async Task<HttpResponseMessage> DeserializeResponse(string folder, string baseName)
        {
            var fileName = baseName + ".response.json";
            // first look for a completely serialized response
            if (await Exists(folder, fileName))
            {
                var json = await LoadAsString(folder, fileName);
                var info = JsonConvert.DeserializeObject<ResponseInfo>(json);

                if (info == null)
                {
                    return null;
                }

                _formatter.RepsonseCallbacks.Deserialized(info);

                var response = info.CreateResponse();
                if (!string.IsNullOrEmpty(info.ContentFileName))
                {
                    response.Content = await LoadContent(folder, info.ContentFileName);
                }
                return response;
            }

            // no fully serialized response exists just look for a content file
            return await CreateResponseFromContent(folder, baseName);
        }

        private async Task<HttpResponseMessage> CreateResponseFromContent(string folder, string baseName)
        {
            var fileName = baseName + ".content.json"; // only json supported as raw content right now
            if (await Exists(folder, fileName))
            {
                // no serialized response but we have serialized content
                // craft a response and attach content
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = await LoadContent(folder, fileName)
                };
            }

            return null;
        }

        private async Task<HttpContent> LoadContent(string folder, string fileName)
        {
            if (await Exists(folder, fileName))
            {
                var stream = await LoadAsStream(folder, fileName);
                return new StreamContent(stream);
            }

            return null;
        }
    }
}