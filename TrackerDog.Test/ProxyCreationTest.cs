using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackerDog.Configuration;

namespace TrackerDog.Test
{
    [TestClass]
    public class ProxyCreationTest
    {
        public class A
        {

        }

        [TestMethod]
        public void AllProxiesAreOfSameProxyType()
        {
            TrackerDogConfiguration.TrackTheseTypes(Track.ThisType<A>());

            A a1 = new A().AsTrackable();
            A a2 = new A().AsTrackable();

            Assert.AreEqual(a1.GetType(), a2.GetType());
        }
    }
}
