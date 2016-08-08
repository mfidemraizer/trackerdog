using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;

namespace TrackerDog.Configuration
{
    /// <summary>
    /// Defines the required set of members to configure object change tracking
    /// </summary>
    public interface IObjectChangeTrackingConfiguration
    {
        /// <summary>
        /// Gets access to collection change tracking specific configuration
        /// </summary>
        ICollectionChangeTrackingConfiguration Collections { get; }
        
        /// <summary>
        /// Gets configured types to be tracked that are interfaces
        /// </summary>
        IImmutableList<ITrackableType> TrackableInterfaceTypes { get; }

        /// <summary>
        /// Gets configured types to be tracked
        /// </summary>
        IImmutableSet<ITrackableType> TrackableTypes { get; }

        /// <summary>
        /// Configures given type as generic parameter to be change-trackable
        /// </summary>
        /// <typeparam name="T">The type to which its instances will be change-trackable</typeparam>
        /// <param name="configure">A configuration action. It receives a trackable type instance to be configured beyond defaults</param>
        /// <returns>Current configuration instance</returns>
        IObjectChangeTrackingConfiguration TrackThisType<T>(Action<IConfigurableTrackableType<T>> configure = null);


        /// <summary>
        /// Configures given type to be change-trackable
        /// </summary>
        /// <param name="type">The type to which its instances will be change-trackable</param>
        /// <param name="configure">A configuration action. It receives a trackable type instance to be configured beyond defaults</param>
        /// <returns>Current configuration instance</returns>
        IObjectChangeTrackingConfiguration TrackThisType(Type type, Action<IConfigurableTrackableType> configure = null);

        /// <summary>
        /// Configures given type given as generic parameter to be change-trackable and recurisvely configures all associated types within 
        /// any nesting level to be also change-trackable.
        /// </summary>
        /// <typeparam name="TRoot">The type of root change-trackable type</typeparam>
        /// <param name="searchSettings">Search settings</param>
        /// <param name="configure">A predicate to customize configuration for each found type</param>
        /// <returns>Current configuration instance</returns>
        IObjectChangeTrackingConfiguration TrackThisTypeRecursive<TRoot>(Action<IConfigurableTrackableType> configure = null, TypeSearchSettings searchSettings = null);

        /// <summary>
        /// Configures given type given as parameter to be change-trackable and recurisvely configures all associated types within 
        /// any nesting level to be also change-trackable.
        /// </summary>
        /// <param name="rootType">The type of root change-trackable type</param>
        /// <param name="configure">A predicate to customize configuration for each found type</param>
        /// <param name="searchSettings">Search settings</param>
        /// <returns>Current configuration instance</returns>
        IObjectChangeTrackingConfiguration TrackThisTypeRecursive(Type rootType, Action<IConfigurableTrackableType> configure = null, TypeSearchSettings searchSettings = null);

        /// <summary>
        /// Configures types from a given assembly to be change-trackable based on provided search settings
        /// </summary>
        /// <param name="assemblyName">The assembly name containing the types to configure</param>
        /// <param name="searchSettings">Search settings</param>
        /// <param name="configure">A predicate to customize configuration for each found type</param>
        /// <returns>Current configuration instance</returns>
        IObjectChangeTrackingConfiguration TrackTypesFromAssembly(string assemblyName, Action<IConfigurableTrackableType> configure = null, TypeSearchSettings searchSettings = null);

        /// <summary>
        /// Configures types from a given assembly to be change-trackable based on provided search settings
        /// </summary>
        /// <param name="assembly">The assembly containing the types to configure</param>
        /// <param name="searchSettings">Search settings</param>
        /// <param name="configure">A predicate to customize configuration for each found type</param>
        /// <returns>Current configuration instance</returns>
        IObjectChangeTrackingConfiguration TrackTypesFromAssembly(Assembly assembly, Action<IConfigurableTrackableType> configure = null, TypeSearchSettings searchSettings = null);

        /// <summary>
        /// Gets a trackable type configuration by giving its type
        /// </summary>
        /// <param name="type">The already configured type to be change-trackable</param>
        /// <returns></returns>
        ITrackableType GetTrackableType(Type type);

        /// <summary>
        /// Gets all base trackable types of a given other trackable type
        /// </summary>
        /// <param name="trackableType">The trackable type to look for its base types</param>
        /// <returns>All found base trackable types</returns>
        IEnumerable<ITrackableType> GetAllTrackableBaseTypes(ITrackableType trackableType);

        /// <summary>
        /// Given an arbitrary type, returns if it is an already configured trackable type
        /// </summary>
        /// <param name="someType">The type to check</param>
        /// <returns><literal>true</literal> if it is trackable, <literal>false</literal> if it is not trackable</returns>
        bool CanTrackType(Type someType);

        /// <summary>
        /// Given an arbitrary type, returns if there is some already configured trackable type that is a base type of itself.
        /// </summary>
        /// <param name="someType">Type to check</param>
        /// <param name="baseType">Found base trackable type</param>
        /// <returns><literal>true</literal> if some base type is found, <literal>false</literal>if no type is found</returns>
        bool ImplementsBaseType(Type someType, out ITrackableType baseType);

        /// <summary>
        /// Given an arbitrary reflected property, determines if it is configured to be trackable in some already configured trackable type.
        /// </summary>
        /// <param name="property">Property to check</param>
        /// <returns><literal>true</literal> if it can be trackable, <literal>false</literal> if it cannot be trackable</returns>
        bool CanTrackProperty(PropertyInfo property);

        /// <summary>
        /// Creates a factory to turn any elegible object into a change-trackable one
        /// </summary>
        /// <returns>A trackable object factory</returns>
        ITrackableObjectFactory CreateTrackableObjectFactory();
    }
}
