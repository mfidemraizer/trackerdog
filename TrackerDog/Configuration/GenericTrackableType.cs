using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Reflection;

namespace TrackerDog.Configuration
{
    /// <summary>
    /// Represents the default implementation to a fluent trackable type configuration.
    /// </summary>
    /// <typeparam name="T">The tracked object type</typeparam>
    internal sealed class TrackableType<T> : TrackableType, ITrackableType<T>, IConfigurableTrackableType<T>
    {
        private readonly ISet<PropertyInfo> _includedProperties = new HashSet<PropertyInfo>(new PropertyInfoEqualityComparer());

        public TrackableType(IObjectChangeTrackingConfiguration configuration)
            : base(configuration, typeof(T))
        {
        }

        /// <summary>
        /// Configures a property to be change-tracked for the current tracked type.
        /// </summary>
        /// <param name="propertySelector">A property selector to select which property to track its changes</param>
        /// <returns>Current trackable type configuration</returns>
        public ITrackableType<T> IncludeProperty(Expression<Func<T, object>> propertySelector)
        {
            Contract.Requires(propertySelector != null);
            Contract.Ensures(Contract.Result<ITrackableType<T>>() != null);

            PropertyInfo property = propertySelector.ExtractProperty();

            return (ITrackableType<T>)IncludeProperty(property);
        }

        /// <summary>
        /// Configures multiple properties to be change-tracked for the current tracked type.
        /// </summary>
        /// <param name="propertySelectors">One or more property selectors to select which properties to track its changes</param>
        /// <returns>Current trackable type configuration</returns>
        public ITrackableType<T> IncludeProperties(params Expression<Func<T, object>>[] propertySelectors)
        {
            //Contract.Requires(propertySelectors != null && propertySelectors.Length > 0, "Cannot include no selectors");
            //Contract.Ensures(Contract.Result<TrackableType<T>>() != null);

            foreach (Expression<Func<T, object>> propertySelector in propertySelectors)
                IncludeProperty(propertySelector);

            return this;
        }
    }
}