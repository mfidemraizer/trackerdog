namespace TrackerDog
{
    using System;
    using System.Diagnostics.Contracts;
    using System.Reflection;

    /// <summary>
    /// Defines the state of some trackable object property
    /// </summary>
    [ContractClass(typeof(IDeclaredObjectPropertyChangeTrackingContract))]
    public interface IDeclaredObjectPropertyChangeTracking : IObjectPropertyChangeTracking, IEquatable<IDeclaredObjectPropertyChangeTracking>
    {
        /// <summary>
        /// Gets the parent type owning the association with the tracked property
        /// </summary>
        PropertyInfo OwnerProperty { get; }

        /// <summary>
        /// Gets the tracked property
        /// </summary>
        PropertyInfo Property { get; }
    }
}