using System;
using System.Reflection;

namespace TrackerDog.Configuration
{
    /// <summary>
    /// Defines how a trackable type can be configured
    /// </summary>
    public interface IConfigurableTrackableType
    {
        /// <summary>
        /// Gets the type to be tracked
        /// </summary>
        Type Type { get; }

        /// <summary>
        /// Configures a given property to be tracked for current type. The property must be declared on the type being configured.
        /// </summary>
        /// <param name="propertySelector">The property to configure</param>
        /// <returns>Current configurable trackable type</returns>
        IConfigurableTrackableType IncludeProperty(PropertyInfo propertySelector);

        /// <summary>
        /// Configures multiple properties to be change-tracked for the current tracked type.
        /// </summary>
        /// <param name="propertySelectors">One or more property selectors to select which properties to track its changes</param>
        /// <returns>Current trackable type configuration</returns>
        IConfigurableTrackableType IncludeProperties(params PropertyInfo[] propertySelectors);
    }
}
