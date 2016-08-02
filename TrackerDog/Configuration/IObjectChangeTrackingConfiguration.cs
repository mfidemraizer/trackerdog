using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;

namespace TrackerDog.Configuration
{
    public interface IObjectChangeTrackingConfiguration
    {
        ICollectionChangeTrackingConfiguration Collections { get; }

        IImmutableSet<ITrackableType> TrackableTypes { get; }

        IObjectChangeTrackingConfiguration TrackThisType<T>(Action<TrackableType<T>> configure = null);
        IObjectChangeTrackingConfiguration TrackThisType(Type type, Action<ITrackableType> configure = null);
        IObjectChangeTrackingConfiguration TrackThisTypeRecursive<TRoot>(Action<ITrackableType> configure = null, Func<Type, bool> filter = null);
        IObjectChangeTrackingConfiguration TrackThisTypeRecursive(Type rootType, Action<ITrackableType> configure = null, Func<Type, bool> filter = null);

        ITrackableType GetTrackableType(Type type);
        IEnumerable<ITrackableType> GetAllTrackableBaseTypes(ITrackableType trackableType);
        bool CanTrackType(Type someType);
        bool ImplementsBaseType(Type someType, out ITrackableType baseType);
        bool CanTrackProperty(PropertyInfo property);

        ITrackableObjectFactory CreateTrackableObjectFactory();
    }
}
