using System;
using System.Reflection;

namespace TrackerDog
{
    internal interface ITrackableObjectFactoryInternal
    {
        object Create(object some = null, Type typeToTrack = null, ObjectChangeTracker reusedTracker = null, object parentObject = null, PropertyInfo propertyToSet = null, object[] constructorArguments = null);
        object CreateForCollection(object some, IChangeTrackableObject parentObject, PropertyInfo parentObjectProperty);
    }
}