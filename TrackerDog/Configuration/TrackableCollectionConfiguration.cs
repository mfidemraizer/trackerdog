namespace TrackerDog.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using TrackerDog.CollectionHandling;

    /// <summary>
    /// Represents a configuration to setup how collection change tracking will behave.
    /// </summary>
    public sealed class TrackableCollectionConfiguration
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public TrackableCollectionConfiguration()
        {
            DefaultCollectionTrackingHandler defaultHandler = new DefaultCollectionTrackingHandler();
            SetTrackingHandler setHandler = new SetTrackingHandler();

            AddImplementation(typeof(ISet<>), typeof(HashSet<>), setHandler);
            AddImplementation(typeof(IList<>), typeof(List<>), defaultHandler);
            AddImplementation(typeof(ICollection<>), typeof(List<>), defaultHandler);
            AddImplementation(typeof(IEnumerable<>), typeof(List<>), defaultHandler);
        }

        /// <summary>
        /// Gets a dictionary of implementations to common collection interfaces.
        /// </summary>
        internal Dictionary<Type, CollectionImplementation> Implementations { get; } = new Dictionary<Type, CollectionImplementation>();

        /// <summary>
        /// Gets the implementation of a given type. The given type can or cannot be a collection interface, but
        /// it can be also an actual collection type. This method will return the most appropiate generic collection
        /// implementation to the given type.
        /// </summary>
        /// <param name="some">The whole collection type</param>
        /// <returns>A pair, where the key is the collection interface and value is the collection implementation</returns>
        public KeyValuePair<Type, CollectionImplementation> GetImplementation(Type some)
        {
            Contract.Requires(some != null, "A non-null reference to a type is mandatory to get its implementation");
            Contract.Ensures(Contract.Result<KeyValuePair<Type, CollectionImplementation>>().Key != null);

            Type someGenericTypeDefinition = some.IsGenericType && !some.IsGenericTypeDefinition ? some.GetGenericTypeDefinition() : null;
            IEnumerable<Type> someInterfaces = some.GetInterfaces();

            KeyValuePair<Type, CollectionImplementation> result = Implementations.FirstOrDefault
            (
                interfaceType =>
                    someGenericTypeDefinition != null && someGenericTypeDefinition == interfaceType.Key
                    ||
                    someInterfaces.Any
                    (
                        i => i.IsGenericType
                            && i.GetGenericTypeDefinition() == interfaceType.Key
                    )
            );

            if (result.Key != null)
                return result;
            else
                throw new InvalidOperationException($"No implementation found to '{some.AssemblyQualifiedName}'");
        }

        /// <summary>
        /// Adds a new implementation to some collection interface. This method will not support replacing
        /// an already added interface/implementation pair.
        /// </summary>
        /// <param name="interfaceType">The collection interface</param>
        /// <param name="implementationType">The collection implementation</param>
        /// <param name="trackingHandler">A tracking handler to specify how collection changes will be handled</param>
        public void AddImplementation(Type interfaceType, Type implementationType, ICollectionTrackingHandler trackingHandler)
        {
            Contract.Requires(interfaceType != null, "Cannot add an implementation of a null interface");
            Contract.Requires(interfaceType.IsInterface, "Given type must be an interface");
            Contract.Requires(interfaceType.IsGenericTypeDefinition, "Given collection interface must be provided as a generic type definition");
            Contract.Requires(implementationType != null, "Given collection implementation cannot be a null reference");
            Contract.Requires(implementationType.IsClass && !implementationType.IsAbstract, "Given collection implementation must be a non-abstract class");
            Contract.Requires(implementationType.IsGenericTypeDefinition, "Given collection implementation must be a generic type definition");
            Contract.Requires(trackingHandler != null);

            Contract.Assert(!Implementations.ContainsKey(interfaceType), "Adding an implementation can be done once");

            Implementations.Add(interfaceType, new CollectionImplementation(implementationType, trackingHandler));
        }

        /// <summary>
        /// Replaces an existing collection interface/implementation or adds it.
        /// </summary>
        /// <param name="interfaceType"></param>
        /// <param name="implementationType"></param>
        /// <param name="trackingHandler">A tracking handler to specify how collection changes will be handled</param>
        public void ReplaceImplementation(Type interfaceType, Type implementationType, ICollectionTrackingHandler trackingHandler)
        {
            Contract.Requires(interfaceType != null, "Cannot add an implementation of a null interface");
            Contract.Requires(interfaceType.IsInterface, "Given type must be an interface");
            Contract.Requires(interfaceType.IsGenericTypeDefinition, "Given collection interface must be provided as a generic type definition");
            Contract.Requires(implementationType != null, "Given collection implementation cannot be a null reference");
            Contract.Requires(implementationType.IsClass && !implementationType.IsAbstract, "Given collection implementation must be a non-abstract class");
            Contract.Requires(implementationType.IsGenericTypeDefinition, "Given collection implementation must be a generic type definition");
            Contract.Requires(trackingHandler != null);

            if (Implementations.ContainsKey(interfaceType))
                Implementations[interfaceType] = new CollectionImplementation(implementationType, trackingHandler);
            else
                AddImplementation(interfaceType, implementationType, trackingHandler);
        }
    }
}