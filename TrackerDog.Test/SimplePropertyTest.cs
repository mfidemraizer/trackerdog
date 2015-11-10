namespace TrackerDog.Test
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Collections.Generic;
    using System.Linq;
    using TrackerDog.Configuration;

    [TestClass]
    public class SimplePropertyTest
    {
        [ClassInitialize]
        public static void Init(TestContext context)
        {
            TrackerDogConfiguration.TrackTheseTypes
            (
                typeof(A), typeof(B), typeof(C), typeof(D)
            );
        }

        public class A
        {
            public virtual string Text { get; set; }
            public virtual B B { get; set; }
        }

        public class B
        {
            public virtual string Text { get; set; }
            public virtual C C { get; set; }
        }

        public class C
        {
            public virtual string Text { get; set; }
            public virtual IList<D> ListOfD { get; set; }
        }

        public class D
        {
            public string Text { get; set; }
        }

        [TestMethod]
        public void CanTrackSimplePropertyChanges()
        {
            const string initialValue = "hello world";
            const string changedValue = "hello world 2";

            A a = new A { Text = initialValue }.AsTrackable();
            a.Text = changedValue;

            Assert.AreNotEqual(initialValue, a.Text);
            Assert.AreEqual(changedValue, a.Text);

            IObjectChangeTracker changeTracker = a.GetChangeTracker();

            Assert.AreEqual(1, changeTracker.ChangedProperties.Count);
            Assert.AreEqual(1, changeTracker.UnchangedProperties.Count);

            IObjectPropertyChangeTracking tracking = changeTracker.ChangedProperties.Single();

            Assert.AreEqual("Text", tracking.Property.Name);
            Assert.AreEqual(initialValue, tracking.OldValue);
            Assert.AreEqual(changedValue, tracking.CurrentValue);
        }

        [TestMethod]
        public void CanUndoSimplePropertyChanges()
        {
            const string initialValue = "hello world";
            const string changedValue = "hello world 2";


            A a = new A { Text = initialValue }.AsTrackable();
            a.Text = changedValue;

            IObjectChangeTracker changeTracker = a.GetChangeTracker();
            a.UndoChanges();

            Assert.AreEqual(0, changeTracker.ChangedProperties.Count);
            Assert.AreEqual(2, changeTracker.UnchangedProperties.Count);
            Assert.AreEqual(initialValue, a.Text);
        }

        [TestMethod]
        public void CanAcceptObjectGraphChanges()
        {
            const string initialValue = "hello world";
            const string changedValue = "hello world 2";

            A a = new A
            {
                Text = initialValue,
                B = new B
                {
                    Text = initialValue,
                    C = new C
                    {
                        Text = initialValue,
                        ListOfD = new List<D> { new D { Text = "initialValue" } }
                    }
                }
            }.AsTrackable();

            a.Text = changedValue;
            a.B.Text = changedValue;
            a.B.C.Text = changedValue;
            a.B.C.ListOfD.Add(new D { Text = changedValue });

            IObjectChangeTracker changeTracker = a.GetChangeTracker();
            a.AcceptChanges();

            Assert.AreEqual(changedValue, a.Text);
            Assert.AreEqual(changedValue, a.B.Text);
            Assert.AreEqual(changedValue, a.B.C.Text);
            Assert.AreEqual(2, a.B.C.ListOfD.Count);
        }

        [TestMethod]
        public void CanUndoObjectGraphChanges()
        {
            const string initialValue = "hello world";
            const string changedValue = "hello world 2";

            A a = new A
            {
                Text = initialValue,
                B = new B
                {
                    Text = initialValue,
                    C = new C
                    {
                        Text = initialValue,
                        ListOfD = new List<D> { new D { Text = "initialValue" } }
                    }
                }
            }.AsTrackable();

            a.Text = changedValue;
            a.B.Text = changedValue;
            a.B.C.Text = changedValue;
            a.B.C.ListOfD.Add(new D { Text = changedValue });

            IObjectChangeTracker changeTracker = a.GetChangeTracker();
            a.UndoChanges();

            Assert.AreEqual(initialValue, a.Text);
            Assert.AreEqual(initialValue, a.B.Text);
            Assert.AreEqual(initialValue, a.B.C.Text);
            Assert.AreEqual(1, a.B.C.ListOfD.Count);
        }

        [TestMethod]
        public void CanUndoAcceptSimplePropertyChangesAndContinueTrackingChanges()
        {
            const string initialValue = "hello world";
            const string changedValue = "hello world 2";

            A a = new A { Text = initialValue }.AsTrackable();
            a.Text = changedValue;

            IObjectChangeTracker changeTracker = a.GetChangeTracker();
            a.UndoChanges();

            a.Text = changedValue;

            Assert.AreEqual(1, changeTracker.ChangedProperties.Count);

            a.AcceptChanges();

            a.Text = initialValue;

            Assert.AreEqual(1, changeTracker.ChangedProperties.Count);
        }

        [TestMethod]
        public void CanAcceptSimplePropertyChanges()
        {
            const string initialValue = "hello world";
            const string changedValue = "hello world 2";

            A a = new A { Text = initialValue }.AsTrackable();
            a.Text = changedValue;

            IObjectChangeTracker changeTracker = a.GetChangeTracker();
            a.AcceptChanges();

            Assert.AreEqual(0, changeTracker.ChangedProperties.Count);
            Assert.AreEqual(2, changeTracker.UnchangedProperties.Count);
            Assert.AreEqual(changedValue, a.Text);
        }

        [TestMethod]
        public void CanTrackChangesOfAssociations()
        {
            const string initialAText = "hello";
            const string initialBText = "world";
            const string initialCText = "!";
            const string initialDText = "boh!";

            A a = new A
            {
                Text = initialAText,
                B = new B
                {
                    Text = initialBText,
                    C = new C
                    {
                        Text = initialCText,
                        ListOfD = new List<D>
                        {
                            new D { Text = initialDText }
                        }
                    }
                }
            }.AsTrackable();

            a.B.C.Text = "changed 1!";
            a.B.Text = "changed 2!";
            a.Text = "changed 3!";
            a.B.C.ListOfD.First().Text = "changed 4!";
            a.B.C.ListOfD.Add(new D { Text = "changed 5!" });

            IObjectChangeTracker tracker = a.GetChangeTracker();

            Assert.AreEqual(4, tracker.ChangedProperties.Count);
            Assert.AreEqual(2, tracker.UnchangedProperties.Count);
            Assert.AreEqual("changed 1!", a.B.C.Text);
            Assert.AreEqual("changed 2!", a.B.Text);
            Assert.AreEqual("changed 3!", a.Text);
            Assert.AreEqual("changed 4!", a.B.C.ListOfD.First().Text);
            Assert.AreEqual("changed 5!", a.B.C.ListOfD.Last().Text);
        }

        [TestMethod]
        public void CanGetOldAndCurrentValueAndCheckIfHasChanged()
        {
            const string initialValue = "hello world";
            const string changedValue = "hello world 2";

            A a = new A { Text = initialValue }.AsTrackable();
            a.Text = changedValue;

            Assert.AreEqual(initialValue, a.OldPropertyValue(i => i.Text));
            Assert.AreEqual(changedValue, a.CurrentPropertyValue(i => i.Text));
            Assert.IsTrue(a.PropertyHasChanged(i => i.Text));
        }
    }
}