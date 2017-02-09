using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TrackerDog.Configuration;

namespace TrackerDog.Test
{
    [TestClass]
    public sealed class ConfigurationTest
    {
        public class NoAttributes { }

        [ChangeTrackable]
        public class TwoPropertiesOneTrackable
        {
            [ChangeTrackable]
            public virtual string Text { get; set; }

            public virtual string Text2 { get; set; }
        }

        [ChangeTrackable]
        public class AllPropertiesExceptingNonTrackable
        {
            public virtual string Text { get; set; }

            [DoNotTrackChanges]
            public virtual string Text2 { get; set; }
        }

        [ChangeTrackable]
        public class AllProperties
        {
            public virtual string Text { get; set; }
            public virtual string Text2 { get; set; }
        }

        [ChangeTrackable]
        public class ClassWithReadOnlyProperties
        {
            public virtual string Text { get; set; } = "hello world";
            public virtual string Text2 { get; } = "hey";

            public IList<string> Texts { get; } = new List<string>();

            public virtual List<int> Numbers => new List<int> { 1, 2, 3 };
        }

        [ChangeTrackable]
        public abstract class AbstractClass
        {
            public AbstractClass() { }
            public AbstractClass(int a) { }

            public virtual string Text { get; set; }
        }

        [ChangeTrackable]
        public class DerivesAbstractClass : AbstractClass
        {
        }

        [ChangeTrackable]
        public class WithMultipleConstructors
        {
            public WithMultipleConstructors() { }
            public WithMultipleConstructors(int a) { }
        }

        [ChangeTrackable]
        public abstract class ClassWithNestedEnum
        {
            public enum Test
            {
                One
            }

            [ChangeTrackable]
            public virtual Test TestEnumProperty { get; set; }
        }

        [ChangeTrackable]
        public class DerivedClassWithNestedEnum : ClassWithNestedEnum
        {

        }

        [TestMethod]
        public void ConfiguresClassWithTwoPropertiesButOneIsTrackable()
        {
            IObjectChangeTrackingConfiguration config = ObjectChangeTracking.CreateConfiguration();
            config.TrackTypesFromAssembly(typeof(ConfigurationTest).GetTypeInfo().Assembly, searchSettings: new TypeSearchSettings
            {
                Mode = TypeSearchMode.AttributeConfigurationOnly,
                Filter = t => t == typeof(TwoPropertiesOneTrackable)
            });

            ITrackableType trackableType = config.TrackableTypes.Single();

            Assert.AreEqual(1, trackableType.IncludedProperties.Count);
            Assert.AreEqual("Text", trackableType.IncludedProperties.Single().Name);
        }

        [TestMethod]
        public void ConfiguresClassWithAllPropertiesExceptingNonTrackable()
        {
            IObjectChangeTrackingConfiguration config = ObjectChangeTracking.CreateConfiguration();
            config.TrackTypesFromAssembly(typeof(ConfigurationTest).GetTypeInfo().Assembly, searchSettings: new TypeSearchSettings
            {
                Mode = TypeSearchMode.AttributeConfigurationOnly,
                Filter = t => t == typeof(TwoPropertiesOneTrackable)
            });

            ITrackableType trackableType = config.TrackableTypes.Single();

            Assert.AreEqual(1, trackableType.IncludedProperties.Count);
            Assert.AreEqual("Text", trackableType.IncludedProperties.Single().Name);
        }

        [TestMethod]
        public void ConfiguresClassWithAllProperties()
        {
            IObjectChangeTrackingConfiguration config = ObjectChangeTracking.CreateConfiguration();
            config.TrackTypesFromAssembly(typeof(ConfigurationTest).GetTypeInfo().Assembly, searchSettings: new TypeSearchSettings
            {
                Mode = TypeSearchMode.AttributeConfigurationOnly,
                Filter = t => t == typeof(AllProperties)
            });

            ITrackableType trackableType = config.TrackableTypes.Single();

            Assert.AreEqual(2, trackableType.IncludedProperties.Count);
            Assert.IsTrue(trackableType.IncludedProperties.Any(p => p.Name == "Text"));
            Assert.IsTrue(trackableType.IncludedProperties.Any(p => p.Name == "Text2"));
        }

        [TestMethod]
        public void ConfiguresWithAttributesOnly()
        {
            IObjectChangeTrackingConfiguration config = ObjectChangeTracking.CreateConfiguration();
            config.TrackTypesFromAssembly(typeof(ConfigurationTest).GetTypeInfo().Assembly, searchSettings: new TypeSearchSettings
            {
                Mode = TypeSearchMode.AttributeConfigurationOnly,
                Filter = t =>
                {
                    return t.DeclaringType == typeof(ConfigurationTest);
                }
            });

            IList<Type> trackables = config.TrackableTypes.Select(t => t.Type).ToList();

            Assert.IsTrue(config.TrackableTypes.Count > 0);
            Assert.IsTrue(trackables.Contains(typeof(AllProperties)));
            Assert.IsTrue(trackables.Contains(typeof(AllPropertiesExceptingNonTrackable)));
            Assert.IsTrue(trackables.Contains(typeof(TwoPropertiesOneTrackable)));
            Assert.IsFalse(trackables.Contains(typeof(NoAttributes)));
        }

        [TestMethod]
        public void CanConfigureClassWithReadOnlyProperties()
        {
            IObjectChangeTrackingConfiguration config = ObjectChangeTracking.CreateConfiguration();
            config.TrackTypesFromAssembly(typeof(ConfigurationTest).GetTypeInfo().Assembly, searchSettings: new TypeSearchSettings
            {
                Mode = TypeSearchMode.AttributeConfigurationOnly,
                Filter = t => t == typeof(ClassWithReadOnlyProperties)
            });

            ITrackableObjectFactory factory = config.CreateTrackableObjectFactory();

            ClassWithReadOnlyProperties instance = factory.CreateFrom(new ClassWithReadOnlyProperties());
            instance.Text = "hey";

            IObjectChangeTracker tracker = instance.GetChangeTracker();

            Assert.AreEqual(1, tracker.ChangedProperties.Count);
            Assert.AreEqual(1, ((ObjectChangeTracker)tracker).PropertyTrackings.Count);
        }

        [TestMethod]
        public void CanConfigureAbstractClass()
        {
            IObjectChangeTrackingConfiguration config = ObjectChangeTracking.CreateConfiguration();
            config.TrackTypesFromAssembly(typeof(ConfigurationTest).GetTypeInfo().Assembly, searchSettings: new TypeSearchSettings
            {
                Mode = TypeSearchMode.AttributeConfigurationOnly,
                Filter = t => t == typeof(AbstractClass) || t == typeof(DerivesAbstractClass)
            });

            ITrackableObjectFactory factory = config.CreateTrackableObjectFactory();
            factory.CreateFrom(new DerivesAbstractClass());
        }

        [TestMethod]
        public void CanConfigureClassWithMultipleConstructors()
        {
            IObjectChangeTrackingConfiguration config = ObjectChangeTracking.CreateConfiguration();
            config.TrackTypesFromAssembly(typeof(ConfigurationTest).GetTypeInfo().Assembly, searchSettings: new TypeSearchSettings
            {
                Mode = TypeSearchMode.AttributeConfigurationOnly,
                Filter = t => t == typeof(WithMultipleConstructors)
            });

            ITrackableObjectFactory factory = config.CreateTrackableObjectFactory();

            WithMultipleConstructors instance = factory.CreateFrom(new WithMultipleConstructors());

            Assert.IsNotNull(instance);
        }

        [TestMethod]
        public void CanConfigureWithNestedTypes()
        {
            IObjectChangeTrackingConfiguration config = ObjectChangeTracking.CreateConfiguration();
            config.TrackTypesFromAssembly(typeof(ConfigurationTest).GetTypeInfo().Assembly, searchSettings: new TypeSearchSettings
            {
                Mode = TypeSearchMode.AttributeConfigurationOnly,
                Filter = t => t == typeof(ClassWithNestedEnum) || t == typeof(DerivedClassWithNestedEnum)
            });

            ITrackableObjectFactory factory = config.CreateTrackableObjectFactory();

            DerivedClassWithNestedEnum instance = factory.CreateOf<DerivedClassWithNestedEnum>();

            Assert.IsNotNull(instance);
        }
    }
}