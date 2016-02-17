namespace TrackerDog.Configuration
{
    using System;
    using System.Collections.Immutable;
    using System.Diagnostics.Contracts;
    using System.Reflection;

    /// <summary>
    /// Defines the configuration of a trackable type
    /// </summary>
    [ContractClass(typeof(ITrackableTypeContract))]
    public interface ITrackableType
    {
        /// <summary>
        /// Gets the type being tracked
        /// </summary>
        Type Type { get; }

        /// <summary>
        /// Gets a map of full associated trackable objects.
        /// </summary>
        IImmutableSet<IObjectPropertyInfo> ObjectPaths { get; }

        /// <summary>
        /// Gets which properties should be tracked for the tracked type.
        /// </summary>
        IImmutableSet<PropertyInfo> IncludedProperties { get; }
    }
}