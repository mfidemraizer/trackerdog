using System;
using System.Linq.Expressions;

namespace TrackerDog.Configuration
{
    /// <summary>
    /// Defines how a trackable type can be configured
    /// </summary>
    /// <typeparam name="T">The type to be tracked</typeparam>
    public interface IConfigurableTrackableType<T> : IConfigurableTrackableType
    {
        /// <summary>
        /// Configures a property to be trackable by provoding an expression which selects the whole property
        /// </summary>
        /// <param name="propertySelector">The expression to select the property to track for changes</param>
        /// <returns>Current configurable trackable type</returns>
        IConfigurableTrackableType<T> IncludeProperty(Expression<Func<T, object>> propertySelector);

        /// <summary>
        /// Configures a property to be trackable by provoding an expression which selects the whole property
        /// </summary>
        /// <param name="propertySelectors">One or many property selectors of properties to be tracked for changes</param>
        /// <returns>Current configurable trackable type</returns>
        IConfigurableTrackableType<T> IncludeProperties(params Expression<Func<T, object>>[] propertySelectors);
    }
}