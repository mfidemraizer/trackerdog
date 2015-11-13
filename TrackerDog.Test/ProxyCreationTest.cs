namespace TrackerDog.Test
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TrackerDog.Configuration;

    [TestClass]
    public class ProxyCreationTest
    {
        public class A
        {
        }

        public class B
        {
            public B(int a, int b)
            {

            }

            public virtual string Text { get; set; }
        }

        [TestMethod]
        public void AllProxiesAreOfSameProxyType()
        {
            TrackerDogConfiguration.TrackTheseTypes(Track.ThisType<A>());

            A a1 = new A().AsTrackable();
            A a2 = new A().AsTrackable();

            Assert.AreEqual(a1.GetType(), a2.GetType());
        }

        [TestMethod]
        public void CanCreateProxyWithConstructorArguments()
        {
            TrackerDogConfiguration.TrackTheseTypes(Track.ThisType<B>());

            B b = Trackable.Of<B>(1, 2);

            Assert.IsTrue(b.IsTrackable());

            b.Text = "hello world";
        }
    }
}
