using System;
using System.Collections.Generic;
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
        /// <param name="property">The property to configure</param>
        /// <returns>Current configurable trackable type</returns>
        IConfigurableTrackableType IncludeProperty(PropertyInfo property);

        /// <summary>
        /// Configures multiple properties to be change-tracked for the current tracked type.
        /// </summary>
        /// <param name="properties">One or more properties to track its changes</param>
        /// <returns>Current trackable type configuration</returns>
        IConfigurableTrackableType IncludeProperties(params PropertyInfo[] properties);

        /// <summary>
        /// Configures multiple properties to be change-tracked for the current tracked type.
        /// </summary>
        /// <param name="properties">One or more properties to track its changes</param>
        /// <returns>Current trackable type configuration</returns>
        IConfigurableTrackableType IncludeProperties(IEnumerable<PropertyInfo> properties);
    }
}
