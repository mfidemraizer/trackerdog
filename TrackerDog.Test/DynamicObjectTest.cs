namespace TrackerDog.Test
{
    using Configuration;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Collections.Generic;
    using System.Dynamic;

    [TestClass]
    public sealed class DynamicObjectTest
    {
        public class TestDynamicObject : DynamicObject
        {
            private readonly Dictionary<string, object> _values = new Dictionary<string, object>();

            public override bool TryGetMember(GetMemberBinder binder, out object result)
            {
                result = _values[binder.Name];

                return true;
            }

            public override bool TrySetMember(SetMemberBinder binder, object value)
            {
                _values[binder.Name] = value;
                return true;
            }

            public virtual string DeclaredText { get; set; }
        }

        public class A
        {
            public string Text { get; set; }
        }

        private static ITrackableObjectFactory TrackableObjectFactory { get; set; }

        [ClassInitialize]
        public static void Init(TestContext context)
        {
            IObjectChangeTrackingConfiguration config = ObjectChangeTracking.CreateConfiguration()
                .TrackThisType<A>()
                .TrackThisType<TestDynamicObject>();

            TrackableObjectFactory = config.CreateTrackableObjectFactory();
        }

        [TestMethod]
        public void CanTrackDynamicObjectPropertyChanges()
        {
            dynamic dynamicObject = TrackableObjectFactory.CreateFrom(new TestDynamicObject());

            dynamicObject.Text = "hello world 1";
            dynamicObject.Text = "hello world 2";
            dynamicObject.Text2 = "hello world 3";
            ((TestDynamicObject)dynamicObject).DeclaredText = "hello world 4";
            ((TestDynamicObject)dynamicObject).DeclaredText = "hello world 5";

            IObjectChangeTracker tracker = ((object)dynamicObject).GetChangeTracker();
            IObjectPropertyChangeTracking tracking = tracker.GetDynamicTrackingByProperty("Text");

            Assert.AreEqual(2, tracker.ChangedProperties.Count);
            Assert.AreEqual("Text", tracking.PropertyName);
            Assert.AreEqual("hello world 1", tracking.OldValue);
            Assert.AreEqual("hello world 2", tracking.CurrentValue);
            Assert.AreEqual("hello world 1", ((object)dynamicObject).OldPropertyValue("Text"));
            Assert.AreEqual(null, ((TestDynamicObject)dynamicObject).OldPropertyValue(o => o.DeclaredText));
            Assert.AreEqual("hello world 2", ((object)dynamicObject).CurrentPropertyValue("Text"));
            Assert.AreEqual("hello world 5", ((TestDynamicObject)dynamicObject).CurrentPropertyValue(o => o.DeclaredText));
            Assert.IsTrue(((object)dynamicObject).PropertyHasChanged("Text"));
            Assert.IsFalse(((object)dynamicObject).PropertyHasChanged("Text2"));
        }

        [TestMethod]
        public void CanUntrackDynamicProperties()
        {
            const string text1 = "hello";
            const string text2 = "world";

            dynamic a = TrackableObjectFactory.CreateFrom
            (
                new TestDynamicObject
                {
                    DeclaredText = text1
                }
            );

            a.DynamicText = text2;
            a.A = new A();

            // TODO:
            TestDynamicObject untrackedA = ((TestDynamicObject)a).ToUntracked(null);

            Assert.AreEqual(text1, untrackedA.DeclaredText);
            Assert.AreEqual(text2, ((dynamic)untrackedA).DynamicText);
            Assert.IsNotNull(((dynamic)untrackedA).A);
            Assert.IsFalse(untrackedA.IsTrackable());
            Assert.IsNotNull(((TestDynamicObject)((dynamic)untrackedA)).IsTrackable());
        }
    }
}