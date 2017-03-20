using System;
using System.Collections;
using System.Collections.Generic;

namespace TrackerDog.Configuration
{
    /// <summary>
    /// Defines collection change-tracking configuration 
    /// </summary>
    public interface ICollectionChangeTrackingConfiguration
    {
        /// <summary>
        /// Determines if a given type is a collection that has been configured to track its changes
        /// </summary>
        /// <param name="some">A collection type</param>
        /// <returns><literal>true</literal> if it can be tracked, <literal>false</literal> if it cannot be tracked</returns>
        bool CanTrack(Type some);

        /// <summary>
        /// Gets configured collection type implementation for a given collection interface
        /// </summary>
        /// <param name="some">A collection interface type</param>
        /// <returns></returns>
        KeyValuePair<Type, CollectionImplementation> GetImplementation(Type some);
        
        void AddOrUpdateImplementation<TInterface, TImplementation, TCollectionChangeInterceptor>()
            where TInterface : IEnumerable
            where TImplementation : class, TInterface
            where TCollectionChangeInterceptor : class;
    }
}
