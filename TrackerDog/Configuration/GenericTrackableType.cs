namespace TrackerDog.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics.Contracts;
    using System.Linq.Expressions;
    using System.Reflection;

    /// <summary>
    /// Represents the default implementation to a fluent trackable type configuration.
    /// </summary>
    /// <typeparam name="T">The tracked object type</typeparam>
    public sealed class TrackableType<T> : TrackableType, ICanConfigureTrackableType<TrackableType<T>>
    {
        private readonly ISet<PropertyInfo> _includedProperties = new HashSet<PropertyInfo>(new PropertyInfoEqualityComparer());
        private readonly Type _type;
        private readonly Lazy<IImmutableSet<IObjectPropertyInfo>> _objectPaths;

        public TrackableType()
            : base(typeof(T))
        {
        }

        /// <summary>
        /// Configures a property to be change-tracked for the current tracked type.
        /// </summary>
        /// <param name="propertySelector">A property selector to select which property to track its changes</param>
        /// <returns>Current trackable type configuration</returns>
        public TrackableType<T> IncludeProperty(Expression<Func<T, object>> propertySelector)
        {
            Contract.Requires(propertySelector != null);
            Contract.Ensures(Contract.Result<TrackableType<T>>() != null);

            PropertyInfo property = propertySelector.ExtractProperty();

            return (TrackableType<T>)IncludeProperty(property);
        }

        /// <summary>
        /// Configures multiple properties to be change-tracked for the current tracked type.
        /// </summary>
        /// <param name="propertySelectors">One or more property selectors to select which properties to track its changes</param>
        /// <returns>Current trackable type configuration</returns>
        public TrackableType<T> IncludeProperties(params Expression<Func<T, object>>[] propertySelectors)
        {
            //Contract.Requires(propertySelectors != null && propertySelectors.Length > 0, "Cannot include no selectors");
            //Contract.Ensures(Contract.Result<TrackableType<T>>() != null);

            foreach (Expression<Func<T, object>> propertySelector in propertySelectors)
                IncludeProperty(propertySelector);

            return this;
        }

        TrackableType<T> ICanConfigureTrackableType<TrackableType<T>>.IncludeProperty(PropertyInfo property)
        {
            return (TrackableType<T>)IncludeProperty(property);
        }

        TrackableType<T> ICanConfigureTrackableType<TrackableType<T>>.IncludeProperties(params PropertyInfo[] properties)
        {
            return (TrackableType<T>)IncludeProperties(properties);
        }
    }
}