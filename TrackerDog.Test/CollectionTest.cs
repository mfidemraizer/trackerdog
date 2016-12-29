namespace TrackerDog.Test
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using TrackerDog.Configuration;

    [TestClass]
    public class CollectionTest
    {
        private static ITrackableObjectFactory TrackableObjectFactory { get; set; }
        private static IObjectChangeTrackingConfiguration Configuration { get; set; }

        [ClassInitialize]
        public static void Init(TestContext context)
        {
            Configuration = ObjectChangeTracking.CreateConfiguration()
                    .TrackThisType<A>(t => t.IncludeProperty(a => a.Items))
                    .TrackThisType<B>(t => t.IncludeProperty(b => b.Dogs))
                    .TrackThisType<C>(t => t.IncludeProperty(c => c.Dogs))
                    .TrackThisType<Dog>(t => t.IncludeProperty(d => d.Name))
                    .TrackThisType<D>()
                    .TrackThisType<E>(t => t.IncludeProperty(e => e.Dictionary))
                    .TrackThisType<F>(t => t.IncludeProperty(f => f.ListOfF))
                    .TrackThisType<G>(t => t.IncludeProperty(g => g.Buffer))
                    .TrackThisType<WhateverBase>(t => t.IncludeProperty(w => w.List2))
                    .TrackThisType<WhateverParent>(t => t.IncludeProperty(w => w.List));

            TrackableObjectFactory = Configuration.CreateTrackableObjectFactory();
        }

        public class Whatever
        {
        }

        public class WhateverParent : WhateverBase
        {
            public virtual IList<Whatever> List { get; set; } = new List<Whatever>();
        }
        public class WhateverBase
        {
            public virtual IList<string> List2 { get; set; } = new List<string>();
        }

        [DebuggerDisplay("{Name}")]
        public class Dog : IEquatable<Dog>
        {
            public virtual string Name { get; set; }

            public bool Equals(Dog other) =>
                other != null && other.Name == Name;

            public override bool Equals(object obj) =>
                Equals(obj as Dog);

            public override int GetHashCode() => Name.GetHashCode();
        }

        public class A
        {
            public virtual ICollection<string> Items { get; set; } = new List<string>
            {
                "item1", "item2", "item3"
            };
        }

        public class B
        {
            public virtual ICollection<Dog> Dogs { get; set; } = new List<Dog>
            {
                new Dog { Name = "Doggy" },
                new Dog { Name = "Bobby" }
            };
        }

        public class C
        {
            public virtual ISet<Dog> Dogs { get; set; } = new HashSet<Dog>
            {
                new Dog { Name = "Doggy" },
                new Dog { Name = "Bobby" }
            };
        }

        public class D
        {
            public virtual BitArray Mask { get; set; } = new BitArray(2);
        }

        public class E
        {
            public virtual IDictionary<string, string> Dictionary { get; set; } = new Dictionary<string, string>();
        }

        public class F
        {
            public virtual IList<F> ListOfF { get; set; } = new List<F>();
        }

        public class G
        {
            public virtual byte[] Buffer { get; set; }
        }

        [TestMethod]
        public void TrackingTypeWithNonTrackableCollectionWontCrash()
        {
            D d = TrackableObjectFactory.CreateFrom(new D());
            d.Mask = new BitArray(38);

            Assert.IsTrue(d.PropertyHasChanged(o => o.Mask));
        }

        [TestMethod]
        public void CanTrackCollectionPropertiesOfNonTrackableTypes()
        {
            WhateverParent parent = TrackableObjectFactory.CreateFrom(new WhateverParent());
            parent.List2.Add("hey");
            parent.List.Add(new Whatever());

            Assert.AreEqual(2, parent.GetChangeTracker().ChangedProperties.Count);
        }

        [TestMethod]
        public void CollectionItemsArePreservedWhenTurningParentObjectIntoTrackable()
        {
            A a = TrackableObjectFactory.CreateFrom(new A());

            Assert.AreEqual(3, a.Items.Count);

            WhateverParent parent = new WhateverParent();
            parent.List.Add(new Whatever());
            parent.List2.Add("hey");

            parent = TrackableObjectFactory.CreateFrom(parent);

            Assert.AreEqual(1, parent.List.Count);
            Assert.AreEqual(1, parent.List2.Count);
        }

        [TestMethod]
        public void CanTrackListPropertyChanges()
        {
            A a = TrackableObjectFactory.CreateFrom(new A());
            a.Items.Add("hola");

            B b = TrackableObjectFactory.CreateFrom(new B());
            b.Dogs.First().Name = "Rex";
            b.Dogs.Add(new Dog { Name = "Rex" });

            C c = TrackableObjectFactory.CreateFrom(new C());
            c.Dogs.Add(new Dog { Name = "Rex" });

            IObjectChangeTracker Atracking = a.GetChangeTracker();
            IObjectChangeTracker Btracking = b.GetChangeTracker();
            IObjectChangeTracker Ctracking = c.GetChangeTracker();

            Assert.AreEqual(1, Atracking.ChangedProperties.Count);
            Assert.AreEqual(1, Btracking.ChangedProperties.Count);
            Assert.AreEqual(1, Ctracking.ChangedProperties.Count);
        }

        [TestMethod]
        public void CanTurnCollectionItemsToUntracked()
        {
            B b = TrackableObjectFactory.CreateFrom(new B());
            b.Dogs.First().Name = "Rex";
            b.Dogs.Add(new Dog { Name = "Rex" });

            b = b.ToUntracked();

            Assert.IsFalse(b.IsTrackable());
            Assert.IsFalse(b.Dogs.Any(dog => dog.IsTrackable()));
        }

        [TestMethod]
        public void CanTrackSetPropertyChanges()
        {
            C c = TrackableObjectFactory.CreateFrom(new C());

            IObjectChangeTracker Ctracking = c.GetChangeTracker();
            IReadOnlyChangeTrackableCollection trackableCollection = (IReadOnlyChangeTrackableCollection)c.Dogs;

            c.Dogs.Add(new Dog { Name = "Rex" });

            Assert.AreEqual(1, Ctracking.ChangedProperties.Count);
        }

        [TestMethod]
        public void CanTrackSetIntersections()
        {
            C c = TrackableObjectFactory.CreateFrom(new C());
            c.Dogs.Add(new Dog { Name = "Rex" });

            IReadOnlyChangeTrackableCollection trackableCollection = (IReadOnlyChangeTrackableCollection)c.Dogs;

            c.Dogs.IntersectWith(new[] { new Dog { Name = "Rex" } });

            Assert.AreEqual(1, c.Dogs.Count);
            Assert.AreEqual(2, trackableCollection.RemovedItems.Count);
            Assert.AreEqual(1, trackableCollection.AddedItems.Count);
        }

        [TestMethod]
        public void CanTrackSetExcept()
        {
            C c = TrackableObjectFactory.CreateFrom(new C());
            c.Dogs.Add(new Dog { Name = "Rex" });

            IReadOnlyChangeTrackableCollection trackableCollection = (IReadOnlyChangeTrackableCollection)c.Dogs;

            c.Dogs.IntersectWith(new[] { new Dog { Name = "Rex" } });
            Assert.AreEqual(1, c.Dogs.Count);
            Assert.AreEqual(2, trackableCollection.RemovedItems.Count);
            Assert.AreEqual(1, trackableCollection.AddedItems.Count);

            c.Dogs.ExceptWith(new[] { new Dog { Name = "Rex" } });
            Assert.AreEqual(0, c.Dogs.Count);
            Assert.AreEqual(2, trackableCollection.RemovedItems.Count);
            Assert.AreEqual(0, trackableCollection.AddedItems.Count);
        }

        [TestMethod]
        public void CanTrackDictionaryChanges()
        {
            E e = new E();
            e.Dictionary.Add("hello", "world");
            e = TrackableObjectFactory.CreateFrom(e);
            e.Dictionary.Add("bye", "bye");

            Assert.IsTrue(e.PropertyHasChanged(o => o.Dictionary));
        }

        [TestMethod]
        public void CanCallMethodOfDictionary()
        {
            E e = new E();
            e.Dictionary.Add("hello", "world");
            e = TrackableObjectFactory.CreateFrom(e);

            e.Dictionary.ContainsKey("hello");
        }

        [TestMethod]
        public void CanTrackChangesOfCollectionOfEnclosingType()
        {
            F f = TrackableObjectFactory.CreateFrom(new F());

            f.ListOfF.Add(new F());

            Assert.IsTrue(f.PropertyHasChanged(o => o.ListOfF));
            Assert.AreEqual(1, ((IReadOnlyChangeTrackableCollection)f.ListOfF).AddedItems.Count);
        }

        [TestMethod]
        public void ArrayMustNotBeTrackedAsCollections()
        {
            G g = TrackableObjectFactory.CreateFrom(new G());
            g.Buffer = new byte[] { };
            
            Assert.IsTrue(g.PropertyHasChanged(o => o.Buffer));
            Assert.IsNotInstanceOfType(g.Buffer, typeof(IReadOnlyChangeTrackableCollection));
        }

        [TestMethod]
        public void CanClearCollectionChanges()
        {
            A a = TrackableObjectFactory.CreateOf<A>();
            a.Items.Add("a");
            a.Items.Add("b");
            
            a.Items.ClearChanges();

            IReadOnlyChangeTrackableCollection trackableCollection
                = (IReadOnlyChangeTrackableCollection)a.Items;

            Assert.AreEqual(0, trackableCollection.AddedItems.Count);
            Assert.AreEqual(0, trackableCollection.RemovedItems.Count);
        }
    }
}