using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
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

        [TestMethod]
        public void ConfiguresClassWithTwoPropertiesButOneIsTrackable()
        {
            IObjectChangeTrackingConfiguration config = ObjectChangeTracking.CreateConfiguration();
            config.TrackTypesFromAssembly(typeof(ConfigurationTest).Assembly, searchSettings: new TypeSearchSettings
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
            config.TrackTypesFromAssembly(typeof(ConfigurationTest).Assembly, searchSettings: new TypeSearchSettings
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
            config.TrackTypesFromAssembly(typeof(ConfigurationTest).Assembly, searchSettings: new TypeSearchSettings
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
            config.TrackTypesFromAssembly(typeof(ConfigurationTest).Assembly, searchSettings: new TypeSearchSettings
            {
                Mode = TypeSearchMode.AttributeConfigurationOnly,
                Filter = t => t.ReflectedType == typeof(ConfigurationTest)
            });

            IList<Type> trackables = config.TrackableTypes.Select(t => t.Type).ToList();

            Assert.AreEqual(3, config.TrackableTypes.Count);
            Assert.IsTrue(trackables.Contains(typeof(AllProperties)));
            Assert.IsTrue(trackables.Contains(typeof(AllPropertiesExceptingNonTrackable)));
            Assert.IsTrue(trackables.Contains(typeof(TwoPropertiesOneTrackable)));
            Assert.IsFalse(trackables.Contains(typeof(NoAttributes)));
        }
    }
}