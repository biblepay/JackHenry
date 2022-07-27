using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace JackHenryTwitterFeedTests
{
    public class Tests
    {
        private static JackHenryTwitterService.TwitterInboundService _TwitterInboundService = null;
        
        [SetUp]
        public void Setup()
        {
            _TwitterInboundService = new JackHenryTwitterService.TwitterInboundService();
        }

        [TestCase()]
        public void CanBeInstantiated()
        {
            // Simply checks the component can be instantiated and it has a version > 0:
            try
            {
                int iVersion = JackHenryTwitterService.TwitterInboundService.iVersion;
                Assert.IsTrue(iVersion > 0);
            }
            catch (Exception)
            {
                Assert.Fail();
            }
        }

        [TestCase()]
        public void ServiceLoopRunning()
        { 
            // When the unit tests are launched, we instantiate a new twitter feed service.
            // This test goes into a busy wait loop with one second pauses and waits for the service to start returning results.
            // We break out after we have at least one hashtag processed, and at least one trending hashtag in our dictionary.
            for (int i = 0; i < 60; i++)
            {
                List<JackHenryTwitterService.TwitterExport> lTags = _TwitterInboundService.GetTrendingHashtags(10);
                if (_TwitterInboundService.GetHashtagCount() > 0 && lTags.Count > 0)
                {
                    Assert.Pass();
                }
                System.Threading.Thread.Sleep(1000);
            }
            // After 60 seconds, we fail.
            Assert.Fail();
        }
    }
}