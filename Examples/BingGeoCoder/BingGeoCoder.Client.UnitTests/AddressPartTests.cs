﻿using System.Threading.Tasks;
using System.Net.Http;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using GalaSoft.MvvmLight.Ioc;

using BingGeoCoder.Client;

using UnitTestHelpers;

namespace GeoCoderTests
{
    [TestClass]
    [DeploymentItem("FakeResponses.zip")]
    public class AddressPartTests
    {
        private static IGeoCoder _service;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            var handler = SimpleIoc.Default.GetInstance<HttpMessageHandler>();

            _service = new GeoCoder(handler, CredentialStore.RetrieveObject("bing.key.json").Key, "Portable-Bing-GeoCoder-UnitTests/1.0");
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            if (_service != null)
            {
                _service.Dispose();
            }
        }

        [TestMethod]
        [TestCategory("geocoder")]
        public async Task GetFormattedAddressFromCoordinate()
        {
            var address = await _service.GetFormattedAddress(44.9108238220215, -93.1702041625977);

            Assert.AreEqual("1012 Davern St, St Paul, MN 55116", address);
        }

        [TestMethod]
        [TestCategory("geocoder")]
        public async Task GetNeighborhoodFromCoordinate()
        {
            var address = await _service.GetAddressPart(44.9108238220215, -93.1702041625977, "Neighborhood");

            Assert.AreEqual("Highland", address);
        }

        [TestMethod]
        [TestCategory("geocoder")]
        public async Task GetAddressFromCoordinate()
        {
            var address = await _service.GetAddressPart(44.9108238220215, -93.1702041625977, "Address");

            Assert.AreEqual("1012 Davern St", address);
        }

        [TestMethod]
        [TestCategory("geocoder")]
        public async Task GetPostalCodeFromCoordinate()
        {
            var postalCode = await _service.GetAddressPart(44.9108238220215, -93.1702041625977, "Postcode1");

            Assert.AreEqual("55116", postalCode);
        }

        [TestMethod]
        [TestCategory("geocoder")]
        public async Task GetCityFromCoordinate()
        {
            var city = await _service.GetAddressPart(44.9108238220215, -93.1702041625977, "PopulatedPlace");

            Assert.AreEqual("St Paul", city);
        }

        [TestMethod]
        [TestCategory("geocoder")]
        public async Task GetCountyFromCoordinate()
        {
            var county = await _service.GetAddressPart(44.9108238220215, -93.1702041625977, "AdminDivision2");

            Assert.AreEqual("Ramsey Co.", county);
        }

        [TestMethod]
        [TestCategory("geocoder")]
        public async Task GetStateFromCoordinate()
        {
            var state = await _service.GetAddressPart(44.9108238220215, -93.1702041625977, "AdminDivision1");

            Assert.AreEqual("MN", state);
        }

        [TestMethod]
        [TestCategory("geocoder")]
        public async Task GetCountryFromCoordinate()
        {
            var country = await _service.GetAddressPart(44.9108238220215, -93.1702041625977, "CountryRegion");

            Assert.AreEqual("United States", country);
        }

        [TestMethod]
        [TestCategory("geocoder")]
        public async Task GetCountryFromCoordinateUsingEnum()
        {
            var country = await _service.GetAddressPart(44.9108238220215, -93.1702041625977, AddressEntityType.CountryRegion);

            Assert.AreEqual("United States", country);
        }
    }
}
