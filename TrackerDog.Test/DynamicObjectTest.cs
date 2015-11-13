namespace TrackerDog.Test
{
    using Configuration;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Dynamic;

    [TestClass]
    public sealed class DynamicObjectTest
    {
        public class TestDynamicObject : DynamicObject
        {
            private string value;

            public override bool TryGetMember(GetMemberBinder binder, out object result)
            {
                result = value;

                return true;
            }

            public override bool TrySetMember(SetMemberBinder binder, object value)
            {
                this.value = value?.ToString();
                return true;
            }

            public virtual string DeclaredText { get; set; }
        }

        [TestMethod]
        public void CanTrackDynamicObjectPropertyChanges()
        {
            TrackerDogConfiguration.TrackTheseTypes
            (
                Track.ThisType<TestDynamicObject>()
            );
            
            dynamic dynamicObject = new TestDynamicObject().AsTrackable();

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
    }
}