namespace TrackerDog.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Represents a factory of trackable type configuration.
    /// </summary>
    public static class Track
    {
        /// <summary>
        /// Creates an instance of <see cref="TrackableType{T}"/> to fluently configure it.
        /// </summary>
        /// <typeparam name="T">The type to track</typeparam>
        /// <returns>A <see cref="TrackableType{T}"/> instance</returns>
        public static TrackableType<T> ThisType<T>()
        {
            Contract.Ensures(Contract.Result<TrackableType<T>>() != null);

            return new TrackableType<T>();
        }

        public static TrackableType ThisType(Type type)
        {
            return new TrackableType(type);
        }

        public static IImmutableList<TrackableType> ThisTypeRecursive<TRoot>(Func<Type, bool> filter = null)
        {
            return ThisTypeRecursive(typeof(TRoot), filter);
        }

        public static IImmutableList<TrackableType> ThisTypeRecursive(Type rootType, Func<Type, bool> filter = null)
        {
            TrackableType trackableRoot = new TrackableType(rootType);
            List<TrackableType> trackableTypes = null;

            if (filter == null)
            {
                Assembly rootTypeAssembly = rootType.Assembly;
                filter = t => t.Assembly == rootTypeAssembly;
            }

            trackableTypes = new List<TrackableType>
            (
                rootType.GetAllPropertyTypesRecursive(p => filter(p.PropertyType)).Select(t => ThisType(t))
            );

            trackableTypes.Insert(0, new TrackableType(rootType));

            return trackableTypes.ToImmutableList();
        }
    }
}