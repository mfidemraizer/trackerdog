using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;

namespace TrackerDog.Configuration
{
    /// <summary>
    /// Represents the object change tracking configuration.
    /// </summary>
    internal sealed class ObjectChangeTrackingConfiguration : IObjectChangeTrackingConfiguration
    {
        private readonly static object _syncLock = new object();

        public ObjectChangeTrackingConfiguration(ICollectionChangeTrackingConfiguration collectionConfiguration)
        {
            Collections = collectionConfiguration;
        }

        public ICollectionChangeTrackingConfiguration Collections { get; }

        /// <summary>
        /// Gets current white list of types to which its instances will support change tracking.
        /// </summary>
        internal HashSet<ITrackableType> TrackableTypes { get; } = new HashSet<ITrackableType>(new ITrackableTypeEqualityComparer());

        /// <summary>
        /// Gets current white list of types to which its instances will support change tracking.
        /// </summary>
        internal HashSet<ITrackableType> TrackableInterfaceTypes { get; } = new HashSet<ITrackableType>(new ITrackableTypeEqualityComparer());

        IImmutableSet<ITrackableType> IObjectChangeTrackingConfiguration.TrackableTypes => TrackableTypes.ToImmutableHashSet();

        IImmutableList<ITrackableType> IObjectChangeTrackingConfiguration.TrackableInterfaceTypes => TrackableInterfaceTypes.ToImmutableList();

        /// <summary>
        /// Configures which types will support change tracking on current <see cref="AppDomain"/>.
        /// </summary>
        /// <param name="types">The types to track its changes</param>
        public void TrackTheseTypes(params ITrackableType[] types)
        {
            Contract.Requires(types != null && types.Length > 0 && types.All(t => t != null), "Given types cannot be null");

            lock (_syncLock)
            {
                foreach (ITrackableType type in types)
                    if (!type.Type.IsInterface)
                        Contract.Assert(TrackableTypes.Add(type), "Type can only be configured to be tracked once");
                    else
                    {
                        IConfigurableTrackableType trackableType = type as IConfigurableTrackableType;
                        trackableType.IncludeProperties(type.Type.GetProperties());

                        Contract.Assert(TrackableInterfaceTypes.Add(type), "Interface type can only be configured to be tracked once");
                    }
            }
        }

        /// <summary>
        /// Gets a configured trackable type by type, or returns null if it's not already configured.
        /// </summary>
        /// <param name="type">The whole type to get its tracking configuration</param>
        /// <returns>The configured trackable type by type, or returns null if it's not already configured</returns>
        public ITrackableType GetTrackableType(Type type)
        {
            Contract.Requires(type != null, "Given type cannot be null");

            lock (_syncLock)
                return TrackableTypes.SingleOrDefault(t => t.Type == type);
        }

        public IEnumerable<ITrackableType> GetAllTrackableBaseTypes(ITrackableType trackableType)
        {
            Contract.Requires(trackableType != null, "Given trackable type must be a non-null reference");
            Contract.Ensures(Contract.Result<IEnumerable<ITrackableType>>() != null);

            lock (_syncLock)
                return trackableType.Type.GetAllBaseTypes()
                                        .Where(t => CanTrackType(t))
                                        .Select(t => GetTrackableType(t));
        }

        /// <summary>
        /// Determines if a given type is configured to be change-tracked.
        /// </summary>
        /// <param name="someType">The whole type to check</param>
        /// <returns><literal>true</literal> if it can be tracked, <literal>false</literal> if it can't be tracked</returns>
        public bool CanTrackType(Type someType)
        {
            Contract.Requires(someType != null, "Given type cannot be null");

            lock (_syncLock)
                return TrackableTypes.Any(t => t.Type == someType.GetActualTypeIfTrackable());
        }

        public bool ImplementsBaseType(Type someType, out ITrackableType baseType)
        {
            baseType = TrackableInterfaceTypes.SingleOrDefault(t => t.Type.IsAssignableFrom(someType.GetActualTypeIfTrackable()));

            return baseType != null;
        }

        /// <summary>
        /// Determines if a given property holds an object type configured as a trackable type
        /// </summary>
        /// <param name="property">The whole property to check</param>
        /// <returns><literal>true</literal> if helds an object type configured as a trackable type, <literal>false</literal> if not </returns>
        public bool CanTrackProperty(PropertyInfo property)
        {
            Contract.Requires(property != null, "Property to check cannot be null");
            Contract.Requires(CanTrackType(property.ReflectedType), "Declaring type must be configured as trackable");

            lock (_syncLock)
            {
                Contract.Assert(CanTrackType(property.GetBaseProperty().DeclaringType), "Declaring type must be configured as trackable even if it's a base class");

                ITrackableType trackableType = GetTrackableType(property.GetBaseProperty().DeclaringType);

                return trackableType.IncludedProperties.Count == 0
                            || trackableType.IncludedProperties.Contains(property.GetBaseProperty())
                            || trackableType.IncludedProperties.Any(p => p.DeclaringType.IsAssignableFrom(property.DeclaringType) && p.Name == property.Name);
            }
        }

        private void ConfigureWithAttributes(IConfigurableTrackableType trackableType)
        {
            AttributeConfigurationBuilder configBuilder = new AttributeConfigurationBuilder(this);
            configBuilder.ConfigureType(trackableType);
        }

        public IObjectChangeTrackingConfiguration TrackThisType<T>(Action<IConfigurableTrackableType<T>> configure = null)
        {
            TrackableType<T> trackableType = new TrackableType<T>(this);
            ConfigureWithAttributes(trackableType);
            configure?.Invoke(trackableType);

            if (!trackableType.Type.IsInterface)
                TrackableTypes.Add(trackableType);
            else
                TrackableInterfaceTypes.Add(trackableType);

            return this;
        }

        public IObjectChangeTrackingConfiguration TrackThisType(Type type, Action<IConfigurableTrackableType> configure = null)
        {
            TrackableType trackableType = new TrackableType(this, type);
            ConfigureWithAttributes(trackableType);
            configure?.Invoke(trackableType);

            if (!trackableType.Type.IsInterface)
                TrackableTypes.Add(trackableType);
            else
                TrackableInterfaceTypes.Add(trackableType);

            return this;
        }

        public IObjectChangeTrackingConfiguration TrackThisTypeRecursive<TRoot>(Action<IConfigurableTrackableType> configure = null, TypeSearchSettings searchSettings = null)
        {
            return TrackThisTypeRecursive(typeof(TRoot), configure, searchSettings);
        }

        public IObjectChangeTrackingConfiguration TrackThisTypeRecursive(Type rootType, Action<IConfigurableTrackableType> configure = null, TypeSearchSettings searchSettings = null)
        {
            TrackableType trackableRoot = new TrackableType(this, rootType);
            ConfigureWithAttributes(trackableRoot);
            List<TrackableType> trackableTypes = null;

            searchSettings = searchSettings ?? DefaultSearchSettings;

            if (searchSettings.Filter == null)
                searchSettings.Filter = t => t.Assembly == rootType.Assembly;

            if (searchSettings.Mode == TypeSearchMode.AttributeConfigurationOnly)
            {
                Func<Type, bool> initialFilter = searchSettings.Filter;
                searchSettings.Filter = t => initialFilter(t) && t.GetCustomAttribute<ChangeTrackableAttribute>() != null;
            }

            trackableTypes = new List<TrackableType>
            (
                rootType.GetAllPropertyTypesRecursive(p => searchSettings.Filter(p.PropertyType)).Select
                (
                    t =>
                    {
                        TrackableType trackableType = new TrackableType(this, t);
                        ConfigureWithAttributes(trackableType);
                        configure?.Invoke(trackableType);

                        return trackableType;
                    }
                )
            );

            trackableTypes.Insert(0, new TrackableType(this, rootType));

            foreach (ITrackableType trackableType in trackableTypes)
                if (!trackableType.Type.IsInterface)
                    TrackableTypes.Add(trackableType);
                else
                    TrackableInterfaceTypes.Add(trackableType);

            return this;
        }

        public ITrackableObjectFactory CreateTrackableObjectFactory() => new TrackableObjectFactoryInternal(this);

        private static TypeSearchSettings DefaultSearchSettings { get; } = new TypeSearchSettings();

        public IObjectChangeTrackingConfiguration TrackTypesFromAssembly(string assemblyName, Action<IConfigurableTrackableType> configure = null, TypeSearchSettings searchSettings = null)
        {
            return TrackTypesFromAssembly(Assembly.Load(assemblyName), configure, searchSettings);
        }

        public IObjectChangeTrackingConfiguration TrackTypesFromAssembly(Assembly assembly, Action<IConfigurableTrackableType> configure = null, TypeSearchSettings searchSettings = null)
        {
            searchSettings = searchSettings ?? DefaultSearchSettings;
            
            if (searchSettings.Mode == TypeSearchMode.AttributeConfigurationOnly)
            {
                if (searchSettings.Filter != null)
                {
                    Func<Type, bool> initialFilter = searchSettings.Filter;
                    searchSettings.Filter = t => initialFilter(t) && t.GetCustomAttribute<ChangeTrackableAttribute>() != null;
                }
                else
                    searchSettings.Filter = t => t.GetCustomAttribute<ChangeTrackableAttribute>() != null;
            }

            foreach (Type type in assembly.GetTypes())
                if (searchSettings.Filter == null || searchSettings.Filter(type))
                    if (!searchSettings.Recursive)
                        TrackThisType(type, configure);
                    else
                        TrackThisTypeRecursive(type, configure, searchSettings);

            return this;
        }
    }
}