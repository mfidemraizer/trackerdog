namespace TrackerDog
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics.Contracts;
    using System.Linq.Expressions;

    /// <summary>
    /// Defines an object change tracker.
    /// </summary>
    [ContractClass(typeof(IObjectChangeTrackerContract))]
    public interface IObjectChangeTracker : IEnumerable<IObjectPropertyChangeTracking>
    {
        /// <summary>
        /// Gets a set of already changed properties
        /// </summary>
        IImmutableSet<IObjectPropertyChangeTracking> ChangedProperties { get; }

        /// <summary>
        /// Gets a set of unchanged properties
        /// </summary>
        IImmutableSet<IObjectPropertyChangeTracking> UnchangedProperties { get; }

        /// <summary>
        /// Gets an object property tracking by specifying a property selector
        /// </summary>
        /// <typeparam name="T">The type of the object owning the whole property tracking to get</typeparam>
        /// <typeparam name="TProperty">The return type of the property owned by the change-tracked object</typeparam>
        /// <param name="propertySelector">The property selector</param>
        /// <returns>The object property tracking</returns>
        IObjectPropertyChangeTracking GetTrackingByProperty<T, TProperty>(Expression<Func<T, TProperty>> propertySelector);
    }
}