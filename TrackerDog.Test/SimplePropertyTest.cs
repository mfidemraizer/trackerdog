namespace TrackerDog.Test
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Collections.Generic;
    using System.Linq;
    using TrackerDog.Configuration;

    [TestClass]
    public class SimplePropertyTest
    {
        private static IObjectChangeTrackingConfiguration Configuration { get; set; }
        private static ITrackableObjectFactory TrackableObjectFactory { get; set; }
        [ClassInitialize]
        public static void Init(TestContext context)
        {
            Configuration = ObjectChangeTracking.CreateConfiguration()
                    .TrackThisType<A>(t => t.IncludeProperties(a => a.Text, a => a.B))
                    .TrackThisType<B>(t => t.IncludeProperties(b => b.Text, b => b.C))
                    .TrackThisType<C>(t => t.IncludeProperties(c => c.Text, c => c.ListOfD))
                    .TrackThisType<D>(t => t.IncludeProperty(d => d.Text))
                    .TrackThisType<E>(t => t.IncludeProperties(e => e.Text, e => e.Number))
                    .TrackThisType<Customer>(t => t.IncludeProperty(c => c.ContactInfo))
                    .TrackThisType<Contact>(t => t.IncludeProperty(c => c.Name))
                    .TrackThisType<EnhancedContact>(t => t.IncludeProperty(c => c.Default));

            TrackableObjectFactory = Configuration.CreateTrackableObjectFactory();
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
            public virtual string Text { get; set; }
        }

        public class E
        {
            public string Text { get; set; }
            public int Number { get; set; }
        }

        public class Customer
        {
            public EnhancedContact ContactInfo { get; set; }
        }

        public class Contact
        {
            public string Name { get; set; }
        }

        public class EnhancedContact : Contact
        {
            public bool Default { get; set; }
        }

        [TestMethod]
        public void CanGetTrackableTypeObjectPaths()
        {

            var xxx = Configuration.GetTrackableType(typeof(Customer)).ObjectPaths;
            var yyy = Configuration.GetTrackableType(typeof(A)).ObjectPaths;
        }

        [TestMethod]
        public void CanTrackSimplePropertyChanges()
        {
            const string initialValue = "hello world";
            const string changedValue = "hello world 2";

            A a = TrackableObjectFactory.CreateFrom(new A { Text = initialValue });
            a.Text = changedValue;

            Assert.AreNotEqual(initialValue, a.Text);
            Assert.AreEqual(changedValue, a.Text);

            IObjectChangeTracker changeTracker = a.GetChangeTracker();

            Assert.AreEqual(1, changeTracker.ChangedProperties.Count);
            Assert.AreEqual(1, changeTracker.UnchangedProperties.Count);

            IDeclaredObjectPropertyChangeTracking tracking = changeTracker.ChangedProperties.OfType<IDeclaredObjectPropertyChangeTracking>().Single();

            Assert.AreEqual("Text", tracking.Property.Name);
            Assert.AreEqual(initialValue, tracking.OldValue);
            Assert.AreEqual(changedValue, tracking.CurrentValue);
        }

        [TestMethod]
        public void CanTrackBothReferenceAndValueTypeProperties()
        {
            // It should not throw an exception ;)
            E e = TrackableObjectFactory.CreateFrom(new E());
        }

        [TestMethod]
        public void CanUndoSimplePropertyChanges()
        {
            const string initialValue = "hello world";
            const string changedValue = "hello world 2";


            A a = TrackableObjectFactory.CreateFrom(new A { Text = initialValue });
            a.Text = changedValue;

            IObjectChangeTracker changeTracker = a.GetChangeTracker();
            a.UndoChanges();

            Assert.AreEqual(0, changeTracker.ChangedProperties.Count);
            Assert.AreEqual(2, changeTracker.UnchangedProperties.Count);
            Assert.AreEqual(initialValue, a.Text);
        }

        [TestMethod]
        public void CanGetTrackingObjectGraph()
        {
            A a = TrackableObjectFactory.CreateFrom
            (
                new A
                {
                    Text = "hey",
                    B = new B
                    {
                        Text = "hurray",
                        C = new C
                        {
                            Text = "uhm",
                            ListOfD = new List<D> { new D { Text = "ohm" } }
                        }
                    }
                }
            );

            IObjectChangeTracker changeTracker = a.GetChangeTracker();

            var C_ListOfDGraph = changeTracker.GetTrackingGraphFromProperty(typeof(C).GetProperty("ListOfD"));
            Assert.AreEqual(typeof(C).GetProperty("ListOfD"), C_ListOfDGraph.AggregateHierarchy[2].Property.GetBaseProperty());
            Assert.AreEqual(typeof(B).GetProperty("C"), C_ListOfDGraph.AggregateHierarchy[1].Property.GetBaseProperty());
            Assert.AreEqual(typeof(A).GetProperty("B"), C_ListOfDGraph.AggregateHierarchy[0].Property.GetBaseProperty());
            Assert.AreEqual(a, C_ListOfDGraph.Parent);

            changeTracker.Changed += (s, e) =>
            {
                DeclaredObjectPropertyChangeEventArgs declaredArgs = e as DeclaredObjectPropertyChangeEventArgs;

                IObjectGraphTrackingInfo graphInfo = declaredArgs.GraphTrackingInfo;

                Assert.AreEqual(typeof(B).GetProperty("Text"), graphInfo.AggregateHierarchy[1].Property.GetBaseProperty());
                Assert.AreEqual(typeof(A).GetProperty("B"), graphInfo.AggregateHierarchy[0].Property.GetBaseProperty());
                Assert.AreEqual(a, graphInfo.Parent);
            };

            a.B.Text = "hello world";
        }

        [TestMethod]
        public void CanAcceptObjectGraphChanges()
        {
            const string initialValue = "hello world";
            const string changedValue = "hello world 2";

            A a = TrackableObjectFactory.CreateFrom
            (
                new A
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
                }
            );

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

            A a = TrackableObjectFactory.CreateFrom(
                new A
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
                }
            );

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

            A a = TrackableObjectFactory.CreateFrom(new A { Text = initialValue });
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

            A a = TrackableObjectFactory.CreateFrom(new A { Text = initialValue });
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

            A a = TrackableObjectFactory.CreateFrom
            (
                new A
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
                }
            );

            IObjectChangeTracker tracker = a.GetChangeTracker();
            int changeCount = 0;

            tracker.Changed += (s, e) =>
            {
                changeCount++;
            };

            a.B.C.Text = "changed 1!";
            a.B.Text = "changed 2!";
            a.Text = "changed 3!";
            a.B.C.ListOfD.First().Text = "changed 4!";
            a.B.C.ListOfD.Add(new D { Text = "changed 5!" });

            Assert.AreEqual(5, changeCount);
            Assert.AreEqual(4, tracker.ChangedProperties.Count);
            Assert.AreEqual(2, tracker.UnchangedProperties.Count);
            Assert.AreEqual("changed 1!", a.B.C.Text);
            Assert.AreEqual("changed 2!", a.B.Text);
            Assert.AreEqual("changed 3!", a.Text);
            Assert.AreEqual("changed 4!", a.B.C.ListOfD.First().Text);
            Assert.AreEqual("changed 5!", a.B.C.ListOfD.Last().Text);
        }

        [TestMethod]
        public void CanUntrack()
        {
            const string initialAText = "hello";
            const string initialBText = "world";
            const string initialCText = "!";
            const string initialDText = "boh!";

            A a = TrackableObjectFactory.CreateFrom
            (
                new A
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
                }
            );

            a.B.C.Text = "changed 1!";
            a.B.Text = "changed 2!";
            a.Text = "changed 3!";
            a.B.C.ListOfD.First().Text = "changed 4!";
            a.B.C.ListOfD.Add(new D { Text = "changed 5!" });

            A untrackedA = a.ToUntracked();

            Assert.AreEqual("changed 1!", untrackedA.B.C.Text);
            Assert.AreEqual("changed 2!", untrackedA.B.Text);
            Assert.AreEqual("changed 3!", untrackedA.Text);
            Assert.AreEqual("changed 4!", untrackedA.B.C.ListOfD.First().Text);
            Assert.AreEqual("changed 5!", untrackedA.B.C.ListOfD.Last().Text);
            Assert.IsFalse(untrackedA.IsTrackable());
            Assert.IsFalse(untrackedA.B.IsTrackable());
            Assert.IsFalse(untrackedA.B.C.IsTrackable());
            Assert.IsFalse(untrackedA.B.C.ListOfD.IsTrackable());
            Assert.IsFalse(untrackedA.B.C.ListOfD.Any(i => i.IsTrackable()));
        }

        [TestMethod]
        public void CanGetOldAndCurrentValueAndCheckIfHasChanged()
        {
            const string initialValue = "hello world";
            const string changedValue = "hello world 2";

            A a = TrackableObjectFactory.CreateFrom(new A { Text = initialValue });
            a.Text = changedValue;

            Assert.AreEqual(initialValue, a.OldPropertyValue(i => i.Text));
            Assert.AreEqual(changedValue, a.CurrentPropertyValue(i => i.Text));
            Assert.IsTrue(a.PropertyHasChanged(i => i.Text));
        }
    }
}