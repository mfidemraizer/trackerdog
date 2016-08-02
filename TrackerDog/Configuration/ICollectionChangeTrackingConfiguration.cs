using System;
using System.Collections.Generic;

namespace TrackerDog.Configuration
{
    public interface ICollectionChangeTrackingConfiguration
    {
        bool CanTrack(Type some);
        KeyValuePair<Type, CollectionImplementation> GetImplementation(Type some);
        void AddImplementation(Type interfaceType, Type implementationType, Type collectionChangeInterceptor);
        void ReplaceImplementation(Type interfaceType, Type implementationType, Type collectionChangeInterceptor);
    }
}
