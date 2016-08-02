using Microsoft.VisualStudio.TestTools.UnitTesting;
using TrackerDog.Configuration;

namespace TrackerDog.Test
{
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

        private static ITrackableObjectFactory TrackableObjectFactory { get; set; }

        [ClassInitialize]
        public static void Init(TestContext context)
        {
            IObjectChangeTrackingConfiguration config = ObjectChangeTracking.CreateConfiguration()
                .TrackThisType<A>()
                .TrackThisType<B>()
                .TrackThisType<C>();

            TrackableObjectFactory = config.CreateTrackableObjectFactory();
        }

        [TestMethod]
        public void CanExtractProxyTargetWithChanges()
        {
            C c = TrackableObjectFactory.CreateFrom(new C { Text = "hello world" });
            c.Text = "hello world 2";

            // TODO:
            C noProxy = c.ToUntracked(null);

            Assert.AreEqual("hello world 2", noProxy.Text);
        }


        [TestMethod]
        public void AllProxiesAreOfSameProxyType()
        {

            A a1 = TrackableObjectFactory.CreateFrom(new A());
            A a2 = TrackableObjectFactory.CreateFrom(new A());

            Assert.AreEqual(a1.GetType(), a2.GetType());
        }

        [TestMethod]
        public void CanCreateProxyWithConstructorArguments()
        {
            B b = TrackableObjectFactory.CreateOf<B>(1, 2);

            Assert.IsTrue(b.IsTrackable());

            b.Text = "hello world";
        }
    }
}