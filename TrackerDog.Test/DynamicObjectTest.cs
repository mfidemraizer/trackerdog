namespace TrackerDog.Test
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Configuration;

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
        }

        [TestMethod]
        public void CanTrackDynamicObjectPropertyChanges()
        {
            TrackerDogConfiguration.TrackTheseTypes
            (
                Track.ThisType<TestDynamicObject>()
            );


            dynamic dynamicObject = new TestDynamicObject().AsTrackable();
            string x = dynamicObject.Text;
            ((object)dynamicObject).GetDynamicMember("Text");
            dynamicObject.Text = "hello world 1";
            dynamicObject.Text = "hello world 2";
        }
    }
}
