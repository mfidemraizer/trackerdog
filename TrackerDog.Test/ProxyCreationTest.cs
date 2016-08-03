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

        private static IObjectChangeTrackingConfiguration Configuration { get; set; }

        [ClassInitialize]
        public static void Init(TestContext context)
        {
            Configuration = ObjectChangeTracking.CreateConfiguration()
                .TrackThisType<A>()
                .TrackThisType<B>()
                .TrackThisType<C>();

            TrackableObjectFactory = Configuration.CreateTrackableObjectFactory();
        }

        [TestMethod]
        public void CanExtractProxyTargetWithChanges()
        {
            C c = TrackableObjectFactory.CreateFrom(new C { Text = "hello world" });
            c.Text = "hello world 2";
            
            C noProxy = c.ToUntracked();

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