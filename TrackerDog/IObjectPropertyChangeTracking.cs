namespace TrackerDog
{
    using System;
    using System.Diagnostics.Contracts;
    using System.Reflection;

    /// <summary>
    /// Defines the state of some trackable object property
    /// </summary>
    [ContractClass(typeof(IObjectPropertyChangeTrackingContract))]
    public interface IObjectPropertyChangeTracking : IEquatable<IObjectPropertyChangeTracking>
    {
        /// <summary>
        /// Gets the associated tracker
        /// </summary>
        IObjectChangeTracker Tracker { get; }

        /// <summary>
        /// Gets the tracked property
        /// </summary>
        PropertyInfo Property { get; }

        /// <summary>
        /// Gets the value that had the whole tracked property when the tracking was started
        /// </summary>
        object OldValue { get; }

        /// <summary>
        /// Gets latest tracked property value
        /// </summary>
        object CurrentValue { get; }

        /// <summary>
        /// Gets a flag to determine if the tracked property has changed since its tracking was started
        /// </summary>
        bool HasChanged { get; }
    }
}