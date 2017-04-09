﻿using System;
using System.Threading.Tasks;
using System.Net.Http;

namespace FakeHttp.Stores
{
    /// <summary>
    /// Class that can store and retrieve response messages in a win32 runtime environment
    /// </summary>
    public sealed class ZipFileResponseStore : ReadonlyResponseStore
    {
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="storeFolder">root folder for storage</param>
        public ZipFileResponseStore(string storeFolder)
            : this(storeFolder, new ResponseCallbacks())
        {
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="storeFolder">root folder for storage</param>
        /// <param name="callbacks">Object to manage responses at runtime</param>
        public ZipFileResponseStore(string storeFolder, IResponseCallbacks callbacks)
            : base(new ZipResources(storeFolder), callbacks)
        {
        }
    }
}
