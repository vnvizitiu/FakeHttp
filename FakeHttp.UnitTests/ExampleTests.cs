﻿using System;
using System.Threading.Tasks;
using System.Net.Http;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using UnitTestHelpers;

using FakeHttp.Resources;

namespace FakeHttp.UnitTests
{
    [TestClass]
    [DeploymentItem(@"FakeResponses\")]
    public class ExampleTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public async Task MinimalExampleTest()
        {
            var handler = new FakeHttpMessageHandler(new FileSystemResources(TestContext.DeploymentDirectory));
            using (var client = new HttpClient(handler, true))
            {
                client.BaseAddress = new Uri("https://www.example.com/");
                var response = await client.GetAsync("HelloWorldService");
                response.EnsureSuccessStatusCode();

                dynamic content = await response.Content.Deserialize<dynamic>();

                Assert.IsNotNull(content);
                Assert.AreEqual("Hello World", content.Message);
            }
        }

        [TestMethod]
        [TestCategory("fake")]
        public async Task CanGetSimpleJsonResult()
        {
            var handler = new FakeHttpMessageHandler(new FileSystemResources(TestContext.DeploymentDirectory));

            using (var client = new HttpClient(handler, true))
            {
                client.BaseAddress = new Uri("http://openstates.org/api/v1/");
                string key = CredentialStore.RetrieveObject("sunlight.key.json").Key;
                client.DefaultRequestHeaders.Add("X-APIKEY", key);

                var response = await client.GetAsync("metadata/mn");
                response.EnsureSuccessStatusCode();

                dynamic result = await response.Content.Deserialize<dynamic>();

                Assert.IsNotNull(result);
                Assert.AreEqual("Minnesota", result.name);
            }
        }
    }
}
