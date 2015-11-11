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
        public static TrackableCollectionConfiguration CollectionConfiguration { get; } = new TrackableCollectionConfiguration();

        /// <summary>
        /// Configures which types will support change tracking on current <see cref="AppDomain"/>.
        /// </summary>
        /// <param name="types">The types to track its changes</param>
        public static void TrackTheseTypes(params ITrackableType[] types)
        {
            Contract.Requires(types != null && types.Length > 0 && types.All(t => t != null));

            foreach (ITrackableType type in types)
                TrackableTypes.Add(type);
        }

        /// <summary>
        /// Determines if a given type is configured to be change-tracked.
        /// </summary>
        /// <param name="someType">The whole type to check</param>
        /// <returns><literal>true</literal> if it can be tracked, <literal>false</literal> if it can't be tracked</returns>
        public static bool CanTrackType(Type someType)
        {
            Contract.Requires(someType != null);

            return TrackableTypes.Any(t => someType == t.Type || (someType.IsTrackable() && someType.BaseType == t.Type));
        }

        public static bool CanTrackProperty(PropertyInfo property)
        {
            Contract.Requires(property != null);
            Contract.Requires(CanTrackType(property.DeclaringType));

            return TrackableTypes.Any(t => t.IncludedProperties.Contains(property.GetBaseProperty()));
        }
    }
}