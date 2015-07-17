﻿using System;
using System.Threading.Tasks;
using System.IO;
using System.Net.Http;
using System.Text;

using Newtonsoft.Json;

using FakeHttp.Desktop;

namespace FakeHttp
{
    /// <summary>
    /// Class that can store and retreive response messages in a win32 runtime environment
    /// </summary>
    public class FileSystemResponseStore : IResponseStore
    {
        private readonly MessageFormatter _formatter;
        private readonly ResponseLoader _responseLoader;

        private readonly string _captureFolder;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="storeFolder">root folder for storage</param>
        public FileSystemResponseStore(string storeFolder)
            : this(storeFolder, storeFolder)
        {
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="storeFolder">root folder for storage</param>
        /// <param name="captureFolder">folder to store captued response messages</param>
        public FileSystemResponseStore(string storeFolder, string captureFolder)
            : this(storeFolder, captureFolder, (key, value) => false)
        {

        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="storeFolder">root folder for storage</param>
        /// <param name="paramFilter">call back used to determine if a given query paramters should be excluded from serialziation</param>
        [Obsolete("Use constructor that takes IResponseCallbacks instead")]

        public FileSystemResponseStore(string storeFolder, Func<string, string, bool> paramFilter)
            : this(storeFolder, storeFolder, paramFilter)
        {
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="storeFolder">root folder for storage</param>
        /// <param name="captureFolder">folder to store captued response messages</param>
        /// <param name="paramFilter">call back used to determine if a given query paramters should be excluded from serialziation</param>
        [Obsolete("Use constructor that takes IResponseCallbacks instead")]
        public FileSystemResponseStore(string storeFolder, string captureFolder, Func<string, string, bool> paramFilter)
            : this(storeFolder, captureFolder, new ResponseCallbacks(paramFilter))
        {
        }

        public FileSystemResponseStore(string storeFolder, string captureFolder, IResponseCallbacks callbacks)
        {
            if (string.IsNullOrEmpty(storeFolder)) throw new ArgumentException("storeFolder cannot be empty", "storeFolder");
            if (string.IsNullOrEmpty(storeFolder)) throw new ArgumentException("captureFolder cannot be empty", "captureFolder");

            _captureFolder = captureFolder;
            _formatter = new DesktopMessagetFormatter(callbacks);
            _responseLoader = new DesktopResponseLoader(storeFolder, _formatter);
        }

        /// <summary>
        /// Retreive response message from storage based on the a request message
        /// </summary>
        /// <param name="request">The request message</param>
        /// <returns>The response messsage</returns>
        public async Task<HttpResponseMessage> FindResponse(HttpRequestMessage request)
        {
            return await _responseLoader.FindResponse(request);
        }

        /// <summary>
        /// Stores a response message for later retreival
        /// </summary>
        /// <param name="response">The response message to store</param>
        /// <returns>Task</returns>
        public async Task StoreResponse(HttpResponseMessage response)
        {
            if (response == null) throw new ArgumentNullException("response");

            var query = _formatter.NormalizeQuery(response.RequestMessage.RequestUri);
            var folderPath = Path.Combine(_captureFolder, _formatter.ToFolderPath(response.RequestMessage.RequestUri));
            var fileName = _formatter.ToFileName(response.RequestMessage, query);

            Directory.CreateDirectory(folderPath);

            // this is the object that is serialized (response, normalized request query and pointer to the content file)
            var info = _formatter.PackageResponse(response);

            // just read the entire content stream and serialize it 
            using (var file = new FileStream(Path.Combine(folderPath, info.ContentFileName), FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                var bytes = await response.Content.ReadAsByteArrayAsync();
                bytes = await _formatter.RepsonseCallbacks.Serializing(response, bytes);

                file.Write(bytes, 0, bytes.Length);
                file.Flush();
            }

            // now serialize the response object and its meta-data
            var json = JsonConvert.SerializeObject(info, Formatting.Indented);
            using (var responseWriter = new StreamWriter(Path.Combine(folderPath, fileName + ".response.json"), false, Encoding.UTF8))
            {
                responseWriter.Write(json);
            }
        }
    }
}