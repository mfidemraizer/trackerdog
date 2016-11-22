using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TrackerDog.Configuration;

namespace TrackerDog.Test
{
    [TestClass]
    public sealed class InterfaceTrackingTest
    {
        public enum WhateverEnum { Some, Other }

        public interface IWhatever
        {
            string Text { get; set; }
            A A { get; set; }
            WhateverEnum Enum { get; set; }
        }

        public class A
        {
            public string Text { get; set; }
        }

        public class WhateverImpl : IWhatever
        {
            public virtual A A { get; set; }

            public virtual WhateverEnum Enum { get; set; }

            public virtual string Text { get; set; }
        }

        private static ITrackableObjectFactory TrackableObjectFactory { get; set; }

        [ClassInitialize]
        public static void Init(TestContext context)
        {
            IObjectChangeTrackingConfiguration config = ObjectChangeTracking.CreateConfiguration();

            config.TrackThisType<IWhatever>();

            TrackableObjectFactory = config.CreateTrackableObjectFactory();
        }

        [TestMethod]
        public void CanTrackInterfaceImplementations()
        {
            WhateverImpl whatever = new WhateverImpl();
            whatever.Text = "hello world";
            whatever.A = new A();

            whatever = TrackableObjectFactory.CreateFrom(whatever);
            whatever.Enum = WhateverEnum.Other;
        }
    }
}
