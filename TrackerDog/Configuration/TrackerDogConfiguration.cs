namespace TrackerDog.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Represents the object change tracking configuration.
    /// </summary>
    public static class TrackerDogConfiguration
    {
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

            foreach (ITrackableType type in types)
                Contract.Assert(TrackableTypes.Add(type), "Type can only be configured to be tracked once");
        }

        /// <summary>
        /// Gets a configured trackable type by type, or returns null if it's not already configured.
        /// </summary>
        /// <param name="type">The whole type to get its tracking configuration</param>
        /// <returns>The configured trackable type by type, or returns null if it's not already configured</returns>
        internal static ITrackableType GetTrackableType(Type type)
        {
            Contract.Requires(type != null, "Given type cannot be null");

            return TrackableTypes.SingleOrDefault(t => t.Type == type);
        }

        /// <summary>
        /// Determines if a given type is configured to be change-tracked.
        /// </summary>
        /// <param name="someType">The whole type to check</param>
        /// <returns><literal>true</literal> if it can be tracked, <literal>false</literal> if it can't be tracked</returns>
        public static bool CanTrackType(Type someType)
        {
            Contract.Requires(someType != null, "Given type cannot be null");

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

            ITrackableType trackableType = GetTrackableType(property.GetBaseProperty().DeclaringType);

            return trackableType.IncludedProperties.Count == 0 
                        || trackableType.IncludedProperties.Contains(property.GetBaseProperty());
        }
    }
}