using System;
using System.Diagnostics.Contracts;
using System.Reflection;

namespace TrackerDog
{
    /// <summary>
    /// Defines the state of some trackable object property
    /// </summary>
    [ContractClass(typeof(IDeclaredObjectPropertyChangeTrackingContract))]
    public interface IDeclaredObjectPropertyChangeTracking : IObjectPropertyChangeTracking, IEquatable<IDeclaredObjectPropertyChangeTracking>
    {
        /// <summary>
        /// Gets the tracked property
        /// </summary>
        PropertyInfo Property { get; }
    }
}