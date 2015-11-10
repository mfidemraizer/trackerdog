namespace TrackerDog.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// Represents the object change tracking configuration.
    /// </summary>
    public static class TrackerDogConfiguration
    {
        /// <summary>
        /// Gets current white list of types to which its instances will support change tracking.
        /// </summary>
        internal static HashSet<Type> TrackableTypes { get; } = new HashSet<Type>();

        /// <summary>
        /// Gets current trackable collection configuration
        /// </summary>
        public static TrackableCollectionConfiguration CollectionConfiguration { get; } = new TrackableCollectionConfiguration();

        /// <summary>
        /// Configures which types will support change tracking on current <see cref="AppDomain"/>.
        /// </summary>
        /// <param name="types">The types to track its changes</param>
        public static void TrackTheseTypes(params Type[] types)
        {
            Contract.Requires(types != null && types.Length > 0);

            foreach (Type type in types)
                TrackableTypes.Add(type);
        }
    }
}