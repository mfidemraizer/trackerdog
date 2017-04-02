using System;
using System.Reflection;

namespace TrackerDog
{
    /// <summary>
    /// Defines the state of some trackable object property
    /// </summary>
    public interface IDeclaredObjectPropertyChangeTracking : IObjectPropertyChangeTracking, IEquatable<IDeclaredObjectPropertyChangeTracking>
    {
        /// <summary>
        /// Gets the tracked property
        /// </summary>
        PropertyInfo Property { get; }
    }
}