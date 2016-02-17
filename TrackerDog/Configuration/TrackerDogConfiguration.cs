namespace TrackerDog.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Reflection;
    using TrackerDog;

    /// <summary>
    /// Represents the object change tracking configuration.
    /// </summary>
    public static class TrackerDogConfiguration
    {
        private readonly static object _syncLock = new object();

        /// <summary>
        /// Gets current white list of types to which its instances will support change tracking.
        /// </summary>
        internal static HashSet<ITrackableType> TrackableTypes { get; } = new HashSet<ITrackableType>(new ITrackableTypeEqualityComparer());

        /// <summary>
        /// Gets current trackable collection configuration
        /// </summary>
        public static TrackableCollectionConfiguration Collections { get; } = new TrackableCollectionConfiguration();

        /// <summary>
        /// Configures which types will support change tracking on current <see cref="AppDomain"/>.
        /// </summary>
        /// <param name="types">The types to track its changes</param>
        public static void TrackTheseTypes(params ITrackableType[] types)
        {
            Contract.Requires(types != null && types.Length > 0 && types.All(t => t != null), "Given types cannot be null");

            lock (_syncLock)
            {
                foreach (ITrackableType type in types)
                    Contract.Assert(TrackableTypes.Add(type), "Type can only be configured to be tracked once");
            }
        }

        /// <summary>
        /// Gets a configured trackable type by type, or returns null if it's not already configured.
        /// </summary>
        /// <param name="type">The whole type to get its tracking configuration</param>
        /// <returns>The configured trackable type by type, or returns null if it's not already configured</returns>
        internal static ITrackableType GetTrackableType(Type type)
        {
            Contract.Requires(type != null, "Given type cannot be null");

            lock (_syncLock)
                return TrackableTypes.SingleOrDefault(t => t.Type == type);
        }

        internal static IEnumerable<ITrackableType> GetAllTrackableBaseTypes(ITrackableType trackableType)
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
        public static bool CanTrackType(Type someType)
        {
            Contract.Requires(someType != null, "Given type cannot be null");

            lock (_syncLock)
                return TrackableTypes.Any(t => t.Type == someType.GetActualTypeIfTrackable());
        }

        /// <summary>
        /// Determines if a given property holds an object type configured as a trackable type
        /// </summary>
        /// <param name="property">The whole property to check</param>
        /// <returns><literal>true</literal> if helds an object type configured as a trackable type, <literal>false</literal> if not </returns>
        public static bool CanTrackProperty(PropertyInfo property)
        {
            Contract.Requires(property != null, "Property to check cannot be null");
            Contract.Requires(CanTrackType(property.ReflectedType), "Declaring type must be configured as trackable");

            lock (_syncLock)
            {
                Contract.Assert(CanTrackType(property.GetBaseProperty().DeclaringType), "Declaring type must be configured as trackable even if it's a base class");

                ITrackableType trackableType = GetTrackableType(property.GetBaseProperty().DeclaringType);

                return trackableType.IncludedProperties.Count == 0
                            || trackableType.IncludedProperties.Contains(property.GetBaseProperty());
            }
        }
    }
}