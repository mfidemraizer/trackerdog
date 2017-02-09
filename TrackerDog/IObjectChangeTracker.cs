using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Reflection;

namespace TrackerDog
{
    /// <summary>
    /// Defines an object change tracker.
    /// </summary>
    public interface IObjectChangeTracker : IEnumerable<IObjectPropertyChangeTracking>
    {
        /// <summary>
        /// Occurs when a property change tracking changes
        /// </summary>
        event EventHandler<ObjectChangeEventArgs> Changed;

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
        IDeclaredObjectPropertyChangeTracking GetTrackingByProperty<T, TProperty>(Expression<Func<T, TProperty>> propertySelector);

        /// <summary>
        /// Gets an object property tracking by specifying a property
        /// </summary>
        /// <param name="property">The property</param>
        /// <returns>The object property tracking</returns>
        IDeclaredObjectPropertyChangeTracking GetTrackingByProperty(PropertyInfo property);

        /// <summary>
        /// Gets a dynamic object property tracking by specifying a property name
        /// </summary>
        /// <param name="propertyName">The dynamic property name</param>
        /// <returns>The object property tracking</returns>
        IObjectPropertyChangeTracking GetDynamicTrackingByProperty(string propertyName);

        IObjectGraphTrackingInfo GetTrackingGraphFromProperty(PropertyInfo property);
    }
}