using System;
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

        /// <summary>
        /// Adds a collection implementation
        /// </summary>
        /// <param name="interfaceType">Collection interface type</param>
        /// <param name="implementationType">Implementation type to given collection interface type</param>
        /// <param name="collectionChangeInterceptor">Collection change interceptor</param>
        void AddImplementation(Type interfaceType, Type implementationType, Type collectionChangeInterceptor);


        /// <summary>
        /// Replaces an existing collection implementation and looks for it by given collection interface type.
        /// </summary>
        /// <param name="interfaceType">Collection interface type</param>
        /// <param name="implementationType">Implementation type to given collection interface type</param>
        /// <param name="collectionChangeInterceptor">Collection change interceptor</param>
        void ReplaceImplementation(Type interfaceType, Type implementationType, Type collectionChangeInterceptor);
    }
}
