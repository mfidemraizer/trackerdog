namespace TrackerDog.Test
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.ComponentModel;
    using System.Dynamic;
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

        public class C
        {
            public string Text { get; set; }
        }

        [TestMethod]
        public void CanExtractProxyTargetWithChanges()
        {
            TrackerDogConfiguration.TrackTheseTypes(Track.ThisType<A>());

            C c = new C { Text = "hello world" }.AsTrackable();
            c.Text = "hello world 2";

            C noProxy = c.ToNonTracked();

            Assert.AreEqual("hello world 2", noProxy.Text);
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
