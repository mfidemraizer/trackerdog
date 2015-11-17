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

        public class C
        {
            public virtual string Text { get; set; }
        }

        [ClassInitialize]
        public static void Init(TestContext context)
        {
            TrackerDogConfiguration.TrackTheseTypes(Track.ThisType<A>(), Track.ThisType<B>(), Track.ThisType<C>());
        }

        [TestMethod]
        public void CanExtractProxyTargetWithChanges()
        {
            C c = new C { Text = "hello world" }.AsTrackable();
            c.Text = "hello world 2";

            C noProxy = c.ToUntracked();

            Assert.AreEqual("hello world 2", noProxy.Text);
        }


        [TestMethod]
        public void AllProxiesAreOfSameProxyType()
        {

            A a1 = new A().AsTrackable();
            A a2 = new A().AsTrackable();

            Assert.AreEqual(a1.GetType(), a2.GetType());
        }

        [TestMethod]
        public void CanCreateProxyWithConstructorArguments()
        {
            B b = Trackable.Of<B>(1, 2);

            Assert.IsTrue(b.IsTrackable());

            b.Text = "hello world";
        }
    }
}
